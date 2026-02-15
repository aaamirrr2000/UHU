using Dapper;
using Microsoft.Data.SqlClient;
using MicroERP.API.Helper;
using MicroERP.API.Services;
using MicroERP.Shared.Models;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MicroERP.API.Services.GeneralLedger;

public static class CreateGLFromInvoiceHelper
{
    public static async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromInvoice(
        IGeneralLedgerService service,
        int invoiceId,
        int userId,
        string clientInfo)
    {
        try
        {
            using var connection = new SqlConnection(new Config().DefaultConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get invoice data (invoice model may have Id/AccountId etc. mapped from DB; ensure we use int for Ids)
                string SQLInvoice = $@"SELECT * FROM Invoice WHERE Id = {invoiceId} AND IsSoftDeleted = 0";
                var invoice = (await connection.QueryAsync<InvoiceModel>(SQLInvoice, transaction: transaction))?.FirstOrDefault();
                int invoiceIdVal = invoice != null ? Convert.ToInt32(invoice.Id) : 0;
                if (invoice == null || invoiceIdVal == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "Invoice not found.");
                }
                int orgId = Convert.ToInt32(invoice.OrganizationId);
                int partyIdVal = Convert.ToInt32(invoice.PartyId);
                int locationIdVal = Convert.ToInt32(invoice.LocationId);
                int baseCurrencyIdVal = Convert.ToInt32(invoice.BaseCurrencyId);
                int enteredCurrencyIdVal = Convert.ToInt32(invoice.EnteredCurrencyId);

                // Check if already posted
                if (invoice.IsPostedToGL == 1)
                {
                    transaction.Rollback();
                    return (false, null!, "Invoice is already posted to General Ledger.");
                }

                // Invoice table does not store totals; compute from child tables so GL details can be generated
                var rawInvoiceAmount = await connection.ExecuteScalarAsync($@"
                    SELECT ISNULL(SUM((id.Qty * id.UnitPrice) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0)), 0)
                    FROM InvoiceDetail id
                    LEFT JOIN (SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax GROUP BY InvoiceDetailId) tax ON id.Id = tax.InvoiceDetailId
                    WHERE id.InvoiceId = {invoiceIdVal} AND id.IsSoftDeleted = 0", transaction: transaction);
                var rawTaxAmount = await connection.ExecuteScalarAsync($@"
                    SELECT ISNULL(SUM(idt.TaxAmount), 0) FROM InvoiceDetailTax idt
                    INNER JOIN InvoiceDetail id ON idt.InvoiceDetailId = id.Id
                    WHERE id.InvoiceId = {invoiceIdVal} AND id.IsSoftDeleted = 0", transaction: transaction);
                var rawChargesAmount = await connection.ExecuteScalarAsync($@"
                    SELECT ISNULL(SUM(AppliedAmount), 0) FROM InvoiceCharges
                    WHERE InvoiceId = {invoiceIdVal} AND ChargeCategory = 'SERVICE' AND IsSoftDeleted = 0", transaction: transaction);
                var rawDiscountAmount = await connection.ExecuteScalarAsync($@"
                    SELECT ISNULL(SUM(AppliedAmount), 0) FROM InvoiceCharges
                    WHERE InvoiceId = {invoiceIdVal} AND ChargeCategory = 'DISCOUNT' AND IsSoftDeleted = 0", transaction: transaction);

                decimal computedInvoiceAmount = rawInvoiceAmount != null && rawInvoiceAmount != DBNull.Value ? Convert.ToDecimal(rawInvoiceAmount) : 0m;
                decimal computedTaxAmount = rawTaxAmount != null && rawTaxAmount != DBNull.Value ? Convert.ToDecimal(rawTaxAmount) : 0m;
                decimal computedChargesAmount = rawChargesAmount != null && rawChargesAmount != DBNull.Value ? Convert.ToDecimal(rawChargesAmount) : 0m;
                decimal computedDiscountAmount = rawDiscountAmount != null && rawDiscountAmount != DBNull.Value ? Convert.ToDecimal(rawDiscountAmount) : 0m;
                invoice.InvoiceAmount = computedInvoiceAmount;
                invoice.TaxAmount = computedTaxAmount;
                invoice.ChargesAmount = computedChargesAmount;
                invoice.DiscountAmount = computedDiscountAmount;
                invoice.TotalInvoiceAmount = invoice.InvoiceAmount + invoice.ChargesAmount - invoice.DiscountAmount;

                // Validate period for SALE or PURCHASE invoice module
                string moduleType = invoice.InvoiceType?.ToUpper() == "PURCHASE" ? "PURCHASE" : "SALE";
                var periodCheck = await service.ValidatePeriod(orgId, invoice.TranDate ?? DateTime.Now, moduleType);
                if (!periodCheck.Item1)
                {
                    transaction.Rollback();
                    return (false, null!, periodCheck.Item2);
                }

                // Generate EntryNo within transaction
                string prefix = "GL";
                int codeLength = 6;
                string format = new string('0', codeLength);
                string sqlGetCode = $@"SELECT MAX(CAST(SUBSTRING(EntryNo, {prefix.Length + 1}, LEN(EntryNo) - {prefix.Length}) AS INT)) AS SEQNO
                     FROM GeneralLedgerHeader WHERE LEFT(EntryNo, {prefix.Length}) = '{prefix}'";
                var codeResult = await connection.QueryFirstOrDefaultAsync<int?>(sqlGetCode, transaction: transaction);
                int nextCode = (codeResult ?? 0) + 1;
                string entryNo = prefix + nextCode.ToString(format);

                bool isSaleHeader = invoice.InvoiceType?.ToUpper().Contains("SALE") == true;
                GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
                {
                    OrganizationId = orgId,
                    EntryNo = entryNo,
                    EntryDate = invoice.TranDate ?? DateTime.Now,
                    Source = isSaleHeader ? "SALE INVOICE" : "PURCHASE INVOICE",
                    Description = isSaleHeader ? $"Sale Invoice {invoice.Code}" : $"Purchase Invoice {invoice.Code}",
                    ReferenceNo = invoice.Code,
                    ReferenceType = "INVOICE",
                    ReferenceId = invoiceIdVal,
                    PartyId = partyIdVal,
                    LocationId = locationIdVal,
                    BaseCurrencyId = baseCurrencyIdVal,
                    EnteredCurrencyId = enteredCurrencyIdVal,
                    CreatedBy = userId,
                    CreatedFrom = clientInfo,
                    Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
                };

                // Map invoice to GL entry - Priority: Invoice.AccountId (correct AR/AP type) else Party.AccountId (correct type). No fallback; error if none defined (same as Preview).
                int arApAccountId = 0;
                string accountTypeName = "";
                bool isSale = invoice.InvoiceType?.ToUpper().Contains("SALE") == true;
                
                string expectedInterfaceType = isSale ? "ACCOUNTS RECEIVABLE" : "ACCOUNTS PAYABLE";
                int invoiceAccountIdVal = Convert.ToInt32(invoice.AccountId);
                // First Priority: Use AccountId from invoice — must match invoice type (AR for sale, AP for purchase), same as Preview
                if (invoiceAccountIdVal > 0)
                {
                    string sqlVerifyAccount = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                 WHERE Id = {invoiceAccountIdVal} AND OrganizationId = {orgId} AND IsActive = 1
                                                 AND InterfaceType = '{expectedInterfaceType}'";
                    var verifyResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlVerifyAccount, transaction: transaction);
                    if (verifyResult != null && verifyResult.Value.Id > 0)
                    {
                        arApAccountId = verifyResult.Value.Id;
                        accountTypeName = verifyResult.Value.Name;
                    }
                }
                // Second Priority: Get AccountId from Party — accept the account linked on the Party if it exists and is active (no InterfaceType required; party link is explicit)
                if (arApAccountId == 0 && partyIdVal > 0)
                {
                    string sqlPartyAccount = $@"SELECT TOP 1 p.AccountId FROM Parties p
                                                INNER JOIN ChartOfAccounts coa ON coa.Id = p.AccountId AND coa.OrganizationId = p.OrganizationId AND coa.IsActive = 1
                                                WHERE p.Id = {partyIdVal} AND p.OrganizationId = {orgId} AND p.IsSoftDeleted = 0
                                                AND p.AccountId IS NOT NULL AND p.AccountId > 0";
                    var partyAccountIdRaw = await connection.ExecuteScalarAsync(sqlPartyAccount, transaction: transaction);
                    int partyAccountIdVal = (partyAccountIdRaw != null && partyAccountIdRaw != DBNull.Value) ? Convert.ToInt32(partyAccountIdRaw) : 0;
                    if (partyAccountIdVal > 0)
                    {
                        arApAccountId = partyAccountIdVal;
                        string sqlVerifyAccount = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                     WHERE Id = {arApAccountId} AND OrganizationId = {orgId} AND IsActive = 1";
                        var verifyResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlVerifyAccount, transaction: transaction);
                        if (verifyResult != null && verifyResult.Value.Id > 0)
                        {
                            accountTypeName = verifyResult.Value.Name;
                            await connection.ExecuteAsync($@"UPDATE Invoice SET AccountId = {arApAccountId} WHERE Id = {invoiceIdVal}", transaction: transaction);
                            invoice.AccountId = arApAccountId;
                        }
                        else
                            arApAccountId = 0;
                    }
                }

                // No fallback: do not pick any account. Require account on Invoice or Party, same as Preview
                if (arApAccountId == 0)
                {
                    transaction.Rollback();
                    return (false, null!, $"No {(isSale ? "Accounts Receivable" : "Accounts Payable")} account defined. Set the account on the Invoice (with InterfaceType '{expectedInterfaceType}') or link the Party to an account in Chart of Accounts (Party → AccountId). No default account will be used.");
                }

                string sqlArApDetails = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts WHERE Id = {arApAccountId}";
                var arApAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlArApDetails, transaction: transaction);
                if (arApAccount == null)
                {
                    transaction.Rollback();
                    return (false, null!, $"AR/AP account details not found.");
                }

                // For SALE: Debit AR, Credit Revenue/Tax/Charges, Debit Discount
                // For PURCHASE: Credit AP, Debit Expense/Tax/Charges, Credit Discount
                if (isSale)
                {
                    // Debit AR = TotalInvoiceAmount (amount customer owes) so journal balances; fallback to component sum
                    decimal arAmount = invoice.TotalInvoiceAmount > 0
                        ? invoice.TotalInvoiceAmount
                        : (invoice.InvoiceAmount + invoice.ChargesAmount + invoice.TaxAmount - invoice.DiscountAmount);
                    if (arAmount > 0)
                        GLAccountLookupHelper.AddARAPEntry(glHeader, (arApAccount.Value.Id, arApAccount.Value.Code, arApAccount.Value.Name, arApAccount.Value.Type), arAmount, isSale, invoice.Code ?? "", partyIdVal);

                    // Credit Revenue (from InvoiceAmount)
                    // Post each invoice item separately using item-level GL account mapping
                    // Priority: Item's RevenueAccountId -> Invoice AccountId -> ITEM_REVENUE InterfaceType -> REVENUE Type
                    if (invoice.InvoiceAmount > 0)
                    {
                        // Get invoice items (same query as PURCHASE)
                        // Use pre-tax amount (Qty*UnitPrice - Discount) so tax is posted once; matches Preview and keeps journal balanced
                        // Same query shape as Preview: COALESCE(i.Name, ii.ManualItem) so descriptions match
                        string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.AccountId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                          ii.ManualItem,
                                                          i.Code as ItemCode, COALESCE(i.Name, ii.ManualItem) as ItemName, i.RevenueAccountId,
                                                          COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount, 0) as Amount
                                                   FROM InvoiceDetail ii
                                                   LEFT JOIN Items i ON ii.ItemId = i.Id
                                                   WHERE ii.InvoiceId = {invoiceIdVal} AND ii.IsSoftDeleted = 0
                                                   ORDER BY ii.Id";
                        var invoiceItems = (await connection.QueryAsync<InvoiceLineForGL>(sqlInvoiceItems, transaction: transaction))?.ToList();

                        if (invoiceItems != null && invoiceItems.Count > 0)
                        {
                            // Normalize line amounts so that sum(lines) == invoice.InvoiceAmount (to avoid rounding / storage mismatches)
                            var positiveItems = invoiceItems
                                .Where(i => i.Amount > 0)
                                .ToList();

                            decimal targetRevenueAmount = invoice.InvoiceAmount;
                            decimal sumLineAmounts = positiveItems.Sum(i => i.Amount);

                            bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetRevenueAmount) > 0.01m;
                            decimal scaleFactor = needsScaling ? (targetRevenueAmount / sumLineAmounts) : 1m;
                            decimal allocated = 0m;

                            for (int idx = 0; idx < positiveItems.Count; idx++)
                            {
                                var item = positiveItems[idx];

                                decimal rawLineAmount = item.Amount;
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
                                        ? (targetRevenueAmount - allocated)
                                        : Math.Round(rawLineAmount * scaleFactor, 2, MidpointRounding.AwayFromZero);
                                    allocated += lineAmount;
                                }

                                int revenueAccountId = 0;
                                string revenueCode = "";
                                string revenueName = "";
                                string revenueType = "";

                                // First Priority: Use InvoiceDetail.AccountId (if saved during invoice creation)
                                int? invoiceDetailAccountId = item.AccountId;
                                if (invoiceDetailAccountId.HasValue && invoiceDetailAccountId.Value > 0)
                                {
                                    string detailAccountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                              WHERE Id = {invoiceDetailAccountId.Value} 
                                                              AND OrganizationId = {orgId} 
                                                              AND IsActive = 1";
                                    var detailAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(detailAccountSql, transaction: transaction);
                                    if (detailAccount != null && detailAccount.Value.Id > 0)
                                    {
                                        revenueAccountId = detailAccount.Value.Id;
                                        revenueCode = detailAccount.Value.Code;
                                        revenueName = detailAccount.Value.Name;
                                        revenueType = detailAccount.Value.Type;
                                    }
                                }

                                // Second Priority: Use Item's RevenueAccountId (if configured)
                                if (revenueAccountId == 0)
                                {
                                    int? itemRevenueAccountId = item.RevenueAccountId;
                                    if (itemRevenueAccountId.HasValue && itemRevenueAccountId.Value > 0)
                                    {
                                        string itemAccountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                                  WHERE Id = {itemRevenueAccountId.Value} 
                                                                  AND OrganizationId = {orgId} 
                                                                  AND IsActive = 1";
                                        var itemAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(itemAccountSql, transaction: transaction);
                                        if (itemAccount != null && itemAccount.Value.Id > 0)
                                        {
                                            revenueAccountId = itemAccount.Value.Id;
                                            revenueCode = itemAccount.Value.Code;
                                            revenueName = itemAccount.Value.Name;
                                            revenueType = itemAccount.Value.Type;
                                        }
                                    }
                                }

                                // Third Priority: Use Invoice's AccountId (if specified)
                                if (revenueAccountId == 0 && invoiceAccountIdVal > 0)
                                {
                                    string accountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                           WHERE Id = {invoiceAccountIdVal} 
                                                           AND OrganizationId = {orgId} 
                                                           AND IsActive = 1";
                                    var accountResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(accountSql, transaction: transaction);
                                    if (accountResult != null && accountResult.Value.Id > 0)
                                    {
                                        revenueAccountId = accountResult.Value.Id;
                                        revenueCode = accountResult.Value.Code;
                                        revenueName = accountResult.Value.Name;
                                        revenueType = accountResult.Value.Type;
                                    }
                                }

                                // Fourth Priority: Look for MANUAL ITEM REVENUE InterfaceType (dedicated manual item revenue account)
                                if (revenueAccountId == 0)
                                {
                                    string itemRevenueSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                              WHERE OrganizationId = {orgId} 
                                                              AND InterfaceType = 'MANUAL ITEM REVENUE'
                                                              AND IsActive = 1
                                                              ORDER BY Code";
                                    var itemRevenueAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(itemRevenueSql, transaction: transaction);
                                    if (itemRevenueAccount != null && itemRevenueAccount.Value.Id > 0)
                                    {
                                        revenueAccountId = itemRevenueAccount.Value.Id;
                                        revenueCode = itemRevenueAccount.Value.Code;
                                        revenueName = itemRevenueAccount.Value.Name;
                                        revenueType = itemRevenueAccount.Value.Type;
                                    }
                                }

                                // Add GL entry only if we found a valid revenue account
                                // Note: We do NOT fallback to generic REVENUE Type - must use InterfaceType accounts
                                if (revenueAccountId > 0)
                                {
                                    GLAccountLookupHelper.AddRevenueEntry(glHeader, (revenueAccountId, revenueCode, revenueName, revenueType), lineAmount, item.ItemName ?? item.ItemCode ?? "Manual Item", invoice.Code ?? "");
                                }
                                else
                                {
                                    transaction.Rollback();
                                    return (false, null!, $"Revenue account not found for item '{item.ItemName ?? "Unknown"}'. Please configure a Revenue Account on the Item, or set the Invoice Account, or configure a 'MANUAL ITEM REVENUE' account in Chart of Accounts.");
                                }
                            }
                        }
                    }

                    // Credit Tax accounts (always post when amount > 0; use placeholder if account not configured)
                    if (invoice.TaxAmount > 0)
                    {
                        string taxSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {orgId} 
                                          AND InterfaceType = 'TAX'
                                          AND IsActive = 1
                                          ORDER BY Code";
                        var taxAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(taxSql, transaction: transaction);
                        if (taxAccount != null && taxAccount.Value.Id > 0)
                            GLAccountLookupHelper.AddTaxEntry(glHeader, (taxAccount.Value.Id, taxAccount.Value.Code, taxAccount.Value.Name, taxAccount.Value.Type), invoice.TaxAmount, isSale, invoice.Code ?? "");
                        else
                            GLAccountLookupHelper.AddTaxEntry(glHeader, (0, "MISSING", "TAX ACCOUNT (NOT CONFIGURED)", "LIABILITY"), invoice.TaxAmount, isSale, invoice.Code ?? "");
                    }

                    // Credit Service Charges
                    if (invoice.ChargesAmount > 0)
                    {
                        string chargeSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                             WHERE OrganizationId = {orgId} 
                                             AND InterfaceType = 'SERVICE'
                                             AND IsActive = 1
                                             ORDER BY Code";
                        var chargeAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(chargeSql, transaction: transaction);
                        if (chargeAccount != null && chargeAccount.Value.Id > 0)
                            GLAccountLookupHelper.AddServiceChargesEntry(glHeader, (chargeAccount.Value.Id, chargeAccount.Value.Code, chargeAccount.Value.Name, chargeAccount.Value.Type), invoice.ChargesAmount, isSale, invoice.Code ?? "");
                        else
                            GLAccountLookupHelper.AddServiceChargesEntry(glHeader, (0, "MISSING", "SERVICE CHARGES (NOT CONFIGURED)", "REVENUE"), invoice.ChargesAmount, isSale, invoice.Code ?? "");
                    }

                    // Debit Discount (always post when amount > 0; use placeholder if account not configured)
                    if (invoice.DiscountAmount > 0)
                    {
                        string discountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                               WHERE OrganizationId = {orgId} 
                                               AND InterfaceType = 'DISCOUNT'
                                               AND IsActive = 1
                                               ORDER BY Code";
                        var discountAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(discountSql, transaction: transaction);
                        if (discountAccount != null && discountAccount.Value.Id > 0)
                            GLAccountLookupHelper.AddDiscountEntry(glHeader, (discountAccount.Value.Id, discountAccount.Value.Code, discountAccount.Value.Name, discountAccount.Value.Type), invoice.DiscountAmount, isSale, invoice.Code ?? "");
                        else
                            GLAccountLookupHelper.AddDiscountEntry(glHeader, (0, "MISSING", "DISCOUNT ACCOUNT (NOT CONFIGURED)", "REVENUE"), invoice.DiscountAmount, isSale, invoice.Code ?? "");
                    }
                }
                else
                {
                    // PURCHASE: Credit AP, Debit Expense/Tax/Charges, Credit Discount
                    // Credit AP = TotalInvoiceAmount so journal balances; fallback to component sum
                    decimal apAmount = invoice.TotalInvoiceAmount > 0
                        ? invoice.TotalInvoiceAmount
                        : (invoice.InvoiceAmount + invoice.ChargesAmount + invoice.TaxAmount - invoice.DiscountAmount);
                    if (apAmount > 0)
                        GLAccountLookupHelper.AddARAPEntry(glHeader, (arApAccount.Value.Id, arApAccount.Value.Code, arApAccount.Value.Name, arApAccount.Value.Type), apAmount, isSale, invoice.Code ?? "", partyIdVal);

                    // Debit Expense (from InvoiceAmount)
                    // Post each invoice item separately using item-level GL account mapping
                    // Priority: Item's ExpenseAccountId -> Invoice AccountId -> ITEM_EXPENSE InterfaceType -> EXPENSE Type
                    
                    // Get invoice items
                    // Use pre-tax amount (Qty*UnitPrice - Discount) so tax is posted once; matches Preview and keeps journal balanced
                    // Same query shape as Preview: COALESCE(i.Name, ii.ManualItem) so descriptions match
                    string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.AccountId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                      ii.ManualItem,
                                                      i.Code as ItemCode, COALESCE(i.Name, ii.ManualItem) as ItemName, i.ExpenseAccountId,
                                                      COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount, 0) as Amount
                                               FROM InvoiceDetail ii
                                               LEFT JOIN Items i ON ii.ItemId = i.Id
                                               WHERE ii.InvoiceId = {invoiceIdVal} AND ii.IsSoftDeleted = 0
                                               ORDER BY ii.Id";
                                    var invoiceItems = (await connection.QueryAsync<InvoiceLineForGL>(sqlInvoiceItems, transaction: transaction))?.ToList();

                                    if (invoiceItems != null && invoiceItems.Count > 0)
                                    {
                                        // Normalize line amounts so that sum(lines) == invoice.InvoiceAmount (to avoid rounding / storage mismatches)
                                        var positiveItems = invoiceItems
                                            .Where(i => i.Amount > 0)
                                            .ToList();

                                        decimal targetInvoiceAmount = invoice.InvoiceAmount;
                                        decimal sumLineAmounts = positiveItems.Sum(i => i.Amount);

                                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetInvoiceAmount) > 0.01m;
                                        decimal scaleFactor = needsScaling ? (targetInvoiceAmount / sumLineAmounts) : 1m;
                                        decimal allocated = 0m;

                                        for (int idx = 0; idx < positiveItems.Count; idx++)
                                        {
                                            var item = positiveItems[idx];

                                            decimal rawLineAmount = item.Amount;
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

                                            int expenseAccountId = 0;
                                            string expenseCode = "";
                                            string expenseName = "";
                                            string expenseType = "";

                                            // First Priority: Use InvoiceDetail.AccountId (if saved during invoice creation)
                                            int? invoiceDetailAccountId = item.AccountId;
                            if (invoiceDetailAccountId.HasValue && invoiceDetailAccountId.Value > 0)
                            {
                                string detailAccountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                          WHERE Id = {invoiceDetailAccountId.Value} 
                                                          AND OrganizationId = {orgId} 
                                                          AND IsActive = 1";
                                var detailAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(detailAccountSql, transaction: transaction);
                                if (detailAccount != null && detailAccount.Value.Id > 0)
                                {
                                    expenseAccountId = detailAccount.Value.Id;
                                    expenseCode = detailAccount.Value.Code;
                                    expenseName = detailAccount.Value.Name;
                                    expenseType = detailAccount.Value.Type;
                                }
                            }

                            // Second Priority: Use Item's ExpenseAccountId (if configured)
                            if (expenseAccountId == 0)
                            {
                                int? itemExpenseAccountId = item.ExpenseAccountId;
                                if (itemExpenseAccountId.HasValue && itemExpenseAccountId.Value > 0)
                                {
                                    string itemAccountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                              WHERE Id = {itemExpenseAccountId.Value} 
                                                              AND OrganizationId = {orgId} 
                                                              AND IsActive = 1";
                                    var itemAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(itemAccountSql, transaction: transaction);
                                    if (itemAccount != null && itemAccount.Value.Id > 0)
                                    {
                                        expenseAccountId = itemAccount.Value.Id;
                                        expenseCode = itemAccount.Value.Code;
                                        expenseName = itemAccount.Value.Name;
                                        expenseType = itemAccount.Value.Type;
                                    }
                                }
                            }

                            // Third Priority: Use Invoice's AccountId (if specified)
                            if (expenseAccountId == 0 && invoiceAccountIdVal > 0)
                            {
                                string accountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                       WHERE Id = {invoiceAccountIdVal} 
                                                       AND OrganizationId = {orgId} 
                                                       AND IsActive = 1";
                                var accountResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(accountSql, transaction: transaction);
                                if (accountResult != null && accountResult.Value.Id > 0)
                                {
                                    expenseAccountId = accountResult.Value.Id;
                                    expenseCode = accountResult.Value.Code;
                                    expenseName = accountResult.Value.Name;
                                    expenseType = accountResult.Value.Type;
                                }
                            }

                            // Fourth Priority: Look for MANUAL ITEM EXPENSE InterfaceType (dedicated manual item expense account)
                            if (expenseAccountId == 0)
                            {
                                string itemExpenseSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                          WHERE OrganizationId = {orgId} 
                                                          AND InterfaceType = 'MANUAL ITEM EXPENSE'
                                                          AND IsActive = 1
                                                          ORDER BY Code";
                                var itemExpenseAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(itemExpenseSql, transaction: transaction);
                                if (itemExpenseAccount != null && itemExpenseAccount.Value.Id > 0)
                                {
                                    expenseAccountId = itemExpenseAccount.Value.Id;
                                    expenseCode = itemExpenseAccount.Value.Code;
                                    expenseName = itemExpenseAccount.Value.Name;
                                    expenseType = itemExpenseAccount.Value.Type;
                                }
                            }

                            // Add GL entry only if we found a valid expense account
                            // Note: We do NOT fallback to generic EXPENSE Type - must use InterfaceType accounts
                            if (expenseAccountId > 0)
                            {
                                GLAccountLookupHelper.AddExpenseEntry(glHeader, (expenseAccountId, expenseCode, expenseName, expenseType), lineAmount, item.ItemName ?? item.ItemCode ?? "Manual Item", invoice.Code ?? "");
                            }
                            else
                            {
                                transaction.Rollback();
                                return (false, null!, $"Expense account not found for item '{item.ItemName ?? "Unknown"}'. Please configure an Expense Account on the Item, or set the Invoice Account, or configure a 'MANUAL ITEM EXPENSE' account in Chart of Accounts.");
                            }
                        }
                    }

                    // Debit Tax (always post when amount > 0; use placeholder if account not configured)
                    if (invoice.TaxAmount > 0)
                    {
                        string taxSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {orgId} 
                                          AND InterfaceType = 'TAX'
                                          AND IsActive = 1
                                          ORDER BY Code";
                        var taxAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(taxSql, transaction: transaction);
                        if (taxAccount != null && taxAccount.Value.Id > 0)
                            GLAccountLookupHelper.AddTaxEntry(glHeader, (taxAccount.Value.Id, taxAccount.Value.Code, taxAccount.Value.Name, taxAccount.Value.Type), invoice.TaxAmount, isSale, invoice.Code ?? "");
                        else
                            GLAccountLookupHelper.AddTaxEntry(glHeader, (0, "MISSING", "TAX ACCOUNT (NOT CONFIGURED)", "LIABILITY"), invoice.TaxAmount, isSale, invoice.Code ?? "");
                    }

                    // Debit Service Charges
                    if (invoice.ChargesAmount > 0)
                    {
                        string chargeSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                             WHERE OrganizationId = {orgId} 
                                             AND InterfaceType = 'SERVICE'
                                             AND IsActive = 1
                                             ORDER BY Code";
                        var chargeAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(chargeSql, transaction: transaction);
                        if (chargeAccount != null && chargeAccount.Value.Id > 0)
                            GLAccountLookupHelper.AddServiceChargesEntry(glHeader, (chargeAccount.Value.Id, chargeAccount.Value.Code, chargeAccount.Value.Name, chargeAccount.Value.Type), invoice.ChargesAmount, isSale, invoice.Code ?? "");
                        else
                            GLAccountLookupHelper.AddServiceChargesEntry(glHeader, (0, "MISSING", "SERVICE CHARGES (NOT CONFIGURED)", "EXPENSE"), invoice.ChargesAmount, isSale, invoice.Code ?? "");
                    }

                    // Credit Discount (always post when amount > 0; use placeholder if account not configured)
                    if (invoice.DiscountAmount > 0)
                    {
                        string discountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                               WHERE OrganizationId = {orgId} 
                                               AND InterfaceType = 'DISCOUNT'
                                               AND IsActive = 1
                                               ORDER BY Code";
                        var discountAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(discountSql, transaction: transaction);
                        if (discountAccount != null && discountAccount.Value.Id > 0)
                            GLAccountLookupHelper.AddDiscountEntry(glHeader, (discountAccount.Value.Id, discountAccount.Value.Code, discountAccount.Value.Name, discountAccount.Value.Type), invoice.DiscountAmount, isSale, invoice.Code ?? "");
                        else
                            GLAccountLookupHelper.AddDiscountEntry(glHeader, (0, "MISSING", "DISCOUNT ACCOUNT (NOT CONFIGURED)", "REVENUE"), invoice.DiscountAmount, isSale, invoice.Code ?? "");
                    }
                }

                // Payment entries: match preview so posted journal includes Cash/Bank vs AR/AP (SALE: Debit Cash/Bank Credit AR; PURCHASE: Debit AP Credit Cash/Bank)
                var invoicePayments = (await connection.QueryAsync<InvoicePaymentModel>(
                    $@"SELECT Id, InvoiceId, AccountId, PaymentRef, Amount, PaidOn, Notes, ISNULL(IsSoftDeleted, 0) AS IsSoftDeleted 
                       FROM InvoicePayments WHERE InvoiceId = {invoiceIdVal} AND (IsSoftDeleted = 0 OR IsSoftDeleted IS NULL)", transaction: transaction))?.ToList() ?? new List<InvoicePaymentModel>();
                var arApAccountModel = new ChartOfAccountsModel { Id = arApAccount.Value.Id, Code = arApAccount.Value.Code, Name = arApAccount.Value.Name, Type = arApAccount.Value.Type };
                foreach (var payment in invoicePayments.Where(p => p.Amount > 0 && p.AccountId > 0))
                {
                    string paymentAccountSql = $@"SELECT TOP 1 CAST(Id AS INT) AS Id, Code, Name, Type FROM ChartOfAccounts 
                                                 WHERE Id = {payment.AccountId} AND OrganizationId = {orgId} 
                                                 AND InterfaceType = 'PAYMENT METHOD' AND IsActive = 1";
                    var paymentAccountTuple = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(paymentAccountSql, transaction: transaction);
                    if (paymentAccountTuple == null || paymentAccountTuple.Value.Id == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"Payment account (ID: {payment.AccountId}) not found or not InterfaceType='PAYMENT METHOD'. Configure the account before posting.");
                    }
                    var paymentAccountModel = new ChartOfAccountsModel { Id = paymentAccountTuple.Value.Id, Code = paymentAccountTuple.Value.Code, Name = paymentAccountTuple.Value.Name, Type = paymentAccountTuple.Value.Type };
                    GLAccountLookupHelper.AddPaymentEntries(glHeader, arApAccountModel, paymentAccountModel, (double)payment.Amount, isSale, invoice.Code ?? "", payment.PaymentRef, partyIdVal > 0 ? partyIdVal : (int?)null);
                }

                if (glHeader.Details == null || glHeader.Details.Count == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "No GL details generated. Please ensure invoice items and required accounts are configured before posting.");
                }

                // Consolidate: one line per account (net debit or net credit), matching the Consolidated preview view
                var consolidated = glHeader.Details
                    .GroupBy(d => d.AccountId)
                    .Select(g =>
                    {
                        double totalDebit = g.Sum(x => x.DebitAmount);
                        double totalCredit = g.Sum(x => x.CreditAmount);
                        double net = totalDebit - totalCredit;
                        if (Math.Abs(net) < 0.01) return null;
                        var first = g.First();
                        return new GeneralLedgerDetailModel
                        {
                            AccountId = first.AccountId,
                            AccountCode = first.AccountCode,
                            AccountName = first.AccountName,
                            AccountType = first.AccountType,
                            Description = g.Count() > 1 ? $"{g.Count()} entries consolidated" : (first.Description ?? $"Invoice {invoice.Code}"),
                            DebitAmount = net > 0 ? net : 0,
                            CreditAmount = net < 0 ? Math.Abs(net) : 0,
                            PartyId = first.PartyId
                        };
                    })
                    .Where(x => x != null)
                    .Cast<GeneralLedgerDetailModel>()
                    .ToList();
                glHeader.Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(consolidated);

                // Calculate totals
                glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
                glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

                // Validate balance
                if (Math.Abs(glHeader.TotalDebit - glHeader.TotalCredit) > 0.01)
                {
                    transaction.Rollback();
                    return (false, null!, $"Entry is not balanced. Total Debit: {glHeader.TotalDebit:N2}, Total Credit: {glHeader.TotalCredit:N2}");
                }

                // Insert GL Header
                string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}' AND IsSoftDeleted = 0;";
                string SQLInsertHeader = $@"INSERT INTO GeneralLedgerHeader 
                    (
                        OrganizationId, EntryNo, EntryDate, Source, Description, ReferenceNo, ReferenceType, ReferenceId,
                        PartyId, LocationId, BaseCurrencyId, EnteredCurrencyId, TotalDebit, TotalCredit, IsReversed, ReversedEntryNo, IsPosted,
                        PostedDate, PostedBy, IsAdjusted, AdjustmentEntryNo, FileAttachment, Notes,
                        CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                    ) 
                    VALUES 
                    (
                        {glHeader.OrganizationId}, '{entryNo.ToUpper()}', '{glHeader.EntryDate:yyyy-MM-dd}',
                        '{glHeader.Source?.ToUpper().Replace("'", "''") ?? "INVOICE"}', '{glHeader.Description!.ToUpper().Replace("'", "''")}',
                        '{glHeader.ReferenceNo?.ToUpper().Replace("'", "''") ?? ""}', '{glHeader.ReferenceType?.ToUpper().Replace("'", "''") ?? ""}',
                        {glHeader.ReferenceId}, {(glHeader.PartyId > 0 ? glHeader.PartyId.ToString() : "NULL")},
                        {(glHeader.LocationId > 0 ? glHeader.LocationId.ToString() : "NULL")},
                        {(glHeader.BaseCurrencyId > 0 ? glHeader.BaseCurrencyId.ToString() : "NULL")},
                        {(glHeader.EnteredCurrencyId > 0 ? glHeader.EnteredCurrencyId.ToString() : "NULL")},
                        {glHeader.TotalDebit}, {glHeader.TotalCredit},
                        {glHeader.IsReversed}, '{glHeader.ReversedEntryNo?.ToUpper().Replace("'", "''") ?? ""}',
                        {glHeader.IsPosted}, {(glHeader.PostedDate.HasValue ? $"'{glHeader.PostedDate.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
                        {(glHeader.PostedBy > 0 ? glHeader.PostedBy.ToString() : "NULL")}, {glHeader.IsAdjusted},
                        '{glHeader.AdjustmentEntryNo?.ToUpper().Replace("'", "''") ?? ""}', '{glHeader.FileAttachment?.Replace("'", "''") ?? ""}',
                        '{glHeader.Notes?.ToUpper().Replace("'", "''") ?? ""}', {glHeader.CreatedBy},
                        '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', '{glHeader.CreatedFrom!.ToUpper()}', {glHeader.IsSoftDeleted}
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                // Check for duplicate
                if (!string.IsNullOrEmpty(SQLDuplicate))
                {
                    var duplicateCheck = await connection.QueryAsync(SQLDuplicate, transaction: transaction);
                    if (duplicateCheck.Any())
                    {
                        transaction.Rollback();
                        return (false, null!, "Duplicate Entry No Found.");
                    }
                }

                // Insert header
                await connection.ExecuteAsync(SQLInsertHeader.TrimEnd(';'), transaction: transaction);
                var scopeIdentity = await connection.ExecuteScalarAsync("SELECT SCOPE_IDENTITY();", transaction: transaction);
                int headerId = scopeIdentity != null && scopeIdentity != DBNull.Value ? Convert.ToInt32(scopeIdentity) : 0;

                // Validate all account IDs before inserting details
                var invalidDetails = glHeader.Details.Where(d => d.AccountId == 0).ToList();
                if (invalidDetails.Any())
                {
                    transaction.Rollback();
                    var invalidDescriptions = string.Join(", ", invalidDetails.Select(d => d.Description ?? "Unknown"));
                    return (false, null!, $"Invalid Account ID found in GL entries. One or more accounts have AccountId = 0. Details: {invalidDescriptions}. Please ensure all accounts are properly configured in Chart of Accounts.");
                }

                // Insert Details
                int seqNo = 1;
                foreach (var detail in glHeader.Details)
                {
                    // Double-check AccountId before inserting (defensive programming)
                    if (detail.AccountId == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"Invalid Account ID (0) found in GL detail entry: {detail.Description ?? "Unknown"}. AccountId must be a valid account from Chart of Accounts.");
                    }

                    string SQLInsertDetail = $@"INSERT INTO GeneralLedgerDetail 
                        (HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo,
                         CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted)
                        VALUES
                        ({headerId}, {detail.AccountId}, '{detail.Description?.ToUpper().Replace("'", "''") ?? ""}',
                         {detail.DebitAmount}, {detail.CreditAmount}, {(detail.PartyId > 0 ? detail.PartyId.ToString() : "NULL")},
                         {seqNo++}, {glHeader.CreatedBy}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                         '{glHeader.CreatedFrom!.ToUpper()}', {detail.IsSoftDeleted});";
                    
                    await connection.ExecuteAsync(SQLInsertDetail.TrimEnd(';'), transaction: transaction);
                }

                // Update invoice with GL posting info
                string SQLUpdateInvoice = $@"UPDATE Invoice SET 
                    IsPostedToGL = 1,
                    PostedToGLDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    PostedToGLBy = {userId},
                    GLEntryNo = '{entryNo}'
                WHERE Id = {invoiceId}";

                await connection.ExecuteAsync(SQLUpdateInvoice, transaction: transaction);
                transaction.Commit();

                // Get the created GL entry
                var result = await service.Get(headerId);
                return (true, result!.Item2!, "");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (InvalidCastException ex)
        {
            Log.Error(ex, "CreateGLFromInvoiceHelper CreateGLFromInvoice InvalidCastException (decimal to int?)");
            var lineInfo = ex.StackTrace?.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).FirstOrDefault(s => s.Contains("CreateGLFromInvoice"));
            return (false, null!, $"Conversion error (decimal to int): {ex.Message}. Check: {lineInfo?.Trim() ?? ex.StackTrace}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CreateGLFromInvoiceHelper CreateGLFromInvoice Error");
            return (false, null!, ex.Message);
        }
    }

    /// <summary>Typed row for invoice line in GL (avoids dynamic decimal→int at Sum).</summary>
    private sealed class InvoiceLineForGL
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
        public int? RevenueAccountId { get; set; }
        public int? ExpenseAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}

