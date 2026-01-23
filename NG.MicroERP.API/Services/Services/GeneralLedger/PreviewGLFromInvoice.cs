using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services.GeneralLedger;

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
            if (invoice.Invoice.Id > 0 && invoice.Invoice.IsPostedToGL == 1 && !string.IsNullOrWhiteSpace(invoice.Invoice.GLEntryNo))
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
                    // Get details for the posted entry
                    string sqlPostedDetails = $@"SELECT gld.*, coa.Code as AccountCode, coa.Name as AccountName, coa.Type as AccountType,
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
                OrganizationId = invoice.Invoice.OrganizationId,
                EntryNo = "PREVIEW",
                EntryDate = invoice.Invoice.TranDate ?? DateTime.Now,
                Source = "INVOICE",
                Description = $"Invoice {invoice.Invoice.Code ?? "NEW"}",
                ReferenceNo = invoice.Invoice.Code ?? "NEW",
                ReferenceType = "INVOICE",
                ReferenceId = invoice.Invoice.Id,
                PartyId = invoice.Invoice.PartyId,
                LocationId = invoice.Invoice.LocationId,
                BaseCurrencyId = invoice.Invoice.BaseCurrencyId,
                EnteredCurrencyId = invoice.Invoice.EnteredCurrencyId,
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(),
                Warnings = new List<string>()
            };

            bool isSale = invoice.Invoice.InvoiceType?.ToUpper() == "SALE";
            
            // ====================================================================================
            // STEP 1: LOOKUP AR/AP ACCOUNT (used in Entry 1 and Entry 6)
            // Source Priority: Invoice.AccountId -> Party.AccountId -> ChartOfAccounts.InterfaceType
            // ====================================================================================
            var arApResult = await GLAccountLookupHelper.GetARAPAccount(
                dapper,
                invoice.Invoice.OrganizationId,
                isSale,
                invoice.Invoice.AccountId > 0 ? invoice.Invoice.AccountId : null,
                invoice.Invoice.PartyId > 0 ? invoice.Invoice.PartyId : null);
            
            ChartOfAccountsModel? arApAccount = null;
            if (!arApResult.Success)
            {
                string warningMsg = $"AR/AP Account Missing: {arApResult.ErrorMessage}";
                glHeader.Warnings.Add(warningMsg);
                Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                // Create placeholder account for missing AR/AP
                arApAccount = new ChartOfAccountsModel
                {
                    Id = 0,
                    Code = "MISSING",
                    Name = isSale ? "ACCOUNTS RECEIVABLE (NOT CONFIGURED)" : "ACCOUNTS PAYABLE (NOT CONFIGURED)",
                    Type = isSale ? "ASSET" : "LIABILITY"
                };
            }
            else
            {
                arApAccount = arApResult.Account!;
            }

            // ====================================================================================
            // CALCULATE BALANCE (Invoice Amount - Payments)
            // Balance will be charged to AR/AP account based on PartyId
            // ====================================================================================
            decimal totalPaymentAmount = 0;
            if (invoice.InvoicePayments != null && invoice.InvoicePayments.Any())
            {
                totalPaymentAmount = (decimal)invoice.InvoicePayments.Where(p => p.Amount > 0).Sum(p => p.Amount);
            }
            
            decimal balanceAmount = invoice.Invoice.TotalInvoiceAmount - totalPaymentAmount;

            // ====================================================================================
            // CREATE ACCOUNTING ENTRIES BASED ON INVOICE TYPE
            // ====================================================================================
            // For SALE: Debit AR (balance only), Credit Revenue/Tax/Charges, Debit Discount
            // For PURCHASE: Credit AP (balance only), Debit Expense/Tax/Charges, Credit Discount
            if (isSale)
            {
                // ====================================================================================
                // ACCOUNTING ENTRY 1: AR/AP ENTRY (SALE - Debit AR for balance amount)
                // Source: Invoice.AccountId -> Party.AccountId -> ChartOfAccounts.InterfaceType('ACCOUNTS RECEIVABLE')
                // Amount: Balance (TotalInvoiceAmount - Payments) - only if balance > 0
                // ====================================================================================
                if (arApAccount != null)
                {
                    GLAccountLookupHelper.AddARAPEntry(glHeader, arApAccount, balanceAmount, isSale, invoice.Invoice.Code ?? "NEW", invoice.Invoice.PartyId > 0 ? invoice.Invoice.PartyId : null);
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 2: REVENUE ENTRIES (SALE - Credit Revenue, one per invoice item)
                // Source Priority: Items.RevenueAccountId -> Invoice.AccountId -> ChartOfAccounts.InterfaceType('MANUAL ITEM REVENUE')
                // ====================================================================================
                if (invoice.Invoice.Id > 0)
                {
                    // Get invoice items from database
                    string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                      ii.ManualItem,
                                                      COALESCE(i.Name, ii.ManualItem) as ItemName,
                                                      i.RevenueAccountId,
                                                      COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount + ISNULL(tax.TaxAmount, 0), 0) as Amount
                                                FROM InvoiceDetail ii
                                                LEFT JOIN Items i ON ii.ItemId = i.Id
                                                LEFT JOIN (SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax GROUP BY InvoiceDetailId) tax ON ii.Id = tax.InvoiceDetailId
                                                WHERE ii.InvoiceId = {invoice.Invoice.Id} AND ii.IsSoftDeleted = 0
                                                ORDER BY ii.Id";
                    var invoiceItems = await dapper.SearchByQuery<dynamic>(sqlInvoiceItems);
                    
                    if (invoiceItems != null && invoiceItems.Any())
                    {
                        // Normalize line amounts so that sum(lines) == invoice.InvoiceAmount (to avoid rounding / storage mismatches)
                        var positiveItems = invoiceItems
                            .Where(i => Convert.ToDecimal(i.Amount ?? 0) > 0)
                            .ToList();

                        decimal targetInvoiceAmount = invoice.Invoice.InvoiceAmount;
                        decimal sumLineAmounts = positiveItems.Sum(i => Convert.ToDecimal(i.Amount ?? 0));

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetInvoiceAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetInvoiceAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var invoiceItem = positiveItems[idx];

                            decimal rawLineAmount = Convert.ToDecimal(invoiceItem.Amount ?? 0);
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                            {
                                lineAmount = rawLineAmount;
                            }
                            else
                            {
                                // Round and ensure final line fixes any rounding remainder
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetInvoiceAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                allocated += lineAmount;
                            }

                            // Get Revenue Account using common helper
                            int? itemId = null;
                            int? itemRevenueAccountId = null;
                            if (invoiceItem.ItemId != null && !Convert.IsDBNull(invoiceItem.ItemId))
                            {
                                itemId = Convert.ToInt32(invoiceItem.ItemId);
                                if (invoiceItem.RevenueAccountId != null && !Convert.IsDBNull(invoiceItem.RevenueAccountId))
                                {
                                    itemRevenueAccountId = Convert.ToInt32(invoiceItem.RevenueAccountId);
                                }
                            }
                            
                            var revenueResult = await GLAccountLookupHelper.GetRevenueAccount(
                                dapper,
                                invoice.Invoice.OrganizationId,
                                itemId,
                                itemRevenueAccountId,
                                invoice.Invoice.AccountId > 0 ? (int?)invoice.Invoice.AccountId : null,
                                invoiceItem.ItemName?.ToString());
                            
                            ChartOfAccountsModel? revenueAccount = null;
                            if (!revenueResult.Success)
                            {
                                string warningMsg = $"Revenue Account Missing for item '{invoiceItem.ItemName}': {revenueResult.ErrorMessage}";
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
                                revenueAccount = revenueResult.Account!;
                            }
                            
                            // Creates one accounting entry per invoice item
                            GLAccountLookupHelper.AddRevenueEntry(glHeader, revenueAccount, lineAmount, invoiceItem.ItemName?.ToString(), invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }
                else
                {
                    // Handle unsaved invoices (preview mode) - use InvoiceDetails from in-memory collection
                    if (invoice.InvoiceDetails != null && invoice.InvoiceDetails.Any())
                    {
                        var positiveItems = invoice.InvoiceDetails
                            .Where(i => i.ItemTotalAmount > 0)
                            .ToList();

                        decimal targetInvoiceAmount = invoice.Invoice.InvoiceAmount;
                        decimal sumLineAmounts = positiveItems.Sum(i => (decimal)i.ItemTotalAmount);

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetInvoiceAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetInvoiceAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var invoiceItem = positiveItems[idx];

                            decimal rawLineAmount = (decimal)invoiceItem.ItemTotalAmount;
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                            {
                                lineAmount = rawLineAmount;
                            }
                            else
                            {
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetInvoiceAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                allocated += lineAmount;
                            }

                            // Get Revenue Account using common helper
                            var revenueResult = await GLAccountLookupHelper.GetRevenueAccount(
                                dapper,
                                invoice.Invoice.OrganizationId,
                                invoiceItem.ItemId > 0 ? invoiceItem.ItemId : null,
                                null,
                                invoice.Invoice.AccountId > 0 ? invoice.Invoice.AccountId : null,
                                invoiceItem.ItemName);
                            
                            ChartOfAccountsModel? revenueAccount = null;
                            if (!revenueResult.Success)
                            {
                                string warningMsg = $"Revenue Account Missing for item '{invoiceItem.ItemName}': {revenueResult.ErrorMessage}";
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
                                revenueAccount = revenueResult.Account!;
                            }
                            
                            GLAccountLookupHelper.AddRevenueEntry(glHeader, revenueAccount, lineAmount, invoiceItem.ItemName, invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 3: TAX ENTRY (SALE - Credit Tax)
                // Source: ChartOfAccounts.InterfaceType = 'TAX'
                // Note: Tax is calculated from item-level TaxRuleId (via InvoiceDetailTax table)
                // ====================================================================================
                if (invoice.Invoice.TaxAmount > 0)
                {
                    ChartOfAccountsModel? taxAccount = await GLAccountLookupHelper.GetTaxAccount(dapper, invoice.Invoice.OrganizationId, required: false);
                    if (taxAccount != null && taxAccount.Id > 0)
                    {
                        GLAccountLookupHelper.AddTaxEntry(glHeader, taxAccount, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                    else
                    {
                        string warningMsg = $"Tax account not found. Invoice has TaxAmount of {invoice.Invoice.TaxAmount:N2}, but no account with InterfaceType='TAX' is configured. Tax entry will be skipped.";
                        glHeader.Warnings.Add(warningMsg);
                        Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                        // Create placeholder entry for missing tax account
                        var placeholderTaxAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "TAX ACCOUNT (NOT CONFIGURED)",
                            Type = "LIABILITY"
                        };
                        GLAccountLookupHelper.AddTaxEntry(glHeader, placeholderTaxAccount, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 4: SERVICE CHARGES ENTRY (SALE - Credit Service Charges)
                // Source: ChartOfAccounts.InterfaceType = 'SERVICE'
                // ====================================================================================
                if (invoice.Invoice.ChargesAmount > 0)
                {
                    var chargeAccount = await GLAccountLookupHelper.GetServiceAccount(dapper, invoice.Invoice.OrganizationId, required: false);
                    
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
                    var discountAccount = await GLAccountLookupHelper.GetDiscountAccount(dapper, invoice.Invoice.OrganizationId, required: false);
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
                // ACCOUNTING ENTRY 1: AR/AP ENTRY (PURCHASE - Credit AP for balance amount)
                // Source: Invoice.AccountId -> Party.AccountId -> ChartOfAccounts.InterfaceType('ACCOUNTS PAYABLE')
                // Amount: Balance (TotalInvoiceAmount - Payments) - only if balance > 0
                // ====================================================================================
                if (arApAccount != null)
                {
                    GLAccountLookupHelper.AddARAPEntry(glHeader, arApAccount, balanceAmount, isSale, invoice.Invoice.Code ?? "NEW", invoice.Invoice.PartyId > 0 ? invoice.Invoice.PartyId : null);
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 2: EXPENSE ENTRIES (PURCHASE - Debit Expense, one per invoice item)
                // Source Priority: Items.ExpenseAccountId -> Invoice.AccountId -> ChartOfAccounts.InterfaceType('MANUAL ITEM EXPENSE')
                // ====================================================================================
                if (invoice.Invoice.Id > 0)
                {
                    // Get invoice items from database
                    string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                      ii.ManualItem,
                                                      COALESCE(i.Name, ii.ManualItem) as ItemName,
                                                      i.ExpenseAccountId,
                                                      COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount + ISNULL(tax.TaxAmount, 0), 0) as Amount
                                                FROM InvoiceDetail ii
                                                LEFT JOIN Items i ON ii.ItemId = i.Id
                                                LEFT JOIN (SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax GROUP BY InvoiceDetailId) tax ON ii.Id = tax.InvoiceDetailId
                                                WHERE ii.InvoiceId = {invoice.Invoice.Id} AND ii.IsSoftDeleted = 0
                                                ORDER BY ii.Id";
                    var invoiceItems = await dapper.SearchByQuery<dynamic>(sqlInvoiceItems);
                    
                    if (invoiceItems != null && invoiceItems.Any())
                    {
                        // Normalize line amounts so that sum(lines) == invoice.InvoiceAmount (to avoid rounding / storage mismatches)
                        var positiveItems = invoiceItems
                            .Where(i => Convert.ToDecimal(i.Amount ?? 0) > 0)
                            .ToList();

                        decimal targetInvoiceAmount = invoice.Invoice.InvoiceAmount;
                        decimal sumLineAmounts = positiveItems.Sum(i => Convert.ToDecimal(i.Amount ?? 0));

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetInvoiceAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetInvoiceAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var invoiceItem = positiveItems[idx];

                            decimal rawLineAmount = Convert.ToDecimal(invoiceItem.Amount ?? 0);
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                            {
                                lineAmount = rawLineAmount;
                            }
                            else
                            {
                                // Round and ensure final line fixes any rounding remainder
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetInvoiceAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                allocated += lineAmount;
                            }

                            // Get Expense Account using common helper
                            int? itemId = null;
                            int? itemExpenseAccountId = null;
                            if (invoiceItem.ItemId != null && !Convert.IsDBNull(invoiceItem.ItemId))
                            {
                                itemId = Convert.ToInt32(invoiceItem.ItemId);
                                if (invoiceItem.ExpenseAccountId != null && !Convert.IsDBNull(invoiceItem.ExpenseAccountId))
                                {
                                    itemExpenseAccountId = Convert.ToInt32(invoiceItem.ExpenseAccountId);
                                }
                            }
                            
                            var expenseResult = await GLAccountLookupHelper.GetExpenseAccount(
                                dapper,
                                invoice.Invoice.OrganizationId,
                                itemId,
                                itemExpenseAccountId,
                                invoice.Invoice.AccountId > 0 ? (int?)invoice.Invoice.AccountId : null,
                                invoiceItem.ItemName?.ToString());
                            
                            ChartOfAccountsModel? expenseAccount = null;
                            if (!expenseResult.Success)
                            {
                                string warningMsg = $"Expense Account Missing for item '{invoiceItem.ItemName}': {expenseResult.ErrorMessage}";
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
                                expenseAccount = expenseResult.Account!;
                            }
                            
                            GLAccountLookupHelper.AddExpenseEntry(glHeader, expenseAccount, lineAmount, invoiceItem.ItemName?.ToString(), invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }
                else
                {
                    // Handle unsaved invoices (preview mode) - use InvoiceDetails from in-memory collection
                    if (invoice.InvoiceDetails != null && invoice.InvoiceDetails.Any())
                    {
                        var positiveItems = invoice.InvoiceDetails
                            .Where(i => i.ItemTotalAmount > 0)
                            .ToList();

                        decimal targetInvoiceAmount = invoice.Invoice.InvoiceAmount;
                        decimal sumLineAmounts = positiveItems.Sum(i => (decimal)i.ItemTotalAmount);

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetInvoiceAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetInvoiceAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var invoiceItem = positiveItems[idx];

                            decimal rawLineAmount = (decimal)invoiceItem.ItemTotalAmount;
                            if (rawLineAmount <= 0) continue;

                            decimal lineAmount;
                            if (!needsScaling)
                            {
                                lineAmount = rawLineAmount;
                            }
                            else
                            {
                                lineAmount = (idx == positiveItems.Count - 1)
                                    ? (targetInvoiceAmount - allocated)
                                    : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                allocated += lineAmount;
                            }

                            // Get Expense Account using common helper
                            int? itemIdForExpense = invoiceItem.ItemId > 0 ? invoiceItem.ItemId : null;
                            var expenseResult = await GLAccountLookupHelper.GetExpenseAccount(
                                dapper,
                                invoice.Invoice.OrganizationId,
                                itemIdForExpense,
                                null,
                                invoice.Invoice.AccountId > 0 ? (int?)invoice.Invoice.AccountId : null,
                                invoiceItem.ItemName);
                            
                            ChartOfAccountsModel? expenseAccount = null;
                            if (!expenseResult.Success)
                            {
                                string warningMsg = $"Expense Account Missing for item '{invoiceItem.ItemName}': {expenseResult.ErrorMessage}";
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
                                expenseAccount = expenseResult.Account!;
                            }
                            
                            GLAccountLookupHelper.AddExpenseEntry(glHeader, expenseAccount, lineAmount, invoiceItem.ItemName, invoice.Invoice.Code ?? "NEW");
                        }
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 3: TAX ENTRY (PURCHASE - Debit Tax)
                // Source: ChartOfAccounts.InterfaceType = 'TAX'
                // Note: Tax is calculated from item-level TaxRuleId (via InvoiceDetailTax table)
                // ====================================================================================
                if (invoice.Invoice.TaxAmount > 0)
                {
                    var taxAccount = await GLAccountLookupHelper.GetTaxAccount(dapper, invoice.Invoice.OrganizationId, required: false);
                    if (taxAccount != null && taxAccount.Id > 0)
                    {
                        GLAccountLookupHelper.AddTaxEntry(glHeader, taxAccount, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                    else
                    {
                        string warningMsg = $"Tax account not found. Invoice has TaxAmount of {invoice.Invoice.TaxAmount:N2}, but no account with InterfaceType='TAX' is configured. Tax entry will be skipped.";
                        glHeader.Warnings.Add(warningMsg);
                        Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                        // Create placeholder entry for missing tax account
                        var placeholderTaxAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "TAX ACCOUNT (NOT CONFIGURED)",
                            Type = "LIABILITY"
                        };
                        GLAccountLookupHelper.AddTaxEntry(glHeader, placeholderTaxAccount, invoice.Invoice.TaxAmount, isSale, invoice.Invoice.Code ?? "NEW");
                    }
                }

                // ====================================================================================
                // ACCOUNTING ENTRY 4: SERVICE CHARGES ENTRY (PURCHASE - Debit Service Charges)
                // Source: ChartOfAccounts.InterfaceType = 'SERVICE'
                // ====================================================================================
                if (invoice.Invoice.ChargesAmount > 0)
                {
                    var chargeAccount = await GLAccountLookupHelper.GetServiceAccount(dapper, invoice.Invoice.OrganizationId, required: false);
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
                    var discountAccount = await GLAccountLookupHelper.GetDiscountAccount(dapper, invoice.Invoice.OrganizationId, required: false);
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
            // ACCOUNTING ENTRIES 6-7: PAYMENT ENTRIES (if payments exist)
            // Source: ChartOfAccounts.InterfaceType = 'PAYMENT METHOD' (from InvoicePayments.AccountId)
            // For SALE: Debit Cash/Bank, Credit AR
            // For PURCHASE: Debit AP, Credit Cash/Bank
            // ====================================================================================
            if (invoice.InvoicePayments != null && invoice.InvoicePayments.Any())
            {
                // First, validate that all payment AccountIds correspond to accounts with InterfaceType = 'PAYMENT METHOD'
                var validPaymentAccountIds = new HashSet<int>();
                foreach (var payment in invoice.InvoicePayments.Where(p => p.Amount > 0 && p.AccountId > 0))
                {
                    string validateAccountSql = $@"SELECT TOP 1 Id FROM ChartOfAccounts 
                                                   WHERE Id = {payment.AccountId} 
                                                   AND OrganizationId = {invoice.Invoice.OrganizationId} 
                                                   AND InterfaceType = 'PAYMENT METHOD'
                                                   AND IsActive = 1";
                    var validateResult = await dapper.SearchByQuery<ChartOfAccountsModel>(validateAccountSql);
                    if (validateResult != null && validateResult.Any())
                    {
                        validPaymentAccountIds.Add(payment.AccountId);
                    }
                }
                
                // Group payments by AccountId and sum amounts - Only include valid payment method accounts
                var groupedPayments = invoice.InvoicePayments
                    .Where(p => p.Amount > 0 && p.AccountId > 0 && validPaymentAccountIds.Contains(p.AccountId))
                    .GroupBy(p => p.AccountId)
                    .Select(g => new
                    {
                        AccountId = g.Key,
                        TotalAmount = g.Sum(p => (double)p.Amount),
                        PaymentRefs = g.Where(p => !string.IsNullOrWhiteSpace(p.PaymentRef))
                                      .Select(p => p.PaymentRef)
                                      .Distinct()
                                      .ToList(),
                        Payments = g.ToList()
                    })
                    .ToList();

                foreach (var groupedPayment in groupedPayments)
                {
                    // Get payment method account (cash/bank account) - Must have InterfaceType = 'PAYMENT METHOD'
                    string paymentAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                 WHERE Id = {groupedPayment.AccountId} 
                                                 AND OrganizationId = {invoice.Invoice.OrganizationId} 
                                                 AND InterfaceType = 'PAYMENT METHOD'
                                                 AND IsActive = 1";
                    var paymentAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(paymentAccountSql);
                    ChartOfAccountsModel? paymentAccount = paymentAccountResult?.FirstOrDefault();
                    
                    // If account not found or doesn't have InterfaceType = 'PAYMENT METHOD', create placeholder
                    if (paymentAccount == null || paymentAccount.Id == 0)
                    {
                        string warningMsg = $"Payment Account (ID: {groupedPayment.AccountId}) not found or does not have InterfaceType='PAYMENT METHOD'. Payment entry will show as missing account.";
                        glHeader.Warnings.Add(warningMsg);
                        Log.Warning($"PreviewGLFromInvoice: {warningMsg}");
                        // Create placeholder account
                        paymentAccount = new ChartOfAccountsModel
                        {
                            Id = 0,
                            Code = "MISSING",
                            Name = "PAYMENT METHOD ACCOUNT (NOT CONFIGURED)",
                            Type = "ASSET"
                        };
                    }
                    
                    // Build description with payment references if available
                    string paymentDescription = $"Payment - Invoice {invoice.Invoice.Code ?? "NEW"}";
                    if (groupedPayment.PaymentRefs.Any())
                    {
                        paymentDescription += $" ({string.Join(", ", groupedPayment.PaymentRefs)})";
                    }
                    else if (groupedPayment.Payments.Count > 1)
                    {
                        paymentDescription += $" ({groupedPayment.Payments.Count} payments)";
                    }

                    GLAccountLookupHelper.AddPaymentEntries(
                        glHeader,
                        arApAccount,
                        paymentAccount,
                        groupedPayment.TotalAmount,
                        isSale,
                        invoice.Invoice.Code ?? "NEW",
                        groupedPayment.PaymentRefs.Any() ? string.Join(", ", groupedPayment.PaymentRefs) : null,
                        invoice.Invoice.PartyId > 0 ? invoice.Invoice.PartyId : null);
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

}
