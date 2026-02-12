using Dapper;
using Microsoft.Data.SqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services.GeneralLedger;

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
                // Get invoice data
                string SQLInvoice = $@"SELECT * FROM Invoice WHERE Id = {invoiceId} AND IsSoftDeleted = 0";
                var invoice = (await connection.QueryAsync<InvoiceModel>(SQLInvoice, transaction: transaction))?.FirstOrDefault();
                
                if (invoice == null || invoice.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "Invoice not found.");
                }

                // Check if already posted
                if (invoice.IsPostedToGL == 1)
                {
                    transaction.Rollback();
                    return (false, null!, "Invoice is already posted to General Ledger.");
                }

                // Validate period for SALE or PURCHASE invoice module
                string moduleType = invoice.InvoiceType?.ToUpper() == "PURCHASE" ? "PURCHASE" : "SALE";
                var periodCheck = await service.ValidatePeriod(invoice.OrganizationId, invoice.TranDate ?? DateTime.Now, moduleType);
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
                
                GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
                {
                    OrganizationId = invoice.OrganizationId,
                    EntryNo = entryNo,
                    EntryDate = invoice.TranDate ?? DateTime.Now,
                    Source = "INVOICE",
                    Description = $"Invoice {invoice.Code}",
                    ReferenceNo = invoice.Code,
                    ReferenceType = "INVOICE",
                    ReferenceId = invoice.Id,
                    PartyId = invoice.PartyId,
                    LocationId = invoice.LocationId,
                    BaseCurrencyId = invoice.BaseCurrencyId,
                    EnteredCurrencyId = invoice.EnteredCurrencyId,
                    CreatedBy = userId,
                    CreatedFrom = clientInfo,
                    Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
                };

                // Map invoice to GL entry - Priority: Invoice.AccountId -> Party.AccountId -> InterfaceType lookup
                int arApAccountId = 0;
                string accountTypeName = "";
                bool isSale = invoice.InvoiceType?.ToUpper() == "SALE";
                
                // First Priority: Use AccountId from invoice (set from party when selected)
                if (invoice.AccountId > 0)
                {
                    // Use the account specified in the invoice
                    arApAccountId = invoice.AccountId;
                    // Verify the account exists and get its code/name/type
                    string sqlVerifyAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                 WHERE Id = {arApAccountId} AND OrganizationId = {invoice.OrganizationId} AND IsActive = 1";
                    var verifyResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlVerifyAccount, transaction: transaction);
                    if (verifyResult == null || verifyResult.Value.Id == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"Account ID {arApAccountId} specified in invoice is not valid or inactive. Please select a valid account.");
                    }
                    accountTypeName = verifyResult.Value.Name;
                }
                // Second Priority: Get AccountId from Party if invoice doesn't have it
                else if (invoice.PartyId > 0)
                {
                    string sqlPartyAccount = $@"SELECT TOP 1 AccountId FROM Parties 
                                                WHERE Id = {invoice.PartyId} 
                                                AND OrganizationId = {invoice.OrganizationId} 
                                                AND IsSoftDeleted = 0
                                                AND AccountId IS NOT NULL 
                                                AND AccountId > 0";
                    var partyAccountId = await connection.QueryFirstOrDefaultAsync<int?>(sqlPartyAccount, transaction: transaction);
                    
                    if (partyAccountId.HasValue && partyAccountId.Value > 0)
                    {
                        arApAccountId = partyAccountId.Value;
                        // Verify the account exists and get its code/name/type
                        string sqlVerifyAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                     WHERE Id = {arApAccountId} AND OrganizationId = {invoice.OrganizationId} AND IsActive = 1";
                        var verifyResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlVerifyAccount, transaction: transaction);
                        if (verifyResult == null || verifyResult.Value.Id == 0)
                        {
                            transaction.Rollback();
                            return (false, null!, $"Account ID {arApAccountId} from party is not valid or inactive. Please update the party's account.");
                        }
                        accountTypeName = verifyResult.Value.Name;
                    }
                }
                
                // Third Priority: Fallback to AR/AP lookup by InterfaceType if AccountId not found
                if (arApAccountId == 0)
                {
                    // Use InterfaceType = 'ACCOUNTS RECEIVABLE' or 'ACCOUNTS PAYABLE'
                    string interfaceType = isSale ? "ACCOUNTS RECEIVABLE" : "ACCOUNTS PAYABLE";
                    accountTypeName = isSale ? "Accounts Receivable" : "Accounts Payable";
                    string sqlAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.OrganizationId} 
                                          AND InterfaceType = '{interfaceType}'
                                          AND IsActive = 1
                                          ORDER BY Code";
                    var accountResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlAccount, transaction: transaction);
                    
                    // Validate AR/AP account ID
                    if (accountResult == null || accountResult.Value.Id == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"{accountTypeName} account not found for OrganizationId {invoice.OrganizationId}. Please configure an account with InterfaceType='{interfaceType}' in Chart of Accounts, or set AccountId on the party/invoice before posting to GL.");
                    }
                    arApAccountId = accountResult.Value.Id;
                }

                // Get AR/AP account details for GL entry
                string sqlArApDetails = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts WHERE Id = {arApAccountId}";
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
                        GLAccountLookupHelper.AddARAPEntry(glHeader, arApAccount.Value, arAmount, isSale, invoice.Code, invoice.PartyId);

                    // Credit Revenue (from InvoiceAmount)
                    // Post each invoice item separately using item-level GL account mapping
                    // Priority: Item's RevenueAccountId -> Invoice AccountId -> ITEM_REVENUE InterfaceType -> REVENUE Type
                    if (invoice.InvoiceAmount > 0)
                    {
                        // Get invoice items (same query as PURCHASE)
                        // Use pre-tax amount (Qty*UnitPrice - Discount) so tax is posted once; matches Preview and keeps journal balanced
                        string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                          ii.ManualItem,
                                                          i.Code as ItemCode, i.Name as ItemName, i.RevenueAccountId,
                                                          COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount, 0) as Amount
                                                   FROM InvoiceDetail ii
                                                   LEFT JOIN Items i ON ii.ItemId = i.Id
                                                   WHERE ii.InvoiceId = {invoice.Id} AND ii.IsSoftDeleted = 0
                                                   ORDER BY ii.Id";
                        var invoiceItems = (await connection.QueryAsync<dynamic>(sqlInvoiceItems, transaction: transaction))?.ToList();

                        if (invoiceItems != null && invoiceItems.Count > 0)
                        {
                            // Normalize line amounts so that sum(lines) == invoice.InvoiceAmount (to avoid rounding / storage mismatches)
                            var positiveItems = invoiceItems
                                .Where(i => Convert.ToDecimal(i.Amount ?? 0) > 0)
                                .ToList();

                            decimal targetInvoiceAmount = invoice.InvoiceAmount;
                            decimal sumLineAmounts = positiveItems.Sum(i => Convert.ToDecimal(i.Amount ?? 0));

                            bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetInvoiceAmount) > 0.01m;
                            decimal scaleFactor = needsScaling ? (targetInvoiceAmount / sumLineAmounts) : 1m;
                            decimal allocated = 0m;

                            for (int idx = 0; idx < positiveItems.Count; idx++)
                            {
                                var item = positiveItems[idx];

                                decimal rawLineAmount = Convert.ToDecimal(item.Amount ?? 0);
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

                                int revenueAccountId = 0;
                                string revenueCode = "";
                                string revenueName = "";
                                string revenueType = "";

                                // First Priority: Use Item's RevenueAccountId (if configured)
                                int? itemRevenueAccountId = item.RevenueAccountId != null && !Convert.IsDBNull(item.RevenueAccountId) ? Convert.ToInt32(item.RevenueAccountId) : (int?)null;
                                if (itemRevenueAccountId.HasValue && itemRevenueAccountId.Value > 0)
                                {
                                    string itemAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                              WHERE Id = {itemRevenueAccountId.Value} 
                                                              AND OrganizationId = {invoice.OrganizationId} 
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

                                // Second Priority: Use Invoice's AccountId (if specified)
                                if (revenueAccountId == 0 && invoice.AccountId > 0)
                                {
                                    string accountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                           WHERE Id = {invoice.AccountId} 
                                                           AND OrganizationId = {invoice.OrganizationId} 
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

                                // Third Priority: Look for MANUAL ITEM REVENUE InterfaceType (dedicated manual item revenue account)
                                if (revenueAccountId == 0)
                                {
                                    string itemRevenueSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                              WHERE OrganizationId = {invoice.OrganizationId} 
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
                                    GLAccountLookupHelper.AddRevenueEntry(glHeader, (revenueAccountId, revenueCode, revenueName, revenueType), lineAmount, item.ItemCode ?? item.ItemName ?? "Manual Item", invoice.Code);
                                }
                            }
                        }
                    }

                    // Credit Tax accounts
                    if (invoice.TaxAmount > 0)
                    {
                        string taxSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.OrganizationId} 
                                          AND InterfaceType = 'TAX'
                                          AND IsActive = 1
                                          ORDER BY Code";
                        var taxAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(taxSql, transaction: transaction);
                        
                        if (taxAccount != null && taxAccount.Value.Id > 0)
                        {
                            GLAccountLookupHelper.AddTaxEntry(glHeader, taxAccount.Value, invoice.TaxAmount, isSale, invoice.Code);
                        }
                    }

                    // Credit Service Charges
                    if (invoice.ChargesAmount > 0)
                    {
                        string chargeSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                             WHERE OrganizationId = {invoice.OrganizationId} 
                                             AND InterfaceType = 'SERVICE'
                                             AND IsActive = 1
                                             ORDER BY Code";
                        var chargeAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(chargeSql, transaction: transaction);
                        
                        if (chargeAccount != null && chargeAccount.Value.Id > 0)
                        {
                            GLAccountLookupHelper.AddServiceChargesEntry(glHeader, chargeAccount.Value, invoice.ChargesAmount, isSale, invoice.Code);
                        }
                    }

                    // Debit Discount
                    if (invoice.DiscountAmount > 0)
                    {
                        string discountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                               WHERE OrganizationId = {invoice.OrganizationId} 
                                               AND InterfaceType = 'DISCOUNT'
                                               AND IsActive = 1
                                               ORDER BY Code";
                        var discountAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(discountSql, transaction: transaction);
                        
                        if (discountAccount != null && discountAccount.Value.Id > 0)
                        {
                            GLAccountLookupHelper.AddDiscountEntry(glHeader, discountAccount, invoice.DiscountAmount, isSale, invoice.Code);
                        }
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
                        GLAccountLookupHelper.AddARAPEntry(glHeader, arApAccount.Value, apAmount, isSale, invoice.Code, invoice.PartyId);

                    // Debit Expense (from InvoiceAmount)
                    // Post each invoice item separately using item-level GL account mapping
                    // Priority: Item's ExpenseAccountId -> Invoice AccountId -> ITEM_EXPENSE InterfaceType -> EXPENSE Type
                    
                    // Get invoice items
                    // Use pre-tax amount (Qty*UnitPrice - Discount) so tax is posted once; matches Preview and keeps journal balanced
                    string sqlInvoiceItems = $@"SELECT ii.Id, ii.ItemId, ii.Qty, ii.UnitPrice, ii.DiscountAmount,
                                                      ii.ManualItem,
                                                      i.Code as ItemCode, i.Name as ItemName, i.ExpenseAccountId,
                                                      COALESCE((ii.Qty * ii.UnitPrice) - ii.DiscountAmount, 0) as Amount
                                               FROM InvoiceDetail ii
                                               LEFT JOIN Items i ON ii.ItemId = i.Id
                                               WHERE ii.InvoiceId = {invoice.Id} AND ii.IsSoftDeleted = 0
                                               ORDER BY ii.Id";
                    var invoiceItems = (await connection.QueryAsync<dynamic>(sqlInvoiceItems, transaction: transaction))?.ToList();

                    if (invoiceItems != null && invoiceItems.Count > 0)
                    {
                        // Normalize line amounts so that sum(lines) == invoice.InvoiceAmount (to avoid rounding / storage mismatches)
                        var positiveItems = invoiceItems
                            .Where(i => Convert.ToDecimal(i.Amount ?? 0) > 0)
                            .ToList();

                        decimal targetInvoiceAmount = invoice.InvoiceAmount;
                        decimal sumLineAmounts = positiveItems.Sum(i => Convert.ToDecimal(i.Amount ?? 0));

                        bool needsScaling = sumLineAmounts > 0 && Math.Abs(sumLineAmounts - targetInvoiceAmount) > 0.01m;
                        decimal scaleFactor = needsScaling ? (targetInvoiceAmount / sumLineAmounts) : 1m;
                        decimal allocated = 0m;

                        for (int idx = 0; idx < positiveItems.Count; idx++)
                        {
                            var item = positiveItems[idx];

                            decimal rawLineAmount = Convert.ToDecimal(item.Amount ?? 0);
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

                            // First Priority: Use Item's ExpenseAccountId (if configured)
                            int? itemExpenseAccountId = item.ExpenseAccountId != null && !Convert.IsDBNull(item.ExpenseAccountId) ? Convert.ToInt32(item.ExpenseAccountId) : (int?)null;
                            if (itemExpenseAccountId.HasValue && itemExpenseAccountId.Value > 0)
                            {
                                string itemAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                          WHERE Id = {itemExpenseAccountId.Value} 
                                                          AND OrganizationId = {invoice.OrganizationId} 
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

                            // Second Priority: Use Invoice's AccountId (if specified)
                            if (expenseAccountId == 0 && invoice.AccountId > 0)
                            {
                                string accountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                       WHERE Id = {invoice.AccountId} 
                                                       AND OrganizationId = {invoice.OrganizationId} 
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

                            // Third Priority: Look for MANUAL ITEM EXPENSE InterfaceType (dedicated manual item expense account)
                            if (expenseAccountId == 0)
                            {
                                string itemExpenseSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                          WHERE OrganizationId = {invoice.OrganizationId} 
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
                                GLAccountLookupHelper.AddExpenseEntry(glHeader, (expenseAccountId, expenseCode, expenseName, expenseType), lineAmount, item.ItemCode ?? item.ItemName ?? "Manual Item", invoice.Code);
                            }
                        }
                    }

                    // Debit Tax Payable accounts (if applicable - some orgs may need this in AP instead)
                    if (invoice.TaxAmount > 0)
                    {
                        string taxSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.OrganizationId} 
                                          AND InterfaceType = 'TAX'
                                          AND IsActive = 1
                                          ORDER BY Code";
                        var taxAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(taxSql, transaction: transaction);
                        
                        if (taxAccount != null && taxAccount.Value.Id > 0)
                        {
                            GLAccountLookupHelper.AddTaxEntry(glHeader, taxAccount.Value, invoice.TaxAmount, isSale, invoice.Code);
                        }
                    }

                    // Debit Service Charges
                    if (invoice.ChargesAmount > 0)
                    {
                        string chargeSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                             WHERE OrganizationId = {invoice.OrganizationId} 
                                             AND InterfaceType = 'SERVICE'
                                             AND IsActive = 1
                                             ORDER BY Code";
                        var chargeAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(chargeSql, transaction: transaction);
                        
                        if (chargeAccount != null && chargeAccount.Value.Id > 0)
                        {
                            GLAccountLookupHelper.AddServiceChargesEntry(glHeader, chargeAccount.Value, invoice.ChargesAmount, isSale, invoice.Code);
                        }
                    }

                    // Credit Discount (reduces the AP liability)
                    if (invoice.DiscountAmount > 0)
                    {
                        string discountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                               WHERE OrganizationId = {invoice.OrganizationId} 
                                               AND InterfaceType = 'DISCOUNT'
                                               AND IsActive = 1
                                               ORDER BY Code";
                        var discountAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(discountSql, transaction: transaction);
                        
                        if (discountAccount != null && discountAccount.Value.Id > 0)
                        {
                            GLAccountLookupHelper.AddDiscountEntry(glHeader, discountAccount, invoice.DiscountAmount, isSale, invoice.Code);
                        }
                    }
                }

                if (glHeader.Details == null || glHeader.Details.Count == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "No GL details generated. Please ensure invoice items and required accounts are configured before posting.");
                }

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
                int headerId = Convert.ToInt32(await connection.ExecuteScalarAsync("SELECT SCOPE_IDENTITY();", transaction: transaction));

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
        catch (Exception ex)
        {
            Log.Error(ex, "CreateGLFromInvoiceHelper CreateGLFromInvoice Error");
            return (false, null!, ex.Message);
        }
    }

}
