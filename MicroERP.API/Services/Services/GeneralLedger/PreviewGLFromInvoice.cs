using MicroERP.API.Helper;
using MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroERP.API.Services.GeneralLedger;

public static class PreviewGLFromInvoiceHelper
{
    /// <summary>
    /// Preview GL entries from Invoice - Each accounting entry is categorized by source
    /// 
    /// ACCOUNTING ENTRIES CREATED:
    /// ============================
    /// 
    /// FOR SALE INVOICES:
    /// 1. AR Entry (Debit AR) - Source: Invoice.AccountId -> Party.AccountId -> InterfaceType('ACCOUNTS RECEIVABLE')
    /// 2. Revenue Entries (Credit Revenue, one per item) - Source: Items.RevenueAccountId -> Invoice.AccountId -> InterfaceType('MANUAL ITEM REVENUE')
    /// 3. Tax Entry (Credit Tax) - Source: InterfaceType('TAX')
    /// 4. Service Charges Entry (Credit Service) - Source: InterfaceType('SERVICE')
    /// 5. Discount Entry (Debit Discount) - Source: InterfaceType('DISCOUNT')
    /// 6. Payment Entries (if payments exist):
    ///    - Debit Cash/Bank - Source: InterfaceType('PAYMENT METHOD')
    ///    - Credit AR - Source: Same AR account from Entry 1
    /// 
    /// FOR PURCHASE INVOICES:
    /// 1. AP Entry (Credit AP) - Source: Invoice.AccountId -> Party.AccountId -> InterfaceType('ACCOUNTS PAYABLE')
    /// 2. Expense Entries (Debit Expense, one per item) - Source: Items.ExpenseAccountId -> Invoice.AccountId -> InterfaceType('MANUAL ITEM EXPENSE')
    /// 3. Tax Entry (Debit Tax) - Source: InterfaceType('TAX')
    /// 4. Service Charges Entry (Debit Service) - Source: InterfaceType('SERVICE')
    /// 5. Discount Entry (Credit Discount) - Source: InterfaceType('DISCOUNT')
    /// 6. Payment Entries (if payments exist):
    ///    - Debit AP - Source: Same AP account from Entry 1
    ///    - Credit Cash/Bank - Source: InterfaceType('PAYMENT METHOD')
    /// </summary>
    public static async Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromInvoice(InvoicesModel invoice)
    {
        try
        {
            if (invoice == null || invoice.Invoice == null)
            {
                string errorMsg = "PreviewGLFromInvoice: Invoice model is null";
                Log.Warning($"PreviewGLFromInvoice: {errorMsg}");
                return (false, null, errorMsg);
            }

            DapperFunctions dapper = new DapperFunctions();
            
            // If invoice is already posted, return the actual GL entry
            if (Convert.ToInt32(invoice.Invoice.Id) > 0 && Convert.ToInt32(invoice.Invoice.IsPostedToGL) == 1 && !string.IsNullOrWhiteSpace(invoice.Invoice.GLEntryNo))
            {
                // Get the actual posted GL entry by EntryNo
                string sqlPostedEntry = $@"SELECT glh.*, p.Name as PartyName, loc.Name as LocationName
                                          FROM GeneralLedgerHeader as glh
                                          LEFT JOIN Parties as p on p.Id=glh.PartyId
                                          LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                                          WHERE glh.EntryNo = '{invoice.Invoice.GLEntryNo}' AND glh.IsSoftDeleted=0";
                
                var postedHeader = (await dapper.SearchByQuery<GeneralLedgerHeaderModel>(sqlPostedEntry))?.FirstOrDefault();
                
                if (postedHeader != null && postedHeader.Id > 0)
                {
                    // Get details for the posted entry (include coa.InterfaceType for journal sorting)
                    string sqlPostedDetails = $@"SELECT gld.*, coa.Code as AccountCode, coa.Name as AccountName, coa.Type as AccountType, coa.InterfaceType,
                                                p.Name as PartyName, loc.Name as LocationName
                                                FROM GeneralLedgerDetail as gld
                                                LEFT JOIN ChartOfAccounts as coa on coa.Id=gld.AccountId
                                                LEFT JOIN Parties as p on p.Id=gld.PartyId
                                                LEFT JOIN Locations as loc on loc.Id=gld.LocationId
                                                WHERE gld.HeaderId={postedHeader.Id} AND gld.IsSoftDeleted=0
                                                ORDER BY gld.SeqNo, gld.Id";
                    
                    var postedDetails = await dapper.SearchByQuery<GeneralLedgerDetailModel>(sqlPostedDetails);
                    postedHeader.Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(postedDetails ?? new List<GeneralLedgerDetailModel>());
                    
                    return (true, postedHeader, "");
                }
            }
            
            // Create preview GL header (not saved)
            GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
            {
                OrganizationId = Convert.ToInt32(invoice.Invoice.OrganizationId),
                EntryNo = "PREVIEW",
                EntryDate = invoice.Invoice.TranDate ?? DateTime.Now,
                Source = "INVOICE",
                Description = $"Invoice {invoice.Invoice.Code ?? "NEW"}",
                ReferenceNo = invoice.Invoice.Code ?? "NEW",
                ReferenceType = "INVOICE",
                ReferenceId = Convert.ToInt32(invoice.Invoice.Id),
                PartyId = Convert.ToInt32(invoice.Invoice.PartyId),
                LocationId = Convert.ToInt32(invoice.Invoice.LocationId),
                BaseCurrencyId = Convert.ToInt32(invoice.Invoice.BaseCurrencyId),
                EnteredCurrencyId = Convert.ToInt32(invoice.Invoice.EnteredCurrencyId),
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(),
                Warnings = new List<string>()
            };

            bool isSale = invoice.Invoice.InvoiceType?.ToUpper() == "SALE";

            // When invoice is saved, fetch totals and AccountId from DB (same as CreateGLFromInvoice) so preview matches posted journal
            int invIdForTotals = Convert.ToInt32(invoice.Invoice.Id);
            if (invIdForTotals > 0)
            {
                string totalsSql = $@"
                    SELECT 
                        (SELECT ISNULL(SUM((id.Qty * id.UnitPrice) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0)), 0) FROM InvoiceDetail id
                         LEFT JOIN (SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax GROUP BY InvoiceDetailId) tax ON id.Id = tax.InvoiceDetailId
                         WHERE id.InvoiceId = {invIdForTotals} AND id.IsSoftDeleted = 0) AS InvoiceAmount,
                        (SELECT ISNULL(SUM(idt.TaxAmount), 0) FROM InvoiceDetailTax idt INNER JOIN InvoiceDetail id ON idt.InvoiceDetailId = id.Id WHERE id.InvoiceId = {invIdForTotals} AND id.IsSoftDeleted = 0) AS TaxAmount,
                        (SELECT ISNULL(SUM(AppliedAmount), 0) FROM InvoiceCharges WHERE InvoiceId = {invIdForTotals} AND ChargeCategory = 'SERVICE' AND IsSoftDeleted = 0) AS ChargesAmount,
                        (SELECT ISNULL(SUM(AppliedAmount), 0) FROM InvoiceCharges WHERE InvoiceId = {invIdForTotals} AND ChargeCategory = 'DISCOUNT' AND IsSoftDeleted = 0) AS DiscountAmount";
                var totalsRow = (await dapper.SearchByQuery<InvoiceTotalsRow>(totalsSql))?.FirstOrDefault();
                if (totalsRow != null)
                {
                    invoice.Invoice.InvoiceAmount = totalsRow.InvoiceAmount;
                    invoice.Invoice.TaxAmount = totalsRow.TaxAmount;
                    invoice.Invoice.ChargesAmount = totalsRow.ChargesAmount;
                    invoice.Invoice.DiscountAmount = totalsRow.DiscountAmount;
                    invoice.Invoice.TotalInvoiceAmount = invoice.Invoice.InvoiceAmount + invoice.Invoice.ChargesAmount - invoice.Invoice.DiscountAmount;
                }
                // Use AR/AP account from DB so preview matches post (Post uses DB; client-sent AccountId can differ)
                var dbInvoiceRow = (await dapper.SearchByQuery<dynamic>($"SELECT AccountId, PartyId FROM Invoice WHERE Id = {invIdForTotals}"))?.FirstOrDefault();
                if (dbInvoiceRow != null)
                {
                    if (dbInvoiceRow.AccountId != null && !Convert.IsDBNull(dbInvoiceRow.AccountId) && Convert.ToInt32(dbInvoiceRow.AccountId) > 0)
                        invoice.Invoice.AccountId = Convert.ToInt32(dbInvoiceRow.AccountId);
                    if (dbInvoiceRow.PartyId != null && !Convert.IsDBNull(dbInvoiceRow.PartyId) && Convert.ToInt32(dbInvoiceRow.PartyId) > 0)
                        invoice.Invoice.PartyId = Convert.ToInt32(dbInvoiceRow.PartyId);
                }
            }

            // ====================================================================================
            // STEP 1: LOOKUP AR/AP ACCOUNT (same as Post: Invoice or Party with correct AR/AP type; no default pick)
            // ====================================================================================
            var arApResult = await GLAccountLookupHelper.GetARAPAccount(
                dapper,
                Convert.ToInt32(invoice.Invoice.OrganizationId),
                isSale,
                Convert.ToInt32(invoice.Invoice.AccountId) > 0 ? Convert.ToInt32(invoice.Invoice.AccountId) : (int?)null,
                Convert.ToInt32(invoice.Invoice.PartyId) > 0 ? Convert.ToInt32(invoice.Invoice.PartyId) : (int?)null);
            
            if (!arApResult.Item1)
            {
                Log.Warning($"PreviewGLFromInvoice: {arApResult.Item3}");
                return (false, null, arApResult.Item3);
            }
            ChartOfAccountsModel arApAccount = arApResult.Item2!;

            // ====================================================================================
            // CREATE ACCOUNTING ENTRIES BASED ON INVOICE TYPE (same order and logic as CreateGLFromInvoice)
            // ====================================================================================
            // For SALE: Debit AR, Credit Revenue (per line, scaled to InvoiceAmount), Credit Tax, Credit Service Charges, Debit Discount
            // For PURCHASE: Credit AP, Debit Expense (per line, scaled to InvoiceAmount), Debit Tax, Debit Service Charges, Credit Discount
            // Payment entries are NOT included so preview matches what gets posted (CreateGLFromInvoice does not post payments)
            if (isSale)
            {
                // ====================================================================================
                // ACCOUNTING ENTRY 1: AR/AP ENTRY (SALE - Debit AR)
                // Use TotalInvoiceAmount (amount customer owes) so receivable matches credits and journal balances.
                // Fallback to component sum when TotalInvoiceAmount is zero (e.g. unsaved invoice).
                // ====================================================================================
                decimal arAmount = invoice.Invoice.TotalInvoiceAmount > 0
                    ? invoice.Invoice.TotalInvoiceAmount
                    : (invoice.Invoice.InvoiceAmount + invoice.Invoice.ChargesAmount + invoice.Invoice.TaxAmount - invoice.Invoice.DiscountAmount);
                if (arApAccount != null && arAmount > 0)
                {
                    GLAccountLookupHelper.AddARAPEntry(glHeader, arApAccount, arAmount, isSale, invoice.Invoice.Code ?? "NEW", Convert.ToInt32(invoice.Invoice.PartyId) > 0 ? Convert.ToInt32(invoice.Invoice.PartyId) : (int?)null);
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 2: REVENUE ENTRIES (SALE - Credit Revenue, one per invoice item)
                // Source Priority: Items.RevenueAccountId -> Invoice.AccountId -> ChartOfAccounts.InterfaceType('MANUAL ITEM REVENUE')
                // ====================================================================================
                if (Convert.ToInt32(invoice.Invoice.Id) > 0)
                {
                    // Get invoice items from database (same columns and formula as CreateGLFromInvoice)
                    int invId = Convert.ToInt32(invoice.Invoice.Id);
                    string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.AccountId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                      ii.ManualItem,
                                                      i.Code as ItemCode, COALESCE(i.Name, ii.ManualItem) as ItemName,
                                                      i.RevenueAccountId,
                                                      COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount, 0) as Amount
                                                FROM InvoiceDetail ii
                                                LEFT JOIN Items i ON ii.ItemId = i.Id
                                                WHERE ii.InvoiceId = {invId} AND ii.IsSoftDeleted = 0
                                                ORDER BY ii.Id";
                    var invoiceItems = await dapper.SearchByQuery<PreviewInvoiceLineRow>(sqlInvoiceItems);
                    
                    if (invoiceItems != null && invoiceItems.Any())
                    {
                        var positiveItems = invoiceItems.Where(i => i.Amount > 0).ToList();
                        decimal sumLineAmounts = positiveItems.Sum(i => i.Amount);
                        // Use same target as Create: InvoiceAmount from DB so scaling matches posted journal
                        decimal targetRevenueAmount = invoice.Invoice.InvoiceAmount > 0 ? invoice.Invoice.InvoiceAmount : sumLineAmounts;

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetRevenueAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetRevenueAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var invoiceItem = positiveItems[idx];

                            decimal rawLineAmount = invoiceItem.Amount;
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                            {
                                lineAmount = rawLineAmount;
                            }
                            else
                            {
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetRevenueAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                allocated += lineAmount;
                            }

                            // Account resolution: same order as CreateGLFromInvoice — Detail AccountId -> Item RevenueAccountId -> Invoice AccountId -> MANUAL ITEM REVENUE
                            ChartOfAccountsModel? revenueAccount = null;
                            int? detailAccountId = invoiceItem.AccountId;
                            if (detailAccountId.HasValue && detailAccountId.Value > 0)
                            {
                                revenueAccount = await GLAccountLookupHelper.GetAccountById(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), detailAccountId.Value);
                            }
                            if (revenueAccount == null || revenueAccount.Id == 0)
                            {
                                bool isManualItem = (invoiceItem.ItemId == null || invoiceItem.ItemId == 0) && !string.IsNullOrWhiteSpace(invoiceItem.ManualItem);
                                int? itemId = isManualItem ? null : (invoiceItem.ItemId > 0 ? invoiceItem.ItemId : null);
                                int? itemRevenueAccountId = isManualItem ? null : (invoiceItem.RevenueAccountId > 0 ? invoiceItem.RevenueAccountId : null);
                                var revenueResult = await GLAccountLookupHelper.GetRevenueAccount(
                                    dapper,
                                    Convert.ToInt32(invoice.Invoice.OrganizationId),
                                    itemId,
                                    itemRevenueAccountId,
                                    Convert.ToInt32(invoice.Invoice.AccountId) > 0 ? (int?)Convert.ToInt32(invoice.Invoice.AccountId) : null,
                                    invoiceItem.ItemName);
                                if (revenueResult.Item1 && revenueResult.Item2 != null)
                                    revenueAccount = revenueResult.Item2;
                            }
                            if (revenueAccount == null || revenueAccount.Id == 0)
                            {
                                string warningMsg = $"Revenue Account Missing for item '{invoiceItem.ItemName ?? "Unknown"}'.";
                                glHeader.Warnings.Add(warningMsg);
                                Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                                revenueAccount = new ChartOfAccountsModel
                                {
                                    Id = 0,
                                    Code = "MISSING",
                                    Name = "REVENUE ACCOUNT (NOT CONFIGURED)",
                                    Type = "REVENUE"
                                };
                            }
                            GLAccountLookupHelper.AddRevenueEntry(glHeader, revenueAccount, lineAmount, invoiceItem.ItemName ?? "Manual Item", invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }
                else
                {
                    // Handle unsaved invoices (preview mode) - use InvoiceDetails from in-memory collection
                    // Revenue = sum of (Qty*UnitPrice - Discount) per line (pre-tax); use line sum so journal balances
                    if (invoice.InvoiceDetails != null && invoice.InvoiceDetails.Any())
                    {
                        var positiveItems = invoice.InvoiceDetails
                            .Select(i => new { Item = i, RevenueAmount = (decimal)((i.Qty * i.UnitPrice) - i.DiscountAmount) })
                            .Where(x => x.RevenueAmount > 0)
                            .ToList();

                        decimal sumLineAmounts = positiveItems.Sum(x => x.RevenueAmount);
                        // Target = line sum so total revenue matches actual lines; ensures DR = CR
                        decimal targetRevenueAmount = sumLineAmounts > 0 ? sumLineAmounts : invoice.Invoice.InvoiceAmount;

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetRevenueAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetRevenueAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var itemData = positiveItems[idx];
                            var invoiceItem = itemData.Item;

                            decimal rawLineAmount = itemData.RevenueAmount;
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                            {
                                lineAmount = rawLineAmount;
                            }
                            else
                            {
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetRevenueAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                allocated += lineAmount;
                            }

                            // Get Revenue Account using common helper
                            // Check if this is a manual item (ItemId is 0 and ManualItem is not empty)
                            bool isManualItem = (invoiceItem.ItemId == 0 || invoiceItem.ItemId == null) 
                                && !string.IsNullOrWhiteSpace(invoiceItem.ManualItem);
                            
                            var revenueResult = await GLAccountLookupHelper.GetRevenueAccount(
                                dapper,
                                Convert.ToInt32(invoice.Invoice.OrganizationId),
                                isManualItem ? null : (invoiceItem.ItemId > 0 ? invoiceItem.ItemId : null), // Pass null for manual items
                                null,
                                isManualItem ? null : (Convert.ToInt32(invoice.Invoice.AccountId) > 0 ? (int?)Convert.ToInt32(invoice.Invoice.AccountId) : null),
                                invoiceItem.ItemName);
                            
                            ChartOfAccountsModel? revenueAccount = null;
                            if (!revenueResult.Item1)
                            {
                                string warningMsg = $"Revenue Account Missing for item '{invoiceItem.ItemName}': {revenueResult.Item3}";
                                glHeader.Warnings.Add(warningMsg);
                                Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                                // Create placeholder account
                                revenueAccount = new ChartOfAccountsModel
                                {
                                    Id = 0,
                                    Code = "MISSING",
                                    Name = "REVENUE ACCOUNT (NOT CONFIGURED)",
                                    Type = "REVENUE"
                                };
                            }
                            else
                            {
                                revenueAccount = revenueResult.Item2!;
                            }
                            
                            GLAccountLookupHelper.AddRevenueEntry(glHeader, revenueAccount, lineAmount, invoiceItem.ItemName, invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 3: TAX (SALE - Credit Tax) — single consolidated entry to match CreateGLFromInvoice
                // ====================================================================================
                if (invoice.Invoice.TaxAmount > 0)
                {
                    var taxAccount = await GLAccountLookupHelper.GetTaxAccount(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), required: false);
                    if (taxAccount != null && taxAccount.Id > 0)
                        GLAccountLookupHelper.AddTaxEntry(glHeader, taxAccount, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    else
                    {
                        glHeader.Warnings.Add("Tax account (InterfaceType='TAX') not configured. Tax entry will be skipped.");
                        GLAccountLookupHelper.AddTaxEntry(glHeader, new ChartOfAccountsModel { Id = 0, Code = "MISSING", Name = "TAX ACCOUNT (NOT CONFIGURED)", Type = "LIABILITY" }, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 4: SERVICE CHARGES ENTRY (SALE - Credit Service Charges)
                // Source: ChartOfAccounts.InterfaceType = 'SERVICE'
                // ====================================================================================
                if (invoice.Invoice.ChargesAmount > 0)
                {
                    var chargeAccount = await GLAccountLookupHelper.GetServiceAccount(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), required: false);
                    
                    if (chargeAccount != null && chargeAccount.Id > 0)
                    {
                        GLAccountLookupHelper.AddServiceChargesEntry(glHeader, chargeAccount, invoice.Invoice.ChargesAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                    else
                    {
                        string warningMsg = $"Service Charges account not found. Invoice has ChargesAmount of {invoice.Invoice.ChargesAmount:N2}, but no account with InterfaceType='SERVICE' is configured. Service charges entry will be skipped.";
                        glHeader.Warnings.Add(warningMsg);
                        Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                        // Create placeholder entry for missing service account
                        var placeholderServiceAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "SERVICE CHARGES ACCOUNT (NOT CONFIGURED)",
                            Type = "REVENUE"
                        };
                        GLAccountLookupHelper.AddServiceChargesEntry(glHeader, placeholderServiceAccount, invoice.Invoice.ChargesAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 5: DISCOUNT ENTRY (SALE - Debit Discount)
                // Source: ChartOfAccounts.InterfaceType = 'DISCOUNT'
                // ====================================================================================
                if (invoice.Invoice.DiscountAmount > 0)
                {
                    var discountAccount = await GLAccountLookupHelper.GetDiscountAccount(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), required: false);
                    if (discountAccount != null && discountAccount.Id > 0)
                    {
                        GLAccountLookupHelper.AddDiscountEntry(glHeader, discountAccount, invoice.Invoice.DiscountAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                    else
                    {
                        string warningMsg = $"Discount account not found. Invoice has DiscountAmount of {invoice.Invoice.DiscountAmount:N2}, but no account with InterfaceType='DISCOUNT' is configured. Discount entry will be skipped.";
                        glHeader.Warnings.Add(warningMsg);
                        Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                        // Create placeholder entry for missing discount account
                        var placeholderDiscountAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "DISCOUNT ACCOUNT (NOT CONFIGURED)",
                            Type = "EXPENSE"
                        };
                        GLAccountLookupHelper.AddDiscountEntry(glHeader, placeholderDiscountAccount, invoice.Invoice.DiscountAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }
            }
            else
            {
                // ====================================================================================
                // ACCOUNTING ENTRY 1: AR/AP ENTRY (PURCHASE - Credit AP)
                // Use TotalInvoiceAmount so payable matches debits and journal balances. Fallback to component sum.
                // ====================================================================================
                decimal apAmount = invoice.Invoice.TotalInvoiceAmount > 0
                    ? invoice.Invoice.TotalInvoiceAmount
                    : (invoice.Invoice.InvoiceAmount + invoice.Invoice.ChargesAmount + invoice.Invoice.TaxAmount - invoice.Invoice.DiscountAmount);
                if (arApAccount != null && apAmount > 0)
                {
                    GLAccountLookupHelper.AddARAPEntry(glHeader, arApAccount, apAmount, isSale, invoice.Invoice.Code ?? "NEW", Convert.ToInt32(invoice.Invoice.PartyId) > 0 ? Convert.ToInt32(invoice.Invoice.PartyId) : (int?)null);
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 2: EXPENSE ENTRIES (PURCHASE - Debit Expense, one per invoice item)
                // Source Priority: Items.ExpenseAccountId -> Invoice.AccountId -> ChartOfAccounts.InterfaceType('MANUAL ITEM EXPENSE')
                // ====================================================================================
                if (Convert.ToInt32(invoice.Invoice.Id) > 0)
                {
                    int invIdPurchase = Convert.ToInt32(invoice.Invoice.Id);
                    string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.AccountId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                      ii.ManualItem,
                                                      i.Code as ItemCode, COALESCE(i.Name, ii.ManualItem) as ItemName,
                                                      i.ExpenseAccountId,
                                                      COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount, 0) as Amount
                                                FROM InvoiceDetail ii
                                                LEFT JOIN Items i ON ii.ItemId = i.Id
                                                WHERE ii.InvoiceId = {invIdPurchase} AND ii.IsSoftDeleted = 0
                                                ORDER BY ii.Id";
                    var invoiceItems = await dapper.SearchByQuery<PreviewInvoiceLineRow>(sqlInvoiceItems);
                    
                    if (invoiceItems != null && invoiceItems.Any())
                    {
                        var positiveItems = invoiceItems.Where(i => i.Amount > 0).ToList();
                        decimal sumLineAmounts = positiveItems.Sum(i => i.Amount);
                        decimal targetExpenseAmount = invoice.Invoice.InvoiceAmount > 0 ? invoice.Invoice.InvoiceAmount : sumLineAmounts;

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetExpenseAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetExpenseAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var invoiceItem = positiveItems[idx];

                            decimal rawLineAmount = invoiceItem.Amount;
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                                lineAmount = rawLineAmount;
                            else
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetExpenseAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                            allocated += lineAmount;

                            ChartOfAccountsModel? expenseAccount = null;
                            int? detailAccountId = invoiceItem.AccountId;
                            if (detailAccountId.HasValue && detailAccountId.Value > 0)
                                expenseAccount = await GLAccountLookupHelper.GetAccountById(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), detailAccountId.Value);
                            if (expenseAccount == null || expenseAccount.Id == 0)
                            {
                                bool isManualItem = (invoiceItem.ItemId == null || invoiceItem.ItemId == 0) && !string.IsNullOrWhiteSpace(invoiceItem.ManualItem);
                                int? itemId = isManualItem ? null : (invoiceItem.ItemId > 0 ? invoiceItem.ItemId : null);
                                int? itemExpenseAccountId = isManualItem ? null : (invoiceItem.ExpenseAccountId > 0 ? invoiceItem.ExpenseAccountId : null);
                                var expenseResult = await GLAccountLookupHelper.GetExpenseAccount(
                                    dapper,
                                    Convert.ToInt32(invoice.Invoice.OrganizationId),
                                    itemId,
                                    itemExpenseAccountId,
                                    Convert.ToInt32(invoice.Invoice.AccountId) > 0 ? (int?)Convert.ToInt32(invoice.Invoice.AccountId) : null,
                                    invoiceItem.ItemName);
                                if (expenseResult.Item1 && expenseResult.Item2 != null)
                                    expenseAccount = expenseResult.Item2;
                            }
                            if (expenseAccount == null || expenseAccount.Id == 0)
                            {
                                glHeader.Warnings.Add($"Expense Account Missing for item '{invoiceItem.ItemName ?? "Unknown"}'.");
                                expenseAccount = new ChartOfAccountsModel
                                {
                                    Id = 0,
                                    Code = "MISSING",
                                    Name = "EXPENSE ACCOUNT (NOT CONFIGURED)",
                                    Type = "EXPENSE"
                                };
                            }
                            GLAccountLookupHelper.AddExpenseEntry(glHeader, expenseAccount, lineAmount, invoiceItem.ItemName ?? "Manual Item", invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }
                else
                {
                    // Handle unsaved invoices (preview mode) - use InvoiceDetails from in-memory collection
                    if (invoice.InvoiceDetails != null && invoice.InvoiceDetails.Any())
                    {
                        // Expense = sum of (Qty*UnitPrice - Discount) per line (pre-tax); use line sum so journal balances
                        var positiveItems = invoice.InvoiceDetails
                            .Select(i => new
                            {
                                Item = i,
                                ExpenseAmount = (decimal)((i.Qty * i.UnitPrice) - i.DiscountAmount)
                            })
                            .Where(x => x.ExpenseAmount > 0)
                            .ToList();

                        decimal sumLineAmounts = positiveItems.Sum(x => x.ExpenseAmount);
                        // Target = line sum so total expense matches actual lines; ensures DR = CR
                        decimal targetExpenseAmount = sumLineAmounts > 0 ? sumLineAmounts : invoice.Invoice.InvoiceAmount;

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetExpenseAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetExpenseAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var itemData = positiveItems[idx];
                            var invoiceItem = itemData.Item;

                            decimal rawLineAmount = itemData.ExpenseAmount;
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                            {
                                lineAmount = rawLineAmount;
                            }
                            else
                            {
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetExpenseAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                allocated += lineAmount;
                            }

                            // Get Expense Account using common helper
                            // Check if this is a manual item (ItemId is 0 and ManualItem is not empty)
                            bool isManualItem = (invoiceItem.ItemId == 0 || invoiceItem.ItemId == null) 
                                && !string.IsNullOrWhiteSpace(invoiceItem.ManualItem);
                            
                            int? itemIdForExpense = isManualItem ? null : (invoiceItem.ItemId > 0 ? invoiceItem.ItemId : null); // Pass null for manual items
                            var expenseResult = await GLAccountLookupHelper.GetExpenseAccount(
                                dapper,
                                Convert.ToInt32(invoice.Invoice.OrganizationId),
                                itemIdForExpense,
                                null,
                                Convert.ToInt32(invoice.Invoice.AccountId) > 0 ? (int?)Convert.ToInt32(invoice.Invoice.AccountId) : null,
                                invoiceItem.ItemName);
                            
                            ChartOfAccountsModel? expenseAccount = null;
                            if (!expenseResult.Item1)
                            {
                                string warningMsg = $"Expense Account Missing for item '{invoiceItem.ItemName}': {expenseResult.Item3}";
                                glHeader.Warnings.Add(warningMsg);
                                Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                                // Create placeholder account
                                expenseAccount = new ChartOfAccountsModel
                                {
                                    Id = 0,
                                    Code = "MISSING",
                                    Name = "EXPENSE ACCOUNT (NOT CONFIGURED)",
                                    Type = "EXPENSE"
                                };
                            }
                            else
                            {
                                expenseAccount = expenseResult.Item2!;
                            }
                            
                            GLAccountLookupHelper.AddExpenseEntry(glHeader, expenseAccount, lineAmount, invoiceItem.ItemName, invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 3: TAX (PURCHASE - Debit Tax) — single consolidated entry to match CreateGLFromInvoice
                // ====================================================================================
                if (invoice.Invoice.TaxAmount > 0)
                {
                    var taxAccount = await GLAccountLookupHelper.GetTaxAccount(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), required: false);
                    if (taxAccount != null && taxAccount.Id > 0)
                        GLAccountLookupHelper.AddTaxEntry(glHeader, taxAccount, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    else
                    {
                        glHeader.Warnings.Add("Tax account (InterfaceType='TAX') not configured. Tax entry will be skipped.");
                        GLAccountLookupHelper.AddTaxEntry(glHeader, new ChartOfAccountsModel { Id = 0, Code = "MISSING", Name = "TAX ACCOUNT (NOT CONFIGURED)", Type = "LIABILITY" }, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 4: SERVICE CHARGES ENTRY (PURCHASE - Debit Service Charges)
                // Source: ChartOfAccounts.InterfaceType = 'SERVICE'
                // ====================================================================================
                if (invoice.Invoice.ChargesAmount > 0)
                {
                    var chargeAccount = await GLAccountLookupHelper.GetServiceAccount(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), required: false);
                    if (chargeAccount != null && chargeAccount.Id > 0)
                    {
                        GLAccountLookupHelper.AddServiceChargesEntry(glHeader, chargeAccount, invoice.Invoice.ChargesAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                    else
                    {
                        string warningMsg = $"Service Charges account not found. Invoice has ChargesAmount of {invoice.Invoice.ChargesAmount:N2}, but no account with InterfaceType='SERVICE' is configured. Service charges entry will be skipped.";
                        glHeader.Warnings.Add(warningMsg);
                        Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                        // Create placeholder entry for missing service account
                        var placeholderServiceAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "SERVICE CHARGES ACCOUNT (NOT CONFIGURED)",
                            Type = "EXPENSE"
                        };
                        GLAccountLookupHelper.AddServiceChargesEntry(glHeader, placeholderServiceAccount, invoice.Invoice.ChargesAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 5: DISCOUNT ENTRY (PURCHASE - Credit Discount)
                // Source: ChartOfAccounts.InterfaceType = 'DISCOUNT'
                // ====================================================================================
                if (invoice.Invoice.DiscountAmount > 0)
                {
                    var discountAccount = await GLAccountLookupHelper.GetDiscountAccount(dapper, Convert.ToInt32(invoice.Invoice.OrganizationId), required: false);
                    if (discountAccount != null && discountAccount.Id > 0)
                    {
                        GLAccountLookupHelper.AddDiscountEntry(glHeader, discountAccount, invoice.Invoice.DiscountAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                    else
                    {
                        string warningMsg = $"Discount account not found. Invoice has DiscountAmount of {invoice.Invoice.DiscountAmount:N2}, but no account with InterfaceType='DISCOUNT' is configured. Discount entry will be skipped.";
                        glHeader.Warnings.Add(warningMsg);
                        Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                        // Create placeholder entry for missing discount account
                        var placeholderDiscountAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "DISCOUNT ACCOUNT (NOT CONFIGURED)",
                            Type = "REVENUE"
                        };
                        GLAccountLookupHelper.AddDiscountEntry(glHeader, placeholderDiscountAccount, invoice.Invoice.DiscountAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }
            }

            // ====================================================================================
            // PAYMENT ENTRIES (so user sees full journal including payments in preview)
            // For SALE: Debit Cash/Bank, Credit AR per payment. For PURCHASE: Debit AP, Credit Cash/Bank.
            // ====================================================================================
            if (arApAccount != null && invoice.InvoicePayments != null && invoice.InvoicePayments.Any())
            {
                var paymentsToProcess = invoice.InvoicePayments
                    .Where(p => p.Amount > 0 && p.AccountId > 0)
                    .ToList();

                foreach (var payment in paymentsToProcess)
                {
                    string validateAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                   WHERE Id = {payment.AccountId} 
                                                   AND OrganizationId = {Convert.ToInt32(invoice.Invoice.OrganizationId)} 
                                                   AND InterfaceType = 'PAYMENT METHOD'
                                                   AND IsActive = 1";
                    var paymentAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(validateAccountSql);
                    ChartOfAccountsModel? paymentAccount = paymentAccountResult?.FirstOrDefault();

                    if (paymentAccount == null || paymentAccount.Id == 0)
                    {
                        glHeader.Warnings.Add($"Payment Account (ID: {payment.AccountId}) not found or not InterfaceType='PAYMENT METHOD'. Shown as placeholder.");
                        paymentAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "PAYMENT METHOD (NOT CONFIGURED)",
                            Type = "ASSET"
                        };
                    }

                    GLAccountLookupHelper.AddPaymentEntries(
                        glHeader,
                        arApAccount,
                        paymentAccount,
                        (double)payment.Amount,
                        isSale,
                        invoice.Invoice.Code ?? "NEW",
                        payment.PaymentRef,
                        Convert.ToInt32(invoice.Invoice.PartyId) > 0 ? Convert.ToInt32(invoice.Invoice.PartyId) : (int?)null);
                }
            }

            // Assign SeqNo and JournalGroup so UI can show exactly two journals: 1 = Sale/Purchase invoice, 2 = Payment
            int seqSale = 0;
            int seqPayment = 1000;
            foreach (var d in glHeader.Details ?? new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>())
            {
                bool isPaymentLine = (d.Description ?? "").StartsWith("Payment -", StringComparison.OrdinalIgnoreCase)
                    || (d.Description ?? "").StartsWith("Payment Received -", StringComparison.OrdinalIgnoreCase)
                    || (d.Description ?? "").StartsWith("Payment Made -", StringComparison.OrdinalIgnoreCase);
                if (isPaymentLine)
                {
                    d.JournalGroup = 2;
                    d.SeqNo = ++seqPayment;
                }
                else
                {
                    d.JournalGroup = 1;
                    d.SeqNo = ++seqSale;
                }
            }

            // Calculate totals
            glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
            glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

            // Validate that we have at least one entry
            if (glHeader.Details == null || glHeader.Details.Count == 0)
            {
                string warningMsg = "No GL details generated. Please ensure invoice has items or amounts configured.";
                glHeader.Warnings.Add(warningMsg);
                Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
            }

            // Always return success with warnings (if any) - let UI handle displaying warnings and disabling post button
            string warningSummary = glHeader.Warnings.Any() 
                ? $"Preview generated with {glHeader.Warnings.Count} warning(s). Please review and configure missing accounts before posting." 
                : "";
            return (true, glHeader, warningSummary);
        }
        catch (Exception ex)
        {
            string errorMsg = $"PreviewGLFromInvoice: Error generating preview: {ex.Message}";
            Log.Error(ex, "PreviewGLFromInvoice Error: {Message}", ex.Message);
            return (false, null, errorMsg);
        }
    }

    /// <summary>
    /// Add GL tax entries per tax type (e.g. GST -> GENERAL SALES TAX, Advance Tax -> ADVANCE TAX).
    /// 1) If invoice.InvoiceTaxes has items: group by TaxId (name or id), sum TaxAmount, lookup TaxMaster for AccountId → one line per tax.
    /// 2) Else if saved invoice: get breakdown from InvoiceDetailTax grouped by TaxId.
    /// 3) Else: single tax entry (InterfaceType='TAX').
    /// </summary>
    private static async Task AddTaxEntriesByBreakdown(
        DapperFunctions dapper,
        GeneralLedgerHeaderModel glHeader,
        InvoicesModel invoice,
        bool isSale)
    {
        if (invoice?.Invoice == null || invoice.Invoice.TaxAmount <= 0) return;

        int orgId = Convert.ToInt32(invoice.Invoice.OrganizationId);
        int invoiceId = Convert.ToInt32(invoice.Invoice.Id);
        string code = invoice.Invoice.Code ?? "NEW";

        // 1) Use client-supplied InvoiceTaxes breakdown when present (works for unsaved and invoice-level tax)
        bool taxAddedFromBreakdown = false;
        if (invoice.InvoiceTaxes != null && invoice.InvoiceTaxes.Any())
        {
            var grouped = invoice.InvoiceTaxes
                .Where(t => t.TaxAmount != 0)
                .GroupBy(t => t.TaxId?.ToString().Trim() ?? "")
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new { TaxKey = g.Key, TaxAmount = g.Sum(t => t.TaxAmount) })
                .ToList();
            if (grouped.Any())
            {
                foreach (var g in grouped)
                {
                    decimal amount = (decimal)g.TaxAmount;
                    if (amount <= 0) continue;
                    int accountId = 0;
                    string taxName = g.TaxKey;
                    if (int.TryParse(g.TaxKey, out int taxIdNum))
                    {
                        string sqlTm = $@"SELECT TOP 1 Id, AccountId, TaxName FROM TaxMaster WHERE Id = {taxIdNum} AND OrganizationId = {orgId} AND IsSoftDeleted = 0";
                        var tmRow = (await dapper.SearchByQuery<dynamic>(sqlTm))?.FirstOrDefault();
                        if (tmRow != null)
                        {
                            accountId = Convert.ToInt32(tmRow.AccountId ?? 0);
                            taxName = tmRow.TaxName?.ToString() ?? g.TaxKey;
                        }
                    }
                    if (accountId == 0)
                    {
                        string taxKeyEsc = (g.TaxKey ?? "").Replace("'", "''");
                        string sqlTmByName = $@"SELECT TOP 1 Id, AccountId, TaxName FROM TaxMaster WHERE OrganizationId = {orgId} AND IsSoftDeleted = 0 AND (TaxName = N'{taxKeyEsc}' OR TaxName LIKE N'%' + N'{taxKeyEsc}' + N'%')";
                        var tmRow = (await dapper.SearchByQuery<dynamic>(sqlTmByName))?.FirstOrDefault();
                        if (tmRow != null)
                        {
                            accountId = Convert.ToInt32(tmRow.AccountId ?? 0);
                            taxName = tmRow.TaxName?.ToString() ?? g.TaxKey;
                        }
                    }
                    ChartOfAccountsModel? account = accountId > 0 ? await GLAccountLookupHelper.GetAccountById(dapper, orgId, accountId, required: false) : null;
                    if (account == null || account.Id == 0)
                    {
                        glHeader.Warnings.Add($"Tax account for '{taxName}' not found. Entry will show as missing.");
                        account = new ChartOfAccountsModel { Id = 0, Code = "MISSING", Name = $"TAX - {taxName} (NOT CONFIGURED)", Type = "LIABILITY" };
                    }
                    GLAccountLookupHelper.AddTaxEntry(glHeader, account, amount, isSale, code);
                    taxAddedFromBreakdown = true;
                }
                if (taxAddedFromBreakdown) return;
            }
        }

        // 2) Saved invoice: get tax breakdown from InvoiceDetailTax grouped by TaxId
        if (!taxAddedFromBreakdown && invoiceId > 0)
        {
            string sqlTaxBreakdown = $@"
                SELECT idt.TaxId, tm.AccountId, tm.TaxName, SUM(idt.TaxAmount) AS TaxAmount
                FROM InvoiceDetailTax idt
                INNER JOIN InvoiceDetail id ON id.Id = idt.InvoiceDetailId AND id.InvoiceId = {invoiceId} AND id.IsSoftDeleted = 0
                INNER JOIN TaxMaster tm ON tm.Id = idt.TaxId AND tm.OrganizationId = {orgId} AND tm.IsSoftDeleted = 0
                GROUP BY idt.TaxId, tm.AccountId, tm.TaxName
                HAVING SUM(idt.TaxAmount) <> 0";
            var taxRows = await dapper.SearchByQuery<dynamic>(sqlTaxBreakdown);
            if (taxRows != null && taxRows.Any())
            {
                foreach (var row in taxRows)
                {
                    decimal amount = Convert.ToDecimal(row.TaxAmount ?? 0);
                    if (amount <= 0) continue;
                    int accountId = Convert.ToInt32(row.AccountId ?? 0);
                    string taxName = row.TaxName?.ToString() ?? $"Tax {row.TaxId}";
                    ChartOfAccountsModel? account = await GLAccountLookupHelper.GetAccountById(dapper, orgId, accountId, required: false);
                    if (account == null || account.Id == 0)
                    {
                        glHeader.Warnings.Add($"Tax account (ID: {accountId}) for '{taxName}' not found. Entry will show as missing.");
                        account = new ChartOfAccountsModel { Id = 0, Code = "MISSING", Name = $"TAX - {taxName} (NOT CONFIGURED)", Type = "LIABILITY" };
                    }
                    GLAccountLookupHelper.AddTaxEntry(glHeader, account, amount, isSale, code);
                    taxAddedFromBreakdown = true;
                }
                if (taxAddedFromBreakdown) return;
            }
        }

        // 3) Fallback: always add single tax entry when TaxAmount > 0 and no breakdown was added (ensures journal balances)
        if (!taxAddedFromBreakdown && invoice.Invoice.TaxAmount > 0)
        {
            ChartOfAccountsModel? taxAccount = await GLAccountLookupHelper.GetTaxAccount(dapper, orgId, required: false);
            if (taxAccount != null && taxAccount.Id > 0)
                GLAccountLookupHelper.AddTaxEntry(glHeader, taxAccount, invoice.Invoice.TaxAmount, isSale, code);
            else
            {
                glHeader.Warnings.Add($"Tax account not found. Invoice has TaxAmount of {invoice.Invoice.TaxAmount:N2}. Using placeholder.");
                GLAccountLookupHelper.AddTaxEntry(glHeader, new ChartOfAccountsModel { Id = 0, Code = "MISSING", Name = "TAX ACCOUNT (NOT CONFIGURED)", Type = "LIABILITY" }, invoice.Invoice.TaxAmount, isSale, code);
            }
        }
    }

    /// <summary>One row from DB for invoice totals (same as CreateGLFromInvoice computed values).</summary>
    internal class InvoiceTotalsRow
    {
        public decimal InvoiceAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ChargesAmount { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    /// <summary>One invoice detail line for preview (same shape as CreateGLFromInvoice item query).</summary>
    internal class PreviewInvoiceLineRow
    {
        public int Id { get; set; }
        public int? ItemId { get; set; }
        public int? AccountId { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? ManualItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int RevenueAccountId { get; set; }
        public int ExpenseAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}

