using Dapper;
using Microsoft.Data.SqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services.GeneralLedger;

/// <summary>
/// Common helper functions for General Ledger account lookups
/// Used by both CreateGLFrom... and PreviewGLFrom... helper classes
/// </summary>
public static class GLAccountLookupHelper
{
    #region Account Lookup by InterfaceType (DapperFunctions - for Preview methods)

    /// <summary>
    /// Get account by InterfaceType using DapperFunctions (for Preview methods)
    /// </summary>
    public static async Task<ChartOfAccountsModel?> GetAccountByInterfaceType(
        DapperFunctions dapper,
        int organizationId,
        string interfaceType,
        bool required = false)
    {
        string sql = $@"SELECT TOP 1 Id, Code, Name, Type, InterfaceType FROM ChartOfAccounts 
                       WHERE OrganizationId = {organizationId} 
                       AND InterfaceType = '{interfaceType}'
                       AND IsActive = 1
                       ORDER BY Code";
        
        var result = await dapper.SearchByQuery<ChartOfAccountsModel>(sql);
        var account = result?.FirstOrDefault();
        
        if (required && (account == null || account.Id == 0))
        {
            Log.Warning($"GLAccountLookupHelper: Account with InterfaceType='{interfaceType}' not found for OrganizationId {organizationId}.");
        }
        
        return account;
    }

    /// <summary>
    /// Get Tax account using DapperFunctions
    /// </summary>
    public static async Task<ChartOfAccountsModel?> GetTaxAccount(
        DapperFunctions dapper,
        int organizationId,
        bool required = false)
    {
        return await GetAccountByInterfaceType(dapper, organizationId, "TAX", required);
    }

    /// <summary>
    /// Get Service account using DapperFunctions
    /// </summary>
    public static async Task<ChartOfAccountsModel?> GetServiceAccount(
        DapperFunctions dapper,
        int organizationId,
        bool required = false)
    {
        return await GetAccountByInterfaceType(dapper, organizationId, "SERVICE", required);
    }

    /// <summary>
    /// Get Discount account using DapperFunctions
    /// </summary>
    public static async Task<ChartOfAccountsModel?> GetDiscountAccount(
        DapperFunctions dapper,
        int organizationId,
        bool required = false)
    {
        return await GetAccountByInterfaceType(dapper, organizationId, "DISCOUNT", required);
    }

    /// <summary>
    /// Get Payment Method account by name matching using DapperFunctions
    /// </summary>
    public static async Task<ChartOfAccountsModel?> GetPaymentMethodAccount(
        DapperFunctions dapper,
        int organizationId,
        string paymentMethodName,
        bool required = false)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodName))
        {
            if (required)
            {
                Log.Warning("GLAccountLookupHelper: PaymentMethod name is required but was empty.");
            }
            return null;
        }

        string paymentMethodUpper = paymentMethodName.ToUpper().Replace("'", "''");
        string sql = $@"SELECT TOP 1 coa.* FROM ChartOfAccounts coa 
                        WHERE coa.OrganizationId = {organizationId} 
                        AND coa.InterfaceType = 'PAYMENT METHOD'
                        AND coa.IsActive = 1
                        AND (
                            UPPER(coa.Name) = '{paymentMethodUpper}'
                            OR UPPER(coa.Name) LIKE '%{paymentMethodUpper}%'
                            OR UPPER(coa.Name) LIKE '%{paymentMethodUpper.Replace(" ", "%")}%'
                            OR '{paymentMethodUpper}' LIKE '%' + UPPER(coa.Name) + '%'
                        )
                        ORDER BY 
                            CASE 
                                WHEN UPPER(coa.Name) = '{paymentMethodUpper}' THEN 1
                                WHEN UPPER(coa.Name) LIKE '{paymentMethodUpper}%' THEN 2
                                WHEN UPPER(coa.Name) LIKE '%{paymentMethodUpper}%' THEN 3
                                WHEN '{paymentMethodUpper}' LIKE '%' + UPPER(coa.Name) + '%' THEN 4
                                ELSE 5
                            END,
                            coa.Code";
        
        var result = await dapper.SearchByQuery<ChartOfAccountsModel>(sql);
        var account = result?.FirstOrDefault();
        
        // Fallback to any payment method account if no match found
        if (account == null || account.Id == 0)
        {
            string fallbackSql = $@"SELECT TOP 1 coa.* FROM ChartOfAccounts coa 
                                    WHERE coa.OrganizationId = {organizationId} 
                                    AND coa.InterfaceType = 'PAYMENT METHOD'
                                    AND coa.IsActive = 1
                                    ORDER BY coa.Code";
            var fallbackResult = await dapper.SearchByQuery<ChartOfAccountsModel>(fallbackSql);
            account = fallbackResult?.FirstOrDefault();
        }
        
        if (required && (account == null || account.Id == 0))
        {
            Log.Warning($"GLAccountLookupHelper: Payment Method account not found for OrganizationId {organizationId} with PaymentMethod='{paymentMethodName}'.");
        }
        
        return account;
    }

    /// <summary>
    /// Get Payment Method account by AccountId using DapperFunctions
    /// </summary>
    public static async Task<ChartOfAccountsModel?> GetPaymentMethodAccountById(
        DapperFunctions dapper,
        int organizationId,
        int accountId,
        bool required = false)
    {
        string sql = $@"SELECT TOP 1 Id, Code, Name, Type, InterfaceType FROM ChartOfAccounts 
                       WHERE Id = {accountId} 
                       AND OrganizationId = {organizationId} 
                       AND InterfaceType = 'PAYMENT METHOD'
                       AND IsActive = 1";
        
        var result = await dapper.SearchByQuery<ChartOfAccountsModel>(sql);
        var account = result?.FirstOrDefault();
        
        if (required && (account == null || account.Id == 0))
        {
            Log.Warning($"GLAccountLookupHelper: Payment Method account with Id={accountId} not found or does not have InterfaceType='PAYMENT METHOD' for OrganizationId {organizationId}.");
        }
        
        return account;
    }

    /// <summary>
    /// Get AR/AP account with priority: Invoice.AccountId -> Party.AccountId -> InterfaceType lookup
    /// Using DapperFunctions (for Preview methods)
    /// </summary>
    public static async Task<(bool Success, ChartOfAccountsModel? Account, string ErrorMessage)> GetARAPAccount(
        DapperFunctions dapper,
        int organizationId,
        bool isSale,
        int? invoiceAccountId = null,
        int? partyId = null)
    {
        ChartOfAccountsModel? arApAccount = null;
        string expectedInterfaceType = isSale ? "ACCOUNTS RECEIVABLE" : "ACCOUNTS PAYABLE";
        string accountTypeName = isSale ? "Accounts Receivable" : "Accounts Payable";

        // Priority 1: Use AccountId from invoice (set from party when selected)
        if (invoiceAccountId.HasValue && invoiceAccountId.Value > 0)
        {
            string sqlAccountById = $@"SELECT TOP 1 Id, Code, Name, Type, InterfaceType FROM ChartOfAccounts 
                                       WHERE Id = {invoiceAccountId.Value} 
                                       AND OrganizationId = {organizationId} 
                                       AND IsActive = 1
                                       AND InterfaceType = '{expectedInterfaceType}'";
            var accountResultById = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccountById);
            arApAccount = accountResultById?.FirstOrDefault();
            
            if (arApAccount == null || arApAccount.Id == 0)
            {
                Log.Warning($"GLAccountLookupHelper: Invoice.AccountId {invoiceAccountId.Value} does not have InterfaceType='{expectedInterfaceType}'. Falling back to Party.AccountId or InterfaceType lookup.");
                arApAccount = null;
            }
        }

        // Priority 2: Get AccountId from Party if invoice doesn't have it (or invoice.AccountId is invalid)
        if ((arApAccount == null || arApAccount.Id == 0) && partyId.HasValue && partyId.Value > 0)
        {
            string sqlPartyAccount = $@"SELECT TOP 1 p.AccountId, coa.Id, coa.Code, coa.Name, coa.Type, coa.InterfaceType
                                        FROM Parties p
                                        INNER JOIN ChartOfAccounts coa ON coa.Id = p.AccountId
                                        WHERE p.Id = {partyId.Value} 
                                        AND p.OrganizationId = {organizationId} 
                                        AND p.IsSoftDeleted = 0
                                        AND p.AccountId IS NOT NULL 
                                        AND p.AccountId > 0
                                        AND coa.IsActive = 1
                                        AND coa.InterfaceType = '{expectedInterfaceType}'";
            var partyAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlPartyAccount);
            arApAccount = partyAccountResult?.FirstOrDefault();
            
            if (arApAccount == null || arApAccount.Id == 0)
            {
                Log.Warning($"GLAccountLookupHelper: Party {partyId.Value} does not have AccountId with InterfaceType='{expectedInterfaceType}'. Falling back to InterfaceType lookup.");
            }
        }

        // Priority 3: Fallback to ChartOfAccounts.InterfaceType lookup if AccountId not found
        if (arApAccount == null || arApAccount.Id == 0)
        {
            string interfaceType = isSale ? "ACCOUNTS RECEIVABLE" : "ACCOUNTS PAYABLE";
            string sqlAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                  WHERE OrganizationId = {organizationId} 
                                  AND InterfaceType = '{interfaceType}'
                                  AND IsActive = 1
                                  ORDER BY Code";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccount);
            arApAccount = accountResult?.FirstOrDefault();
            
            if (arApAccount == null || arApAccount.Id == 0)
            {
                string errorMsg = $"{accountTypeName} account not found for OrganizationId {organizationId}. Please configure an account with InterfaceType='{interfaceType}' in Chart of Accounts, or set AccountId on the party/invoice.";
                Log.Warning($"GLAccountLookupHelper: {errorMsg}");
                return (false, null, errorMsg);
            }
        }

        return (true, arApAccount, "");
    }

    public static async Task<(bool Success, ChartOfAccountsModel? Account, string ErrorMessage)> GetRevenueAccount(
        DapperFunctions dapper,
        int organizationId,
        int? itemId = null,
        int? itemRevenueAccountId = null,
        int? invoiceAccountId = null,
        string? itemName = null)
    {
        ChartOfAccountsModel? revenueAccount = null;
        int revenueAccountId = 0;

        // Priority 1: Use Item's RevenueAccountId (if item is selected and account is configured)
        if (itemId.HasValue && itemId.Value > 0)
        {
            int? actualRevenueAccountId = itemRevenueAccountId;
            
            if (!actualRevenueAccountId.HasValue || actualRevenueAccountId.Value == 0)
            {
                string itemAccountLookupSql = $@"SELECT TOP 1 RevenueAccountId FROM Items 
                                                  WHERE Id = {itemId.Value} 
                                                  AND OrganizationId = {organizationId} 
                                                  AND IsSoftDeleted = 0";
                var itemAccountLookup = await dapper.SearchByQuery<dynamic>(itemAccountLookupSql);
                if (itemAccountLookup != null && itemAccountLookup.Any())
                {
                    var itemData = itemAccountLookup.FirstOrDefault();
                    if (itemData?.RevenueAccountId != null && !Convert.IsDBNull(itemData.RevenueAccountId))
                    {
                        actualRevenueAccountId = Convert.ToInt32(itemData.RevenueAccountId);
                    }
                }
            }
            
            // Use the RevenueAccountId if found
            if (actualRevenueAccountId.HasValue && actualRevenueAccountId.Value > 0)
            {
                string itemAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE Id = {actualRevenueAccountId.Value} 
                                          AND OrganizationId = {organizationId} 
                                          AND IsActive = 1";
                var itemAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(itemAccountSql);
                revenueAccount = itemAccountResult?.FirstOrDefault();
                
                if (revenueAccount != null && revenueAccount.Id > 0)
                {
                    revenueAccountId = revenueAccount.Id;
                }
            }
        }

        // Priority 2: Use Invoice's AccountId (if specified)
        if (revenueAccountId == 0 && invoiceAccountId.HasValue && invoiceAccountId.Value > 0)
        {
            string accountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                   WHERE Id = {invoiceAccountId.Value} 
                                   AND OrganizationId = {organizationId} 
                                   AND IsActive = 1";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(accountSql);
            if (accountResult != null && accountResult.Any())
            {
                revenueAccount = accountResult.FirstOrDefault();
                if (revenueAccount != null && revenueAccount.Id > 0)
                {
                    revenueAccountId = revenueAccount.Id;
                }
            }
        }

        // Priority 3: Look for MANUAL ITEM REVENUE InterfaceType
        if (revenueAccountId == 0)
        {
            revenueAccount = await GetAccountByInterfaceType(dapper, organizationId, "MANUAL ITEM REVENUE", false);
            if (revenueAccount != null && revenueAccount.Id > 0)
            {
                revenueAccountId = revenueAccount.Id;
            }
        }

        if (revenueAccountId == 0)
        {
            string itemNameStr = itemName ?? "Unknown Item";
            string errorMsg = $"Revenue account not found for item '{itemNameStr}'. Please configure a RevenueAccountId on the item, or set AccountId on the invoice, or configure an account with InterfaceType='MANUAL ITEM REVENUE' in Chart of Accounts for OrganizationId {organizationId}.";
            Log.Warning($"GLAccountLookupHelper: {errorMsg}");
            return (false, null, errorMsg);
        }

        return (true, revenueAccount, "");
    }

    /// <summary>
    /// Get Expense account for item with priority: Items.ExpenseAccountId -> Invoice.AccountId -> InterfaceType('MANUAL ITEM EXPENSE')
    /// Using DapperFunctions (for Preview methods)
    /// </summary>
    public static async Task<(bool Success, ChartOfAccountsModel? Account, string ErrorMessage)> GetExpenseAccount(
        DapperFunctions dapper,
        int organizationId,
        int? itemId = null,
        int? itemExpenseAccountId = null,
        int? invoiceAccountId = null,
        string? itemName = null)
    {
        ChartOfAccountsModel? expenseAccount = null;
        int expenseAccountId = 0;

        // Priority 1: Use Item's ExpenseAccountId (if item is selected and account is configured)
        if (itemId.HasValue && itemId.Value > 0)
        {
            int? actualExpenseAccountId = itemExpenseAccountId;
            
            // If not provided, fetch from Items table
            if (!actualExpenseAccountId.HasValue || actualExpenseAccountId.Value == 0)
            {
                string itemAccountLookupSql = $@"SELECT TOP 1 ExpenseAccountId FROM Items 
                                                  WHERE Id = {itemId.Value} 
                                                  AND OrganizationId = {organizationId} 
                                                  AND IsSoftDeleted = 0";
                var itemAccountLookup = await dapper.SearchByQuery<dynamic>(itemAccountLookupSql);
                if (itemAccountLookup != null && itemAccountLookup.Any())
                {
                    var itemData = itemAccountLookup.FirstOrDefault();
                    if (itemData?.ExpenseAccountId != null && !Convert.IsDBNull(itemData.ExpenseAccountId))
                    {
                        actualExpenseAccountId = Convert.ToInt32(itemData.ExpenseAccountId);
                    }
                }
            }
            
            // Use the ExpenseAccountId if found
            if (actualExpenseAccountId.HasValue && actualExpenseAccountId.Value > 0)
            {
                string itemAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE Id = {actualExpenseAccountId.Value} 
                                          AND OrganizationId = {organizationId} 
                                          AND IsActive = 1";
                var itemAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(itemAccountSql);
                expenseAccount = itemAccountResult?.FirstOrDefault();
                
                if (expenseAccount != null && expenseAccount.Id > 0)
                {
                    expenseAccountId = expenseAccount.Id;
                }
            }
        }

        // Priority 2: Use Invoice's AccountId (if specified)
        if (expenseAccountId == 0 && invoiceAccountId.HasValue && invoiceAccountId.Value > 0)
        {
            string accountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                   WHERE Id = {invoiceAccountId.Value} 
                                   AND OrganizationId = {organizationId} 
                                   AND IsActive = 1";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(accountSql);
            if (accountResult != null && accountResult.Any())
            {
                expenseAccount = accountResult.FirstOrDefault();
                if (expenseAccount != null && expenseAccount.Id > 0)
                {
                    expenseAccountId = expenseAccount.Id;
                }
            }
        }

        // Priority 3: Look for MANUAL ITEM EXPENSE InterfaceType
        if (expenseAccountId == 0)
        {
            expenseAccount = await GetAccountByInterfaceType(dapper, organizationId, "MANUAL ITEM EXPENSE", false);
            if (expenseAccount != null && expenseAccount.Id > 0)
            {
                expenseAccountId = expenseAccount.Id;
            }
        }

        if (expenseAccountId == 0)
        {
            string itemNameStr = itemName ?? "Unknown Item";
            string errorMsg = $"Expense account not found for item '{itemNameStr}'. Please configure an ExpenseAccountId on the item, or set AccountId on the invoice, or configure an account with InterfaceType='MANUAL ITEM EXPENSE' in Chart of Accounts for OrganizationId {organizationId}.";
            Log.Warning($"GLAccountLookupHelper: {errorMsg}");
            return (false, null, errorMsg);
        }

        return (true, expenseAccount, "");
    }

    /// <summary>
    /// Get account by ID using DapperFunctions
    /// </summary>
    public static async Task<ChartOfAccountsModel?> GetAccountById(
        DapperFunctions dapper,
        int organizationId,
        int accountId,
        bool required = false)
    {
        string sql = $@"SELECT TOP 1 Id, Code, Name, Type, InterfaceType FROM ChartOfAccounts 
                       WHERE Id = {accountId} 
                       AND OrganizationId = {organizationId} 
                       AND IsActive = 1";
        
        var result = await dapper.SearchByQuery<ChartOfAccountsModel>(sql);
        var account = result?.FirstOrDefault();
        
        if (required && (account == null || account.Id == 0))
        {
            Log.Warning($"GLAccountLookupHelper: Account with Id={accountId} not found or inactive for OrganizationId {organizationId}.");
        }
        
        return account;
    }

    #endregion

    #region Account Lookup by InterfaceType (SqlConnection - for Create methods)

    /// <summary>
    /// Get account by InterfaceType using SqlConnection (for Create methods)
    /// </summary>
    public static async Task<(int Id, string Code, string Name, string Type)?> GetAccountByInterfaceType(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId,
        string interfaceType)
    {
        string sql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                       WHERE OrganizationId = {organizationId} 
                       AND InterfaceType = '{interfaceType}'
                       AND IsActive = 1
                       ORDER BY Code";
        
        return await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sql, transaction: transaction);
    }

    /// <summary>
    /// Get Tax account using SqlConnection
    /// </summary>
    public static async Task<(int Id, string Code, string Name, string Type)?> GetTaxAccount(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId)
    {
        return await GetAccountByInterfaceType(connection, transaction, organizationId, "TAX");
    }

    /// <summary>
    /// Get Service account using SqlConnection
    /// </summary>
    public static async Task<(int Id, string Code, string Name, string Type)?> GetServiceAccount(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId)
    {
        return await GetAccountByInterfaceType(connection, transaction, organizationId, "SERVICE");
    }

    /// <summary>
    /// Get Discount account using SqlConnection
    /// </summary>
    public static async Task<(int Id, string Code, string Name, string Type)?> GetDiscountAccount(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId)
    {
        return await GetAccountByInterfaceType(connection, transaction, organizationId, "DISCOUNT");
    }

    /// <summary>
    /// Get Payment Method account by name matching using SqlConnection
    /// </summary>
    public static async Task<(int Id, string Code, string Name, string Type)?> GetPaymentMethodAccount(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId,
        string paymentMethodName)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodName))
        {
            return null;
        }

        string paymentMethodUpper = paymentMethodName.ToUpper().Replace("'", "''");
        string sql = $@"SELECT TOP 1 coa.Id, coa.Code, coa.Name, coa.Type FROM ChartOfAccounts coa 
                        WHERE coa.OrganizationId = {organizationId} 
                        AND coa.InterfaceType = 'PAYMENT METHOD'
                        AND coa.IsActive = 1
                        AND (
                            UPPER(coa.Name) = '{paymentMethodUpper}'
                            OR UPPER(coa.Name) LIKE '%{paymentMethodUpper}%'
                            OR UPPER(coa.Name) LIKE '%{paymentMethodUpper.Replace(" ", "%")}%'
                            OR '{paymentMethodUpper}' LIKE '%' + UPPER(coa.Name) + '%'
                        )
                        ORDER BY 
                            CASE 
                                WHEN UPPER(coa.Name) = '{paymentMethodUpper}' THEN 1
                                WHEN UPPER(coa.Name) LIKE '{paymentMethodUpper}%' THEN 2
                                WHEN UPPER(coa.Name) LIKE '%{paymentMethodUpper}%' THEN 3
                                WHEN '{paymentMethodUpper}' LIKE '%' + UPPER(coa.Name) + '%' THEN 4
                                ELSE 5
                            END,
                            coa.Code";
        
        var account = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sql, transaction: transaction);
        
        // Fallback to any payment method account if no match found
        if (account == null || account.Value.Id == 0)
        {
            string fallbackSql = $@"SELECT TOP 1 coa.Id, coa.Code, coa.Name, coa.Type FROM ChartOfAccounts coa 
                                    WHERE coa.OrganizationId = {organizationId} 
                                    AND coa.InterfaceType = 'PAYMENT METHOD'
                                    AND coa.IsActive = 1
                                    ORDER BY coa.Code";
            account = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(fallbackSql, transaction: transaction);
        }
        
        return account;
    }

    /// <summary>
    /// Get AR/AP account with priority: Invoice.AccountId -> Party.AccountId -> InterfaceType lookup
    /// Using SqlConnection (for Create methods)
    /// </summary>
    public static async Task<(bool Success, int AccountId, string AccountCode, string AccountName, string AccountType, string ErrorMessage)> GetARAPAccount(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId,
        bool isSale,
        int? invoiceAccountId = null,
        int? partyId = null)
    {
        int arApAccountId = 0;
        string accountCode = "";
        string accountName = "";
        string accountType = "";
        string expectedInterfaceType = isSale ? "ACCOUNTS RECEIVABLE" : "ACCOUNTS PAYABLE";
        string accountTypeName = isSale ? "Accounts Receivable" : "Accounts Payable";

        // Priority 1: Use AccountId from invoice (set from party when selected)
        if (invoiceAccountId.HasValue && invoiceAccountId.Value > 0)
        {
            string sqlVerifyAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                         WHERE Id = {invoiceAccountId.Value} 
                                         AND OrganizationId = {organizationId} 
                                         AND IsActive = 1";
            var verifyResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlVerifyAccount, transaction: transaction);
            if (verifyResult != null && verifyResult.Value.Id > 0)
            {
                arApAccountId = verifyResult.Value.Id;
                accountCode = verifyResult.Value.Code;
                accountName = verifyResult.Value.Name;
                accountType = verifyResult.Value.Type;
            }
        }

        // Priority 2: Get AccountId from Party if invoice doesn't have it
        if (arApAccountId == 0 && partyId.HasValue && partyId.Value > 0)
        {
            string sqlPartyAccount = $@"SELECT TOP 1 AccountId FROM Parties 
                                       WHERE Id = {partyId.Value} 
                                       AND OrganizationId = {organizationId} 
                                       AND IsSoftDeleted = 0
                                       AND AccountId IS NOT NULL 
                                       AND AccountId > 0";
            var partyAccountId = await connection.QueryFirstOrDefaultAsync<int?>(sqlPartyAccount, transaction: transaction);
            
            if (partyAccountId.HasValue && partyAccountId.Value > 0)
            {
                arApAccountId = partyAccountId.Value;
                string sqlVerifyAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                             WHERE Id = {arApAccountId} 
                                             AND OrganizationId = {organizationId} 
                                             AND IsActive = 1";
                var verifyResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlVerifyAccount, transaction: transaction);
                if (verifyResult != null && verifyResult.Value.Id > 0)
                {
                    accountCode = verifyResult.Value.Code;
                    accountName = verifyResult.Value.Name;
                    accountType = verifyResult.Value.Type;
                }
                else
                {
                    return (false, 0, "", "", "", $"Account ID {arApAccountId} from party is not valid or inactive. Please update the party's account.");
                }
            }
        }

        // Priority 3: Fallback to AR/AP lookup by InterfaceType if AccountId not found
        if (arApAccountId == 0)
        {
            string interfaceType = isSale ? "ACCOUNTS RECEIVABLE" : "ACCOUNTS PAYABLE";
            string sqlAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                  WHERE OrganizationId = {organizationId} 
                                  AND InterfaceType = '{interfaceType}'
                                  AND IsActive = 1
                                  ORDER BY Code";
            var accountResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlAccount, transaction: transaction);
            
            if (accountResult == null || accountResult.Value.Id == 0)
            {
                string errorMsg = $"{accountTypeName} account not found for OrganizationId {organizationId}. Please configure an account with InterfaceType='{interfaceType}' in Chart of Accounts, or set AccountId on the party/invoice before posting to GL.";
                return (false, 0, "", "", "", errorMsg);
            }
            
            arApAccountId = accountResult.Value.Id;
            accountCode = accountResult.Value.Code;
            accountName = accountResult.Value.Name;
            accountType = accountResult.Value.Type;
        }

        return (true, arApAccountId, accountCode, accountName, accountType, "");
    }

    /// <summary>
    /// Get Revenue account for item with priority: Items.RevenueAccountId -> Invoice.AccountId -> InterfaceType('MANUAL ITEM REVENUE')
    /// Using SqlConnection (for Create methods)
    /// </summary>
    public static async Task<(bool Success, int AccountId, string AccountCode, string AccountName, string AccountType, string ErrorMessage)> GetRevenueAccount(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId,
        int? itemId = null,
        int? invoiceAccountId = null)
    {
        int revenueAccountId = 0;
        string revenueCode = "";
        string revenueName = "";
        string revenueType = "";

        // Priority 1: Use Item's RevenueAccountId (if item is selected and account is configured)
        if (itemId.HasValue && itemId.Value > 0)
        {
            string itemAccountLookupSql = $@"SELECT TOP 1 RevenueAccountId FROM Items 
                                              WHERE Id = {itemId.Value} 
                                              AND OrganizationId = {organizationId} 
                                              AND IsSoftDeleted = 0";
            var itemAccountLookup = await connection.QueryFirstOrDefaultAsync<int?>(itemAccountLookupSql, transaction: transaction);
            
            if (itemAccountLookup.HasValue && itemAccountLookup.Value > 0)
            {
                string itemAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE Id = {itemAccountLookup.Value} 
                                          AND OrganizationId = {organizationId} 
                                          AND IsActive = 1";
                var itemAccountResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(itemAccountSql, transaction: transaction);
                
                if (itemAccountResult != null && itemAccountResult.Value.Id > 0)
                {
                    revenueAccountId = itemAccountResult.Value.Id;
                    revenueCode = itemAccountResult.Value.Code;
                    revenueName = itemAccountResult.Value.Name;
                    revenueType = itemAccountResult.Value.Type;
                }
            }
        }

        // Priority 2: Use Invoice's AccountId (if specified)
        if (revenueAccountId == 0 && invoiceAccountId.HasValue && invoiceAccountId.Value > 0)
        {
            string accountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                   WHERE Id = {invoiceAccountId.Value} 
                                   AND OrganizationId = {organizationId} 
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

        // Priority 3: Look for MANUAL ITEM REVENUE InterfaceType
        if (revenueAccountId == 0)
        {
            var revenueAccount = await GetAccountByInterfaceType(connection, transaction, organizationId, "MANUAL ITEM REVENUE");
            if (revenueAccount != null && revenueAccount.Value.Id > 0)
            {
                revenueAccountId = revenueAccount.Value.Id;
                revenueCode = revenueAccount.Value.Code;
                revenueName = revenueAccount.Value.Name;
                revenueType = revenueAccount.Value.Type;
            }
        }

        if (revenueAccountId == 0)
        {
            string errorMsg = $"Revenue account not found. Please configure a RevenueAccountId on the item, or set AccountId on the invoice, or configure an account with InterfaceType='MANUAL ITEM REVENUE' in Chart of Accounts for OrganizationId {organizationId}.";
            return (false, 0, "", "", "", errorMsg);
        }

        return (true, revenueAccountId, revenueCode, revenueName, revenueType, "");
    }

    /// <summary>
    /// Get Expense account for item with priority: Items.ExpenseAccountId -> Invoice.AccountId -> InterfaceType('MANUAL ITEM EXPENSE')
    /// Using SqlConnection (for Create methods)
    /// </summary>
    public static async Task<(bool Success, int AccountId, string AccountCode, string AccountName, string AccountType, string ErrorMessage)> GetExpenseAccount(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId,
        int? itemId = null,
        int? invoiceAccountId = null)
    {
        int expenseAccountId = 0;
        string expenseCode = "";
        string expenseName = "";
        string expenseType = "";

        // Priority 1: Use Item's ExpenseAccountId (if item is selected and account is configured)
        if (itemId.HasValue && itemId.Value > 0)
        {
            string itemAccountLookupSql = $@"SELECT TOP 1 ExpenseAccountId FROM Items 
                                              WHERE Id = {itemId.Value} 
                                              AND OrganizationId = {organizationId} 
                                              AND IsSoftDeleted = 0";
            var itemAccountLookup = await connection.QueryFirstOrDefaultAsync<int?>(itemAccountLookupSql, transaction: transaction);
            
            if (itemAccountLookup.HasValue && itemAccountLookup.Value > 0)
            {
                string itemAccountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE Id = {itemAccountLookup.Value} 
                                          AND OrganizationId = {organizationId} 
                                          AND IsActive = 1";
                var itemAccountResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(itemAccountSql, transaction: transaction);
                
                if (itemAccountResult != null && itemAccountResult.Value.Id > 0)
                {
                    expenseAccountId = itemAccountResult.Value.Id;
                    expenseCode = itemAccountResult.Value.Code;
                    expenseName = itemAccountResult.Value.Name;
                    expenseType = itemAccountResult.Value.Type;
                }
            }
        }

        // Priority 2: Use Invoice's AccountId (if specified)
        if (expenseAccountId == 0 && invoiceAccountId.HasValue && invoiceAccountId.Value > 0)
        {
            string accountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                   WHERE Id = {invoiceAccountId.Value} 
                                   AND OrganizationId = {organizationId} 
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

        // Priority 3: Look for MANUAL ITEM EXPENSE InterfaceType
        if (expenseAccountId == 0)
        {
            var expenseAccount = await GetAccountByInterfaceType(connection, transaction, organizationId, "MANUAL ITEM EXPENSE");
            if (expenseAccount != null && expenseAccount.Value.Id > 0)
            {
                expenseAccountId = expenseAccount.Value.Id;
                expenseCode = expenseAccount.Value.Code;
                expenseName = expenseAccount.Value.Name;
                expenseType = expenseAccount.Value.Type;
            }
        }

        if (expenseAccountId == 0)
        {
            string errorMsg = $"Expense account not found. Please configure an ExpenseAccountId on the item, or set AccountId on the invoice, or configure an account with InterfaceType='MANUAL ITEM EXPENSE' in Chart of Accounts for OrganizationId {organizationId}.";
            return (false, 0, "", "", "", errorMsg);
        }

        return (true, expenseAccountId, expenseCode, expenseName, expenseType, "");
    }

    /// <summary>
    /// Get account by ID using SqlConnection
    /// </summary>
    public static async Task<(int Id, string Code, string Name, string Type)?> GetAccountById(
        SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        int organizationId,
        int accountId)
    {
        string sql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                       WHERE Id = {accountId} 
                       AND OrganizationId = {organizationId} 
                       AND IsActive = 1";
        
        return await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sql, transaction: transaction);
    }

    #endregion

    #region GL Entry Creation Helpers

    /// <summary>
    /// Add a single GL detail entry to the header
    /// </summary>
    public static void AddGLEntry(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel account,
        double debitAmount,
        double creditAmount,
        string description,
        int? partyId = null,
        int? locationId = null)
    {
        glHeader.Details.Add(new GeneralLedgerDetailModel
        {
            AccountId = account.Id,
            AccountCode = account.Code,
            AccountName = account.Name,
            AccountType = account.Type,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount,
            Description = description,
            PartyId = partyId ?? 0,
            LocationId = locationId ?? 0
        });
    }

    /// <summary>
    /// Add a single GL detail entry to the header (overload for tuple type used in CreateGLFromInvoice)
    /// </summary>
    public static void AddGLEntry(
        GeneralLedgerHeaderModel glHeader,
        (int Id, string Code, string Name, string Type) account,
        double debitAmount,
        double creditAmount,
        string description,
        int? partyId = null,
        int? locationId = null)
    {
        glHeader.Details.Add(new GeneralLedgerDetailModel
        {
            AccountId = account.Id,
            AccountCode = account.Code,
            AccountName = account.Name,
            AccountType = account.Type,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount,
            Description = description,
            PartyId = partyId ?? 0,
            LocationId = locationId ?? 0
        });
    }

    /// <summary>
    /// Add AR/AP entry (balance entry)
    /// </summary>
    public static void AddARAPEntry(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel arApAccount,
        decimal balanceAmount,
        bool isSale,
        string invoiceCode,
        int? partyId = null)
    {
        if (balanceAmount > 0)
        {
            AddGLEntry(
                glHeader,
                arApAccount,
                isSale ? (double)balanceAmount : 0,
                isSale ? 0 : (double)balanceAmount,
                $"Invoice Balance - {invoiceCode ?? "NEW"}",
                partyId,
                null);
        }
    }

    /// <summary>
    /// Add AR/AP entry (balance entry) - overload for tuple type
    /// </summary>
    public static void AddARAPEntry(
        GeneralLedgerHeaderModel glHeader,
        (int Id, string Code, string Name, string Type) arApAccount,
        decimal balanceAmount,
        bool isSale,
        string invoiceCode,
        int? partyId = null)
    {
        if (balanceAmount > 0)
        {
            AddGLEntry(
                glHeader,
                arApAccount,
                isSale ? (double)balanceAmount : 0,
                isSale ? 0 : (double)balanceAmount,
                $"Invoice {invoiceCode}",
                partyId,
                null);
        }
    }

    /// <summary>
    /// Add revenue entry for an invoice item
    /// </summary>
    public static void AddRevenueEntry(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel revenueAccount,
        decimal lineAmount,
        string itemName,
        string invoiceCode)
    {
        AddGLEntry(
            glHeader,
            revenueAccount,
            0,
            (double)lineAmount,
            $"Sales - {itemName ?? "Manual Item"} - Invoice {invoiceCode ?? "NEW"}");
    }

    /// <summary>
    /// Add revenue entry for an invoice item - overload for tuple type
    /// </summary>
    public static void AddRevenueEntry(
        GeneralLedgerHeaderModel glHeader,
        (int Id, string Code, string Name, string Type) revenueAccount,
        decimal lineAmount,
        string itemName,
        string invoiceCode)
    {
        AddGLEntry(
            glHeader,
            revenueAccount,
            0,
            (double)lineAmount,
            $"Sales - {itemName ?? "Manual Item"} - Invoice {invoiceCode}");
    }

    /// <summary>
    /// Add expense entry for an invoice item
    /// </summary>
    public static void AddExpenseEntry(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel expenseAccount,
        decimal lineAmount,
        string itemName,
        string invoiceCode)
    {
        AddGLEntry(
            glHeader,
            expenseAccount,
            (double)lineAmount,
            0,
            $"Purchase - {itemName ?? "Manual Item"} - Invoice {invoiceCode ?? "NEW"}");
    }

    /// <summary>
    /// Add expense entry for an invoice item - overload for tuple type
    /// </summary>
    public static void AddExpenseEntry(
        GeneralLedgerHeaderModel glHeader,
        (int Id, string Code, string Name, string Type) expenseAccount,
        decimal lineAmount,
        string itemName,
        string invoiceCode)
    {
        AddGLEntry(
            glHeader,
            expenseAccount,
            (double)lineAmount,
            0,
            $"Purchase - {itemName ?? "Manual Item"} - Invoice {invoiceCode}");
    }

    /// <summary>
    /// Add tax entry
    /// </summary>
    public static void AddTaxEntry(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel taxAccount,
        decimal taxAmount,
        bool isSale,
        string invoiceCode)
    {
        if (taxAmount > 0)
        {
            AddGLEntry(
                glHeader,
                taxAccount,
                isSale ? 0 : (double)taxAmount,
                isSale ? (double)taxAmount : 0,
                $"Tax - Invoice {invoiceCode ?? "NEW"}");
        }
    }

    /// <summary>
    /// Add tax entry - overload for tuple type
    /// </summary>
    public static void AddTaxEntry(
        GeneralLedgerHeaderModel glHeader,
        (int Id, string Code, string Name, string Type) taxAccount,
        decimal taxAmount,
        bool isSale,
        string invoiceCode)
    {
        if (taxAmount > 0)
        {
            AddGLEntry(
                glHeader,
                taxAccount,
                isSale ? 0 : (double)taxAmount,
                isSale ? (double)taxAmount : 0,
                $"Tax - Invoice {invoiceCode}");
        }
    }

    /// <summary>
    /// Add service charges entry
    /// </summary>
    public static void AddServiceChargesEntry(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel serviceAccount,
        decimal chargesAmount,
        bool isSale,
        string invoiceCode)
    {
        if (chargesAmount > 0)
        {
            AddGLEntry(
                glHeader,
                serviceAccount,
                isSale ? 0 : (double)chargesAmount,
                isSale ? (double)chargesAmount : 0,
                $"Service Charges - Invoice {invoiceCode ?? "NEW"}");
        }
    }

    /// <summary>
    /// Add service charges entry - overload for tuple type
    /// </summary>
    public static void AddServiceChargesEntry(
        GeneralLedgerHeaderModel glHeader,
        (int Id, string Code, string Name, string Type) serviceAccount,
        decimal chargesAmount,
        bool isSale,
        string invoiceCode)
    {
        if (chargesAmount > 0)
        {
            AddGLEntry(
                glHeader,
                serviceAccount,
                isSale ? 0 : (double)chargesAmount,
                isSale ? (double)chargesAmount : 0,
                $"Service Charges - Invoice {invoiceCode}");
        }
    }

    /// <summary>
    /// Add discount entry
    /// </summary>
    public static void AddDiscountEntry(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel? discountAccount,
        decimal discountAmount,
        bool isSale,
        string invoiceCode)
    {
        if (discountAmount > 0 && discountAccount != null)
        {
            AddGLEntry(
                glHeader,
                discountAccount,
                isSale ? (double)discountAmount : 0,
                isSale ? 0 : (double)discountAmount,
                $"Discount - Invoice {invoiceCode ?? "NEW"}");
        }
    }

    /// <summary>
    /// Add discount entry - overload for tuple type
    /// </summary>
    public static void AddDiscountEntry(
        GeneralLedgerHeaderModel glHeader,
        (int Id, string Code, string Name, string Type)? discountAccount,
        decimal discountAmount,
        bool isSale,
        string invoiceCode)
    {
        if (discountAmount > 0 && discountAccount.HasValue)
        {
            AddGLEntry(
                glHeader,
                discountAccount.Value,
                isSale ? (double)discountAmount : 0,
                isSale ? 0 : (double)discountAmount,
                $"Discount - Invoice {invoiceCode}");
        }
    }

    /// <summary>
    /// Add payment entries (for SALE: Debit Cash/Bank, Credit AR | for PURCHASE: Debit AP, Credit Cash/Bank)
    /// </summary>
    public static void AddPaymentEntries(
        GeneralLedgerHeaderModel glHeader,
        ChartOfAccountsModel arApAccount,
        ChartOfAccountsModel paymentAccount,
        double paymentAmount,
        bool isSale,
        string invoiceCode,
        string? paymentRefs = null,
        int? partyId = null)
    {
        string paymentDescription = $"Payment - Invoice {invoiceCode ?? "NEW"}";
        if (!string.IsNullOrWhiteSpace(paymentRefs))
        {
            paymentDescription += $" ({paymentRefs})";
        }

        if (isSale)
        {
            // Debit Cash/Bank
            AddGLEntry(
                glHeader,
                paymentAccount,
                paymentAmount,
                0,
                paymentDescription,
                partyId);

            // Credit AR
            string arDescription = $"Payment Received - Invoice {invoiceCode ?? "NEW"}";
            if (!string.IsNullOrWhiteSpace(paymentRefs))
            {
                arDescription += $" ({paymentRefs})";
            }
            AddGLEntry(
                glHeader,
                arApAccount,
                0,
                paymentAmount,
                arDescription,
                partyId);
        }
        else
        {
            // Debit AP
            string apDescription = $"Payment Made - Invoice {invoiceCode ?? "NEW"}";
            if (!string.IsNullOrWhiteSpace(paymentRefs))
            {
                apDescription += $" ({paymentRefs})";
            }
            AddGLEntry(
                glHeader,
                arApAccount,
                paymentAmount,
                0,
                apDescription,
                partyId);

            // Credit Cash/Bank
            AddGLEntry(
                glHeader,
                paymentAccount,
                0,
                paymentAmount,
                paymentDescription,
                partyId);
        }
    }

    #endregion
}
