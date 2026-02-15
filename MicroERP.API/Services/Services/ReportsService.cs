using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroERP.API.Helper;
using MicroERP.Shared.Models;
using Serilog;

namespace MicroERP.API.Services;

public interface IReportsService
{
    Task<List<DailyFundsClosingModel>> GetDailyFundsClosing(int organizationId, DateTime reportDate);
    Task<List<InventoryClosingModel>> GetInventoryClosing(int organizationId, DateTime reportDate);
    Task<List<CashByLocationUserModel>> GetCashByLocationUser(int organizationId, DateTime asOfDate);
    Task<List<InventoryByLocationModel>> GetInventoryByLocation(int organizationId, DateTime asOfDate);
    Task<List<TrialBalanceModel>> GetTrialBalance(int organizationId, DateTime startDate, DateTime endDate);
    Task<List<AccountAnalysisModel>> GetAccountAnalysis(int organizationId, DateTime startDate, DateTime endDate);
    Task<ProfitLossReportModel> GetProfitLoss(int organizationId, DateTime startDate, DateTime endDate);
    Task<BalanceSheetReportModel> GetBalanceSheet(int organizationId, DateTime asOfDate);
    Task<List<CashReconciliationReportModel>> GetCashReconciliation(int organizationId, DateTime reportDate, int? locationId = null, int? countedBy = null, DateTime? countedOn = null);
    Task<List<PhysicalCashCountSessionModel>> GetPhysicalCashCountSessions(int organizationId, DateTime reportDate, int? locationId = null);
    Task<List<EmployeeAdvanceReportModel>> GetEmployeeAdvances(int organizationId, int? locationId = null, int? employeeId = null);
    Task<DashboardModel> GetDashboardData(int organizationId);
    Task<List<CashMovementModel>> GetCashMovement(int organizationId, DateTime startDate, DateTime endDate, int? locationId = null);
    Task<List<PartyReceivablePayableModel>> GetPartyReceivablePayable(int organizationId, string? partyType = null);
}

/// <summary>Raw row from trial balance SQL query for Dapper mapping.</summary>
internal class TrialBalanceRow
{
    public int AccountId { get; set; }
    public int ParentId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Nature { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal DrEntered { get; set; }
    public decimal CrEntered { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class ReportsService : IReportsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<List<DailyFundsClosingModel>> GetDailyFundsClosing(int organizationId, DateTime reportDate)
    {
        string dateStr = reportDate.ToString("yyyy-MM-dd");
        
        string SQL = $@"
            WITH CashTransactions AS (
                SELECT 
                    LocationId,
                    TranDate,
                    SUM(CASE WHEN TranType = 'RECEIPT' THEN Amount ELSE 0 END) AS Receipts,
                    SUM(CASE WHEN TranType = 'PAYMENT' THEN Amount ELSE 0 END) AS Payments
                FROM Cashbook
                WHERE IsSoftDeleted = 0 AND OrganizationId = {organizationId}
                GROUP BY LocationId, TranDate
            )
            SELECT 
                l.Id AS LocationId,
                l.Name AS LocationName,
                ISNULL((
                    SELECT SUM(Receipts - Payments)
                    FROM CashTransactions ct
                    WHERE ct.LocationId = l.Id AND ct.TranDate < '{dateStr}'
                ), 0) AS OpeningBalance,
                ISNULL((
                    SELECT SUM(Receipts)
                    FROM CashTransactions ct
                    WHERE ct.LocationId = l.Id AND ct.TranDate = '{dateStr}'
                ), 0) AS Receipts,
                ISNULL((
                    SELECT SUM(Payments)
                    FROM CashTransactions ct
                    WHERE ct.LocationId = l.Id AND ct.TranDate = '{dateStr}'
                ), 0) AS Payments,
                ISNULL((
                    SELECT SUM(Receipts - Payments)
                    FROM CashTransactions ct
                    WHERE ct.LocationId = l.Id AND ct.TranDate <= '{dateStr}'
                ), 0) AS ClosingBalance
            FROM Locations l
            WHERE l.IsActive = 1 AND l.IsSoftDeleted = 0 AND l.OrganizationId = {organizationId}";

        var result = await dapper.SearchByQuery<DailyFundsClosingModel>(SQL);
        return result ?? new List<DailyFundsClosingModel>();
    }

    public async Task<List<InventoryClosingModel>> GetInventoryClosing(int organizationId, DateTime reportDate)
    {
        string dateStr = reportDate.ToString("yyyy-MM-dd");
        
        string SQL = $@"
            WITH InventoryTransactions AS (
                SELECT 
                    i.LocationId,
                    id.ItemId,
                    SUM(CASE WHEN i.InvoiceType = 'PURCHASE' AND i.TranDate < '{dateStr}' THEN id.Qty ELSE 0 END) - 
                    SUM(CASE WHEN i.InvoiceType = 'SALE' AND i.TranDate < '{dateStr}' THEN id.Qty ELSE 0 END) AS OpeningQty,
                    SUM(CASE WHEN i.InvoiceType = 'PURCHASE' AND i.TranDate = '{dateStr}' THEN id.Qty ELSE 0 END) AS ReceivedQty,
                    SUM(CASE WHEN i.InvoiceType = 'SALE' AND i.TranDate = '{dateStr}' THEN id.Qty ELSE 0 END) AS IssuedQty,
                    AVG(CASE WHEN i.InvoiceType = 'PURCHASE' THEN id.UnitPrice ELSE NULL END) AS AvgUnitPrice
                FROM Invoice i
                INNER JOIN InvoiceDetail id ON i.Id = id.InvoiceId
                WHERE i.IsSoftDeleted = 0 AND id.IsSoftDeleted = 0 
                    AND i.OrganizationId = {organizationId}
                    AND i.TranDate <= '{dateStr}'
                GROUP BY i.LocationId, id.ItemId
            )
            SELECT 
                itm.Code AS ItemCode,
                itm.Name AS ItemName,
                l.Name AS LocationName,
                ISNULL(SUM(inv.OpeningQty), 0) AS OpeningQty,
                ISNULL(SUM(inv.ReceivedQty), 0) AS ReceivedQty,
                ISNULL(SUM(inv.IssuedQty), 0) AS IssuedQty,
                ISNULL(SUM(inv.OpeningQty + inv.ReceivedQty - inv.IssuedQty), 0) AS ClosingQty,
                ISNULL(SUM((inv.OpeningQty + inv.ReceivedQty - inv.IssuedQty) * ISNULL(inv.AvgUnitPrice, 0)), 0) AS ClosingValue
            FROM InventoryTransactions inv
            INNER JOIN Items itm ON itm.Id = inv.ItemId
            INNER JOIN Locations l ON l.Id = inv.LocationId
            GROUP BY itm.Code, itm.Name, l.Name
            HAVING SUM(inv.OpeningQty + inv.ReceivedQty - inv.IssuedQty) <> 0";

        var result = await dapper.SearchByQuery<InventoryClosingModel>(SQL);
        return result ?? new List<InventoryClosingModel>();
    }

    public async Task<List<CashByLocationUserModel>> GetCashByLocationUser(int organizationId, DateTime asOfDate)
    {
        string dateStr = asOfDate.ToString("yyyy-MM-dd");
        
        string SQL = $@"
            WITH CashBalances AS (
                SELECT 
                    cb.LocationId,
                    cb.CreatedBy AS UserId,
                    SUM(CASE WHEN cb.TranType = 'RECEIPT' THEN cb.Amount ELSE -cb.Amount END) AS CashAmount,
                    MAX(cb.TranDate) AS LastTransactionDate
                FROM Cashbook cb
                WHERE cb.IsSoftDeleted = 0 AND cb.OrganizationId = {organizationId}
                    AND cb.TranDate <= '{dateStr}'
                GROUP BY cb.LocationId, cb.CreatedBy
            )
            SELECT 
                l.Id AS LocationId,
                l.Name AS LocationName,
                u.Id AS UserId,
                u.Username AS UserName,
                ISNULL(e.Fullname, u.Username) AS UserFullName,
                SUM(cb.CashAmount) AS CashAmount,
                MAX(cb.LastTransactionDate) AS LastTransactionDate
            FROM CashBalances cb
            INNER JOIN Locations l ON l.Id = cb.LocationId
            INNER JOIN Users u ON u.Id = cb.UserId
            LEFT JOIN Employees e ON e.Id = u.EmpId
            WHERE l.IsActive = 1 AND l.IsSoftDeleted = 0
            GROUP BY l.Id, l.Name, u.Id, u.Username, e.Fullname
            HAVING SUM(cb.CashAmount) <> 0";

        var result = await dapper.SearchByQuery<CashByLocationUserModel>(SQL);
        return result ?? new List<CashByLocationUserModel>();
    }

    public async Task<List<InventoryByLocationModel>> GetInventoryByLocation(int organizationId, DateTime asOfDate)
    {
        string dateStr = asOfDate.ToString("yyyy-MM-dd");
        
        string SQL = $@"
            SELECT 
                l.Name AS LocationName,
                itm.Code AS ItemCode,
                itm.Name AS ItemName,
                ISNULL(vi.Quantity, 0) AS Quantity,
                ISNULL(vi.AverageCost, 0) AS UnitPrice,
                ISNULL(vi.Quantity * vi.AverageCost, 0) AS TotalValue
            FROM vw_Inventory vi
            INNER JOIN Locations l ON l.Id = vi.LocationId
            INNER JOIN Items itm ON itm.Id = vi.ItemId
            WHERE vi.OrganizationId = {organizationId}
                AND l.IsActive = 1 AND l.IsSoftDeleted = 0
            ORDER BY l.Name, itm.Code";

        var result = await dapper.SearchByQuery<InventoryByLocationModel>(SQL);
        return result ?? new List<InventoryByLocationModel>();
    }

    public async Task<List<TrialBalanceModel>> GetTrialBalance(int organizationId, DateTime startDate, DateTime endDate)
    {
        string startStr = startDate.ToString("yyyy-MM-dd");
        string endStr = endDate.ToString("yyyy-MM-dd");

        // Amounts in base currency: convert using header exchange rate when entered <> base (NULL-safe for older DBs)
        string SQL = $@"
            SELECT 
                coa.Id AS AccountId,
                ISNULL(coa.ParentId, 0) AS ParentId,
                coa.Code AS AccountCode,
                coa.Name AS AccountName,
                coa.Type AS Nature,
                MAX(ISNULL(bc.Code, '')) AS CurrencyCode,
                SUM(gld.DebitAmount) AS DrEntered,
                SUM(gld.CreditAmount) AS CrEntered,
                SUM(CASE WHEN (glh.EnteredCurrencyId = glh.BaseCurrencyId OR glh.EnteredCurrencyId IS NULL OR glh.BaseCurrencyId IS NULL OR ISNULL(glh.ExchangeRate, 0) = 0) THEN gld.DebitAmount ELSE gld.DebitAmount * ISNULL(glh.ExchangeRate, 1) END) AS Debit,
                SUM(CASE WHEN (glh.EnteredCurrencyId = glh.BaseCurrencyId OR glh.EnteredCurrencyId IS NULL OR glh.BaseCurrencyId IS NULL OR ISNULL(glh.ExchangeRate, 0) = 0) THEN gld.CreditAmount ELSE gld.CreditAmount * ISNULL(glh.ExchangeRate, 1) END) AS Credit
            FROM GeneralLedgerDetail gld
            INNER JOIN GeneralLedgerHeader glh ON gld.HeaderId = glh.Id
            INNER JOIN ChartOfAccounts coa ON gld.AccountId = coa.Id
            LEFT JOIN Currencies bc ON glh.BaseCurrencyId = bc.Id
            WHERE glh.OrganizationId = {organizationId}
                AND glh.EntryDate >= '{startStr}' AND glh.EntryDate <= '{endStr}'
                AND glh.IsPosted = 1
                AND gld.IsSoftDeleted = 0
                AND glh.IsSoftDeleted = 0
                AND coa.IsSoftDeleted = 0
            GROUP BY coa.Id, coa.ParentId, coa.Code, coa.Name, coa.Type
            HAVING SUM(gld.DebitAmount) <> 0 OR SUM(gld.CreditAmount) <> 0";

        var raw = await dapper.SearchByQuery<TrialBalanceRow>(SQL);
        if (raw == null || !raw.Any())
            return new List<TrialBalanceModel>();

        var list = new List<TrialBalanceModel>();
        var parentMap = raw.ToDictionary(r => r.AccountId, r => r.ParentId);
        int GetLevel(int id)
        {
            int level = 0;
            while (parentMap.TryGetValue(id, out int pid) && pid != 0) { level++; id = pid; }
            return level;
        }

        foreach (var row in raw)
        {
            list.Add(new TrialBalanceModel
            {
                AccountId = row.AccountId,
                ParentId = row.ParentId,
                Level = GetLevel(row.AccountId),
                AccountCode = row.AccountCode,
                AccountName = row.AccountName,
                Nature = row.Nature,
                CurrencyCode = row.CurrencyCode ?? "",
                ExchangeRate = 1m,
                DrEntered = row.DrEntered,
                CrEntered = row.CrEntered,
                NetEntered = row.DrEntered - row.CrEntered,
                Debit = row.Debit,
                Credit = row.Credit,
                Net = row.Debit - row.Credit
            });
        }

        list = list.OrderBy(x => x.AccountCode).ToList();
        return list;
    }

    public async Task<List<AccountAnalysisModel>> GetAccountAnalysis(int organizationId, DateTime startDate, DateTime endDate)
    {
        string startStr = startDate.ToString("yyyy-MM-dd");
        string endStr = endDate.ToString("yyyy-MM-dd");

        string SQL = $@"
            SELECT 
                glh.EntryDate AS TransactionDate,
                coa.Code AS AccountCode,
                coa.Name AS AccountName,
                glh.ReferenceNo AS ReferenceNo,
                ISNULL(ec.Code, '') AS CurrencyCode,
                ISNULL(glh.ExchangeRate, 1) AS ExchangeRate,
                gld.DebitAmount AS DrEntered,
                gld.CreditAmount AS CrEntered,
                (gld.DebitAmount - gld.CreditAmount) AS NetEntered,
                CASE WHEN (glh.EnteredCurrencyId = glh.BaseCurrencyId OR glh.EnteredCurrencyId IS NULL OR glh.BaseCurrencyId IS NULL OR ISNULL(glh.ExchangeRate, 0) = 0) THEN gld.DebitAmount ELSE gld.DebitAmount * ISNULL(glh.ExchangeRate, 1) END AS Debit,
                CASE WHEN (glh.EnteredCurrencyId = glh.BaseCurrencyId OR glh.EnteredCurrencyId IS NULL OR glh.BaseCurrencyId IS NULL OR ISNULL(glh.ExchangeRate, 0) = 0) THEN gld.CreditAmount ELSE gld.CreditAmount * ISNULL(glh.ExchangeRate, 1) END AS Credit,
                (CASE WHEN (glh.EnteredCurrencyId = glh.BaseCurrencyId OR glh.EnteredCurrencyId IS NULL OR glh.BaseCurrencyId IS NULL OR ISNULL(glh.ExchangeRate, 0) = 0) THEN gld.DebitAmount ELSE gld.DebitAmount * ISNULL(glh.ExchangeRate, 1) END -
                 CASE WHEN (glh.EnteredCurrencyId = glh.BaseCurrencyId OR glh.EnteredCurrencyId IS NULL OR glh.BaseCurrencyId IS NULL OR ISNULL(glh.ExchangeRate, 0) = 0) THEN gld.CreditAmount ELSE gld.CreditAmount * ISNULL(glh.ExchangeRate, 1) END) AS Net
            FROM GeneralLedgerDetail gld
            INNER JOIN GeneralLedgerHeader glh ON gld.HeaderId = glh.Id
            INNER JOIN ChartOfAccounts coa ON gld.AccountId = coa.Id
            LEFT JOIN Currencies ec ON glh.EnteredCurrencyId = ec.Id
            WHERE glh.OrganizationId = {organizationId}
                AND glh.EntryDate >= '{startStr}' AND glh.EntryDate <= '{endStr}'
                AND glh.IsPosted = 1
                AND gld.IsSoftDeleted = 0
                AND glh.IsSoftDeleted = 0
                AND coa.IsSoftDeleted = 0
            ORDER BY glh.EntryDate, coa.Code, gld.Id";

        var rows = await dapper.SearchByQuery<AccountAnalysisModel>(SQL);
        return rows?.ToList() ?? new List<AccountAnalysisModel>();
    }

    public async Task<ProfitLossReportModel> GetProfitLoss(int organizationId, DateTime startDate, DateTime endDate)
    {
        string startStr = startDate.ToString("yyyy-MM-dd");
        string endStr = endDate.ToString("yyyy-MM-dd");
        
        string revenueSQL = $@"
            SELECT 
                coa.Name AS AccountName,
                SUM(glr.CreditAmount - glr.DebitAmount) AS Amount
            FROM vw_GeneralLedgerReport glr
            INNER JOIN ChartOfAccounts coa ON coa.Id = glr.AccountId
            WHERE glr.OrganizationId = {organizationId}
                AND glr.EntryDate >= '{startStr}' AND glr.EntryDate <= '{endStr}'
                AND coa.Type = 'REVENUE'
                AND glr.IsPosted = 1
            GROUP BY coa.Name
            HAVING SUM(glr.CreditAmount - glr.DebitAmount) <> 0";

        string expenseSQL = $@"
            SELECT 
                coa.Name AS AccountName,
                SUM(glr.DebitAmount - glr.CreditAmount) AS Amount
            FROM vw_GeneralLedgerReport glr
            INNER JOIN ChartOfAccounts coa ON coa.Id = glr.AccountId
            WHERE glr.OrganizationId = {organizationId}
                AND glr.EntryDate >= '{startStr}' AND glr.EntryDate <= '{endStr}'
                AND coa.Type = 'EXPENSE'
                AND glr.IsPosted = 1
            GROUP BY coa.Name
            HAVING SUM(glr.DebitAmount - glr.CreditAmount) <> 0";

        var revenue = await dapper.SearchByQuery<ProfitLossItemModel>(revenueSQL);
        var expenses = await dapper.SearchByQuery<ProfitLossItemModel>(expenseSQL);

        return new ProfitLossReportModel
        {
            Revenue = revenue ?? new List<ProfitLossItemModel>(),
            Expenses = expenses ?? new List<ProfitLossItemModel>()
        };
    }

    public async Task<BalanceSheetReportModel> GetBalanceSheet(int organizationId, DateTime asOfDate)
    {
        string dateStr = asOfDate.ToString("yyyy-MM-dd");
        
        string assetSQL = $@"
            SELECT 
                coa.Name AS AccountName,
                SUM(glr.DebitAmount - glr.CreditAmount) AS Amount
            FROM vw_GeneralLedgerReport glr
            INNER JOIN ChartOfAccounts coa ON coa.Id = glr.AccountId
            WHERE glr.OrganizationId = {organizationId}
                AND glr.EntryDate <= '{dateStr}'
                AND coa.Type = 'ASSET'
                AND glr.IsPosted = 1
            GROUP BY coa.Name
            HAVING SUM(glr.DebitAmount - glr.CreditAmount) <> 0";

        string liabilitySQL = $@"
            SELECT 
                coa.Name AS AccountName,
                SUM(glr.CreditAmount - glr.DebitAmount) AS Amount
            FROM vw_GeneralLedgerReport glr
            INNER JOIN ChartOfAccounts coa ON coa.Id = glr.AccountId
            WHERE glr.OrganizationId = {organizationId}
                AND glr.EntryDate <= '{dateStr}'
                AND coa.Type = 'LIABILITY'
                AND glr.IsPosted = 1
            GROUP BY coa.Name
            HAVING SUM(glr.CreditAmount - glr.DebitAmount) <> 0";

        string equitySQL = $@"
            SELECT 
                coa.Name AS AccountName,
                SUM(glr.CreditAmount - glr.DebitAmount) AS Amount
            FROM vw_GeneralLedgerReport glr
            INNER JOIN ChartOfAccounts coa ON coa.Id = glr.AccountId
            WHERE glr.OrganizationId = {organizationId}
                AND glr.EntryDate <= '{dateStr}'
                AND coa.Type = 'EQUITY'
                AND glr.IsPosted = 1
            GROUP BY coa.Name
            HAVING SUM(glr.CreditAmount - glr.DebitAmount) <> 0";

        var assets = await dapper.SearchByQuery<BalanceSheetItemModel>(assetSQL);
        var liabilities = await dapper.SearchByQuery<BalanceSheetItemModel>(liabilitySQL);
        var equity = await dapper.SearchByQuery<BalanceSheetItemModel>(equitySQL);

        return new BalanceSheetReportModel
        {
            Assets = assets ?? new List<BalanceSheetItemModel>(),
            Liabilities = liabilities ?? new List<BalanceSheetItemModel>(),
            Equity = equity ?? new List<BalanceSheetItemModel>()
        };
    }

    public async Task<List<PhysicalCashCountSessionModel>> GetPhysicalCashCountSessions(int organizationId, DateTime reportDate, int? locationId = null)
    {
        string dateStr = reportDate.ToString("yyyy-MM-dd");
        string locationFilter = locationId.HasValue ? $"AND pcc.LocationId = {locationId.Value}" : "";
        
        // Get all physical cash count sessions for the selected date and location
        // Group by SessionId to ensure all denomination records in a session are grouped together
        // This prevents issues when multiple people count at the same time or same person does multiple counts
        string SQL = $@"
            SELECT 
                CAST(pcc.CountDate AS DATE) AS CountDate,
                pcc.LocationId,
                ISNULL(l.Name, 'Unknown') AS LocationName,
                ISNULL(pcc.Locker, '') AS Locker,
                pcc.CountedBy,
                ISNULL(e.Fullname, 'Unknown') AS CountedByName,
                MIN(pcc.CreatedOn) AS CountedOn,
                SUM(pcc.Amount) AS TotalAmount,
                MIN(pcc.Status) AS Status,
                ISNULL(CONCAT(ISNULL(l.Name, 'Unknown'), ' - ', ISNULL(pcc.Locker, ''), ' - ', ISNULL(e.Fullname, 'Unknown'), ' - ', FORMAT(MIN(pcc.CreatedOn), 'dd MMM yyyy HH:mm')), 'Unknown Session') AS DisplayText
            FROM PhysicalCashCount pcc
            LEFT JOIN Locations l ON l.Id = pcc.LocationId
            LEFT JOIN Employees e ON e.Id = pcc.CountedBy
            WHERE pcc.IsSoftDeleted = 0 
                AND pcc.OrganizationId = {organizationId}
                AND CAST(pcc.CountDate AS DATE) = '{dateStr}'
                {locationFilter}
            GROUP BY pcc.SessionId, CAST(pcc.CountDate AS DATE), pcc.LocationId, l.Name, pcc.Locker, pcc.CountedBy, e.Fullname
            ORDER BY l.Name, pcc.Locker, e.Fullname, MIN(pcc.CreatedOn)";

        var result = await dapper.SearchByQuery<PhysicalCashCountSessionModel>(SQL);
        return result ?? new List<PhysicalCashCountSessionModel>();
    }

    public async Task<List<CashReconciliationReportModel>> GetCashReconciliation(int organizationId, DateTime reportDate, int? locationId = null, int? countedBy = null, DateTime? countedOn = null)
    {
        string dateStr = reportDate.ToString("yyyy-MM-dd");
        string locationFilter = locationId.HasValue ? $"AND l.Id = {locationId.Value}" : "";
        string countedByFilter = countedBy.HasValue && countedBy.Value > 0 ? $"AND pcc.CountedBy = {countedBy.Value}" : "";
        
        // Filter by CountedOn timestamp - use a time window to match the session (within 10 minutes)
        string countedOnFilter = "";
        if (countedOn.HasValue)
        {
            DateTime countOnStart = countedOn.Value.AddMinutes(-5);
            DateTime countOnEnd = countedOn.Value.AddMinutes(5);
            string countOnStartStr = countOnStart.ToString("yyyy-MM-dd HH:mm:ss");
            string countOnEndStr = countOnEnd.ToString("yyyy-MM-dd HH:mm:ss");
            countedOnFilter = $"AND pcc.CreatedOn >= '{countOnStartStr}' AND pcc.CreatedOn <= '{countOnEndStr}'";
        }
        
        // This query reconciles cash transactions against physical cash count
        // Expected Balance = Opening Balance + Cash Receipts - Cash Payments
        // Variance = Physical Count - Expected Balance
        // Cash Receipts = Sale Invoices (cash) + CashBook Receipts + General Ledger (cash account debits)
        // Cash Payments = Purchase Invoices (cash) + CashBook Payments + General Ledger (cash account credits)
        string SQL = $@"
            WITH CashAccounts AS (
                -- Identify cash accounts: InterfaceType='PAYMENT METHOD' and Name contains 'CASH'
                SELECT DISTINCT Id AS CashAccountId
                FROM ChartOfAccounts
                WHERE OrganizationId = {organizationId}
                    AND IsActive = 1
                    AND IsSoftDeleted = 0
                    AND InterfaceType = 'PAYMENT METHOD'
                    AND (UPPER(Name) LIKE '%CASH%' OR UPPER(Name) = 'CASH IN HAND')
            ),
            CashTransactions AS (
                -- 1. Cash transactions from CashBook where PaymentMethod is CASH or NULL (assume CASH if NULL)
                SELECT 
                    cb.LocationId,
                    CAST(cb.TranDate AS DATE) AS TranDate,
                    SUM(CASE WHEN cb.TranType = 'RECEIPT' AND (UPPER(ISNULL(cb.PaymentMethod, 'CASH')) = 'CASH' OR cb.PaymentMethod IS NULL) THEN cb.Amount ELSE 0 END) AS CashReceipts,
                    SUM(CASE WHEN cb.TranType = 'PAYMENT' AND (UPPER(ISNULL(cb.PaymentMethod, 'CASH')) = 'CASH' OR cb.PaymentMethod IS NULL) THEN cb.Amount ELSE 0 END) AS CashPayments
                FROM Cashbook cb
                WHERE cb.IsSoftDeleted = 0 AND cb.OrganizationId = {organizationId}
                GROUP BY cb.LocationId, CAST(cb.TranDate AS DATE)
                
                UNION ALL
                
                -- 2. Cash transactions from Sale Invoices (InvoicePayments where InvoiceType='SALE' and AccountId is cash account)
                SELECT 
                    i.LocationId,
                    CAST(i.TranDate AS DATE) AS TranDate,
                    SUM(ip.Amount) AS CashReceipts,
                    0 AS CashPayments
                FROM InvoicePayments ip
                INNER JOIN Invoice i ON i.Id = ip.InvoiceId
                INNER JOIN CashAccounts ca ON ca.CashAccountId = ip.AccountId
                WHERE ip.IsSoftDeleted = 0 
                    AND i.IsSoftDeleted = 0
                    AND i.OrganizationId = {organizationId}
                    AND UPPER(ISNULL(i.InvoiceType, '')) IN ('SALE', 'BILL', 'SALES')
                GROUP BY i.LocationId, CAST(i.TranDate AS DATE)
                
                UNION ALL
                
                -- 3. Cash transactions from Purchase Invoices (InvoicePayments where InvoiceType='PURCHASE' and AccountId is cash account)
                SELECT 
                    i.LocationId,
                    CAST(i.TranDate AS DATE) AS TranDate,
                    0 AS CashReceipts,
                    SUM(ip.Amount) AS CashPayments
                FROM InvoicePayments ip
                INNER JOIN Invoice i ON i.Id = ip.InvoiceId
                INNER JOIN CashAccounts ca ON ca.CashAccountId = ip.AccountId
                WHERE ip.IsSoftDeleted = 0 
                    AND i.IsSoftDeleted = 0
                    AND i.OrganizationId = {organizationId}
                    AND UPPER(ISNULL(i.InvoiceType, '')) IN ('PURCHASE', 'PURCHASE ORDER', 'PURCHASES')
                GROUP BY i.LocationId, CAST(i.TranDate AS DATE)
                
                UNION ALL
                
                -- 4. Cash transactions from General Ledger (cash account debits = Cash In, credits = Cash Out)
                SELECT 
                    ISNULL(gld.LocationId, glh.LocationId) AS LocationId,
                    CAST(glh.EntryDate AS DATE) AS TranDate,
                    SUM(gld.DebitAmount) AS CashReceipts,
                    SUM(gld.CreditAmount) AS CashPayments
                FROM GeneralLedgerDetail gld
                INNER JOIN GeneralLedgerHeader glh ON glh.Id = gld.HeaderId
                INNER JOIN CashAccounts ca ON ca.CashAccountId = gld.AccountId
                WHERE gld.IsSoftDeleted = 0 
                    AND glh.IsSoftDeleted = 0
                    AND glh.OrganizationId = {organizationId}
                    AND glh.IsPosted = 1
                    -- Exclude transactions already counted in CashBook or Invoices to avoid double counting
                    AND (glh.Source NOT IN ('CASHBOOK', 'INVOICE') OR glh.Source IS NULL)
                GROUP BY ISNULL(gld.LocationId, glh.LocationId), CAST(glh.EntryDate AS DATE)
            ),
            CashTransactionsAggregated AS (
                -- Aggregate all cash transactions by location and date
                SELECT 
                    LocationId,
                    TranDate,
                    SUM(CashReceipts) AS CashReceipts,
                    SUM(CashPayments) AS CashPayments
                FROM CashTransactions
                GROUP BY LocationId, TranDate
            ),
            PhysicalCounts AS (
                -- Physical cash counts grouped by date, location, locker, and user (CountedBy)
                -- If CountedOn filter is provided, it filters to specific session via WHERE clause
                -- The time window (5 minutes before/after) ensures we get the specific session
                SELECT 
                    pcc.CountDate,
                    pcc.LocationId,
                    pcc.Locker,
                    SUM(pcc.Amount) AS PhysicalCount,
                    MIN(pcc.Status) AS Status,
                    MIN(pcc.CountedBy) AS CountedBy,
                    MIN(pcc.CreatedOn) AS CountedOn
                FROM PhysicalCashCount pcc
                WHERE pcc.IsSoftDeleted = 0 AND pcc.OrganizationId = {organizationId}
                    AND CAST(pcc.CountDate AS DATE) = '{dateStr}'
                    {countedByFilter}
                    {countedOnFilter}
                GROUP BY pcc.CountDate, pcc.LocationId, pcc.Locker, pcc.CountedBy,
                         DATEADD(MINUTE, DATEDIFF(MINUTE, 0, pcc.CreatedOn), 0)
            ),
            CashBalances AS (
                -- Calculate opening balance and daily cash movements by location
                -- TranDate is already CAST to DATE in CashTransactionsAggregated CTE
                SELECT 
                    cta.LocationId,
                    ISNULL(SUM(CASE WHEN cta.TranDate < '{dateStr}' THEN cta.CashReceipts - cta.CashPayments ELSE 0 END), 0) AS OpeningBalance,
                    ISNULL(SUM(CASE WHEN cta.TranDate = '{dateStr}' THEN cta.CashReceipts ELSE 0 END), 0) AS CashReceipts,
                    ISNULL(SUM(CASE WHEN cta.TranDate = '{dateStr}' THEN cta.CashPayments ELSE 0 END), 0) AS CashPayments
                FROM CashTransactionsAggregated cta
                GROUP BY cta.LocationId
            ),
            LocationPhysicalTotals AS (
                -- Calculate total physical count per location (sum of all lockers)
                SELECT 
                    LocationId,
                    SUM(PhysicalCount) AS TotalPhysicalCount
                FROM PhysicalCounts
                GROUP BY LocationId
            )
            SELECT 
                pc.CountDate,
                pc.LocationId,
                l.Name AS LocationName,
                pc.Locker,
                ISNULL(cb.OpeningBalance, 0) AS OpeningBalance,
                ISNULL(cb.CashReceipts, 0) AS CashReceipts,
                ISNULL(cb.CashPayments, 0) AS CashPayments,
                -- Expected Balance is at location level (same for all lockers at a location)
                ISNULL(cb.OpeningBalance, 0) + ISNULL(cb.CashReceipts, 0) - ISNULL(cb.CashPayments, 0) AS ExpectedBalance,
                ISNULL(pc.PhysicalCount, 0) AS PhysicalCount,
                -- For individual lockers, variance is Physical Count - Expected Balance (location total)
                -- Note: If location has multiple lockers, see LOCATION TOTAL row for proper variance
                ISNULL(pc.PhysicalCount, 0) - (ISNULL(cb.OpeningBalance, 0) + ISNULL(cb.CashReceipts, 0) - ISNULL(cb.CashPayments, 0)) AS Variance,
                ISNULL(pc.Status, 'NOT RECONCILED') AS Status,
                u.Username AS CountedByName,
                pc.CountedOn,
                pc.CountedBy
            FROM PhysicalCounts pc
            LEFT JOIN Locations l ON l.Id = pc.LocationId
            LEFT JOIN CashBalances cb ON cb.LocationId = pc.LocationId
            LEFT JOIN Users u ON u.Id = pc.CountedBy
            WHERE l.IsActive = 1 AND l.IsSoftDeleted = 0 {locationFilter}
            
            UNION ALL
            
            -- Include locations with cash transactions but no physical count
            SELECT 
                CAST('{dateStr}' AS DATE) AS CountDate,
                cb.LocationId,
                l.Name AS LocationName,
                'NO PHYSICAL COUNT' AS Locker,
                cb.OpeningBalance,
                cb.CashReceipts,
                cb.CashPayments,
                cb.OpeningBalance + cb.CashReceipts - cb.CashPayments AS ExpectedBalance,
                0 AS PhysicalCount,
                -(cb.OpeningBalance + cb.CashReceipts - cb.CashPayments) AS Variance,
                'NOT RECONCILED' AS Status,
                NULL AS CountedByName,
                NULL AS CountedOn,
                0 AS CountedBy
            FROM CashBalances cb
            LEFT JOIN Locations l ON l.Id = cb.LocationId
            WHERE l.IsActive = 1 AND l.IsSoftDeleted = 0
                AND (cb.OpeningBalance + cb.CashReceipts - cb.CashPayments) <> 0
                AND cb.LocationId NOT IN (SELECT DISTINCT LocationId FROM PhysicalCounts)
                {locationFilter}
            
            UNION ALL
            
            -- Add location-level summary row showing total physical count vs expected balance
            -- This row is added for each location that has physical counts to show location-level reconciliation
            SELECT 
                CAST('{dateStr}' AS DATE) AS CountDate,
                lpt.LocationId,
                l.Name AS LocationName,
                'LOCATION TOTAL' AS Locker,
                ISNULL(cb.OpeningBalance, 0) AS OpeningBalance,
                ISNULL(cb.CashReceipts, 0) AS CashReceipts,
                ISNULL(cb.CashPayments, 0) AS CashPayments,
                ISNULL(cb.OpeningBalance, 0) + ISNULL(cb.CashReceipts, 0) - ISNULL(cb.CashPayments, 0) AS ExpectedBalance,
                ISNULL(lpt.TotalPhysicalCount, 0) AS PhysicalCount,
                -- Variance at location level: Sum of all physical counts - Expected Balance
                ISNULL(lpt.TotalPhysicalCount, 0) - (ISNULL(cb.OpeningBalance, 0) + ISNULL(cb.CashReceipts, 0) - ISNULL(cb.CashPayments, 0)) AS Variance,
                CASE 
                    WHEN ISNULL(lpt.TotalPhysicalCount, 0) - (ISNULL(cb.OpeningBalance, 0) + ISNULL(cb.CashReceipts, 0) - ISNULL(cb.CashPayments, 0)) = 0 
                    THEN 'RECONCILED' 
                    ELSE 'NOT RECONCILED' 
                END AS Status,
                NULL AS CountedByName,
                NULL AS CountedOn,
                0 AS CountedBy
            FROM LocationPhysicalTotals lpt
            LEFT JOIN Locations l ON l.Id = lpt.LocationId
            LEFT JOIN CashBalances cb ON cb.LocationId = lpt.LocationId
            WHERE l.IsActive = 1 AND l.IsSoftDeleted = 0 {locationFilter}
            
            ORDER BY LocationName, 
                     CASE WHEN Locker = 'LOCATION TOTAL' THEN 1 ELSE 2 END,  -- Show location total after individual lockers
                     CASE WHEN Locker = 'NO PHYSICAL COUNT' THEN 3 ELSE 0 END,  -- Show no physical count last
                     Locker, 
                     CountDate";

        var result = await dapper.SearchByQuery<CashReconciliationReportModel>(SQL);
        return result ?? new List<CashReconciliationReportModel>();
    }

    public async Task<List<EmployeeAdvanceReportModel>> GetEmployeeAdvances(int organizationId, int? locationId = null, int? employeeId = null)
    {
        // Get the Advance account ID using InterfaceType
        string advanceAccountSQL = $@"SELECT TOP 1 Id FROM ChartOfAccounts 
            WHERE OrganizationId = {organizationId} 
            AND InterfaceType = 'ADVANCE' 
            AND IsActive = 1 
            AND IsSoftDeleted = 0";
        
        var advanceAccountResult = await dapper.SearchByQuery<dynamic>(advanceAccountSQL);
        if (advanceAccountResult == null || !advanceAccountResult.Any())
        {
            return new List<EmployeeAdvanceReportModel>();
        }
        
        int advanceAccountId = (int)advanceAccountResult.First().Id;
        
        string locationFilter = locationId.HasValue ? $"AND cb.LocationId = {locationId.Value}" : "";
        string employeeFilter = employeeId.HasValue ? $"AND cb.PartyId = {employeeId.Value}" : "";
        
        // Query to get employee advances from Cashbook
        // Advances Given = PAYMENT transactions (money going out)
        // Advances Recovered = RECEIPT transactions (money coming back)
        // Outstanding Balance = Advances Given - Advances Recovered
        // Note: Employees may be stored as Parties with PartyType='EMPLOYEE' or linked via PartyId
        string SQL = $@"
            SELECT 
                cb.PartyId,
                ISNULL(p.Code, '') AS PartyCode,
                ISNULL(p.Name, 'Unknown') AS PartyName,
                ISNULL(e.Id, 0) AS EmployeeId,
                ISNULL(e.EmpId, '') AS EmployeeCode,
                ISNULL(e.Fullname, ISNULL(p.Name, 'Unknown')) AS EmployeeName,
                ISNULL(d.Name, '') AS DepartmentName,
                ISNULL(SUM(CASE WHEN UPPER(cb.TranType) = 'PAYMENT' THEN cb.Amount ELSE 0 END), 0) AS TotalAdvanceGiven,
                ISNULL(SUM(CASE WHEN UPPER(cb.TranType) = 'RECEIPT' THEN cb.Amount ELSE 0 END), 0) AS TotalAdvanceRecovered,
                ISNULL(SUM(CASE WHEN UPPER(cb.TranType) = 'PAYMENT' THEN cb.Amount ELSE 0 END), 0) - 
                ISNULL(SUM(CASE WHEN UPPER(cb.TranType) = 'RECEIPT' THEN cb.Amount ELSE 0 END), 0) AS OutstandingBalance,
                COUNT(*) AS TransactionCount,
                MAX(cb.TranDate) AS LastTransactionDate
            FROM Cashbook cb
            LEFT JOIN Parties p ON p.Id = cb.PartyId
            LEFT JOIN Employees e ON (e.Id = cb.PartyId OR (p.PartyType = 'EMPLOYEE' AND e.Id = cb.PartyId))
            LEFT JOIN Departments d ON d.Id = e.DepartmentId
            WHERE cb.OrganizationId = {organizationId}
                AND cb.AccountId = {advanceAccountId}
                AND cb.IsSoftDeleted = 0
                {locationFilter}
                {employeeFilter}
            GROUP BY cb.PartyId, p.Code, p.Name, e.Id, e.EmpId, e.Fullname, d.Name
            HAVING ISNULL(SUM(CASE WHEN UPPER(cb.TranType) = 'PAYMENT' THEN cb.Amount ELSE 0 END), 0) - 
                   ISNULL(SUM(CASE WHEN UPPER(cb.TranType) = 'RECEIPT' THEN cb.Amount ELSE 0 END), 0) <> 0
            ORDER BY OutstandingBalance DESC, p.Name";

        var result = await dapper.SearchByQuery<EmployeeAdvanceReportModel>(SQL);
        return result ?? new List<EmployeeAdvanceReportModel>();
    }

    public async Task<DashboardModel> GetDashboardData(int organizationId)
    {
        var dashboard = new DashboardModel();
        DateTime today = DateTime.Today;
        DateTime sixMonthsAgo = today.AddMonths(-6);

        try
        {
            // 1. Total Inventory Value
            string inventorySQL = $@"
                SELECT ISNULL(SUM(vi.Quantity * vi.AverageCost), 0) AS TotalValue
                FROM vw_Inventory vi
                WHERE vi.OrganizationId = {organizationId}";
            var inventoryResult = await dapper.SearchByQuery<dynamic>(inventorySQL);
            dashboard.TotalInventoryValue = inventoryResult?.FirstOrDefault()?.TotalValue ?? 0;

            // 2. Funds Available (Cash balance from CashBook)
            string fundsSQL = $@"
                SELECT ISNULL(SUM(CASE WHEN UPPER(TranType) = 'RECEIPT' THEN Amount ELSE -Amount END), 0) AS Balance
                FROM Cashbook
                WHERE OrganizationId = {organizationId} AND IsSoftDeleted = 0";
            var fundsResult = await dapper.SearchByQuery<dynamic>(fundsSQL);
            dashboard.FundsAvailable = fundsResult?.FirstOrDefault()?.Balance ?? 0;

            // 3. Outstanding Payments (Purchase Invoices with outstanding balance)
            // Invoice table does not store TotalAmount/PaidAmount; compute from child tables
            string outstandingSQL = $@"
                SELECT ISNULL(SUM(TotalAmount - PaidAmount), 0) AS Outstanding
                FROM (
                    SELECT i.Id,
                        (ISNULL((SELECT SUM((id.Qty * id.UnitPrice) - id.DiscountAmount) FROM InvoiceDetail id WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0), 0)
                         + ISNULL((SELECT SUM(AppliedAmount) FROM InvoiceCharges WHERE InvoiceId = i.Id AND ChargeCategory = 'SERVICE' AND IsSoftDeleted = 0), 0)
                         - ISNULL((SELECT SUM(AppliedAmount) FROM InvoiceCharges WHERE InvoiceId = i.Id AND ChargeCategory = 'DISCOUNT' AND IsSoftDeleted = 0), 0)
                         + ISNULL((SELECT SUM(idt.TaxAmount) FROM InvoiceDetailTax idt INNER JOIN InvoiceDetail id ON idt.InvoiceDetailId = id.Id WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0), 0)) AS TotalAmount,
                        ISNULL((SELECT SUM(Amount) FROM InvoicePayments WHERE InvoiceId = i.Id AND IsSoftDeleted = 0), 0) AS PaidAmount
                    FROM Invoice i
                    WHERE i.OrganizationId = {organizationId} AND i.InvoiceType = 'PURCHASE' AND i.IsSoftDeleted = 0
                ) x
                WHERE (TotalAmount - PaidAmount) > 0";
            var outstandingResult = await dapper.SearchByQuery<dynamic>(outstandingSQL);
            dashboard.OutstandingPayments = outstandingResult?.FirstOrDefault()?.Outstanding ?? 0;

            // 4. Stock Alerts (Items below reorder level; Items table has ReorderQty)
            string stockAlertsSQL = $@"
                SELECT COUNT(DISTINCT vi.ItemId) AS AlertCount
                FROM vw_Inventory vi
                INNER JOIN Items i ON i.Id = vi.ItemId
                WHERE vi.OrganizationId = {organizationId}
                    AND vi.Quantity <= ISNULL(i.ReorderQty, 0)
                    AND ISNULL(i.ReorderQty, 0) > 0";
            var alertsResult = await dapper.SearchByQuery<dynamic>(stockAlertsSQL);
            dashboard.StockAlertsCount = (int)(alertsResult?.FirstOrDefault()?.AlertCount ?? 0);

            // 5. Inventory Distribution by Category
            string categorySQL = $@"
                SELECT 
                    ISNULL(c.Name, 'Uncategorized') AS CategoryName,
                    ISNULL(SUM(vi.Quantity * vi.AverageCost), 0) AS TotalValue
                FROM vw_Inventory vi
                INNER JOIN Items i ON i.Id = vi.ItemId
                LEFT JOIN Categories c ON c.Id = i.CategoryId
                WHERE vi.OrganizationId = {organizationId}
                GROUP BY ISNULL(c.Name, 'Uncategorized')
                HAVING SUM(vi.Quantity * vi.AverageCost) > 0
                ORDER BY TotalValue DESC";
            var categoryResult = await dapper.SearchByQuery<InventoryCategoryModel>(categorySQL);
            dashboard.InventoryByCategory = categoryResult ?? new List<InventoryCategoryModel>();

            // 6. Monthly Funds Flow (last 6 months)
            string monthlyFundsSQL = $@"
                SELECT 
                    CONCAT(DATENAME(MONTH, TranDate), ' ', YEAR(TranDate)) AS Month,
                    ISNULL(SUM(CASE WHEN UPPER(TranType) = 'RECEIPT' THEN Amount ELSE 0 END), 0) AS Receipts,
                    ISNULL(SUM(CASE WHEN UPPER(TranType) = 'PAYMENT' THEN Amount ELSE 0 END), 0) AS Payments,
                    ISNULL(SUM(CASE WHEN UPPER(TranType) = 'RECEIPT' THEN Amount ELSE -Amount END), 0) AS NetFlow
                FROM Cashbook
                WHERE OrganizationId = {organizationId}
                    AND TranDate >= '{sixMonthsAgo:yyyy-MM-dd}'
                    AND IsSoftDeleted = 0
                GROUP BY YEAR(TranDate), MONTH(TranDate), DATENAME(MONTH, TranDate)
                ORDER BY YEAR(TranDate), MONTH(TranDate)";
            var monthlyFundsResult = await dapper.SearchByQuery<MonthlyFundsModel>(monthlyFundsSQL);
            dashboard.MonthlyFundsFlow = monthlyFundsResult ?? new List<MonthlyFundsModel>();

            // 7. Low Stock Items (Items table has ReorderQty)
            string lowStockSQL = $@"
                SELECT TOP 10
                    i.Name AS ItemName,
                    ISNULL(SUM(vi.Quantity), 0) AS Quantity,
                    ISNULL(i.ReorderQty, 0) AS ReorderLevel
                FROM vw_Inventory vi
                INNER JOIN Items i ON i.Id = vi.ItemId
                WHERE vi.OrganizationId = {organizationId}
                    AND ISNULL(i.ReorderQty, 0) > 0
                GROUP BY i.Id, i.Name, i.ReorderQty
                HAVING SUM(vi.Quantity) <= ISNULL(i.ReorderQty, 0)
                ORDER BY (SUM(vi.Quantity) - ISNULL(i.ReorderQty, 0)) ASC";
            var lowStockResult = await dapper.SearchByQuery<LowStockItemModel>(lowStockSQL);
            dashboard.LowStockItems = lowStockResult ?? new List<LowStockItemModel>();

            // 8. Recent Transactions (from CashBook, PettyCash, Invoices)
            string recentTransactionsSQL = $@"
                SELECT TOP 10
                    TranDate AS Date,
                    Description,
                    Amount,
                    'CashBook' AS TransactionType
                FROM Cashbook
                WHERE OrganizationId = {organizationId} AND IsSoftDeleted = 0
                
                UNION ALL
                
                SELECT TOP 10
                    TranDate AS Date,
                    Description,
                    CASE WHEN TranType = 'RECEIPT' THEN Amount ELSE -Amount END AS Amount,
                    'PettyCash' AS TransactionType
                FROM PettyCash
                WHERE OrganizationId = {organizationId} AND IsSoftDeleted = 0
                
                UNION ALL
                
                SELECT TOP 10
                    i.TranDate AS Date,
                    CONCAT(i.InvoiceType, ' - ', i.Code) AS Description,
                    CASE WHEN i.InvoiceType = 'SALE' THEN invTotal.TotalAmount ELSE -invTotal.TotalAmount END AS Amount,
                    'Invoice' AS TransactionType
                FROM Invoice i
                CROSS APPLY (
                    SELECT ISNULL(SUM((id.Qty * id.UnitPrice) - id.DiscountAmount), 0)
                        + ISNULL((SELECT SUM(AppliedAmount) FROM InvoiceCharges WHERE InvoiceId = i.Id AND ChargeCategory = 'SERVICE' AND IsSoftDeleted = 0), 0)
                        - ISNULL((SELECT SUM(AppliedAmount) FROM InvoiceCharges WHERE InvoiceId = i.Id AND ChargeCategory = 'DISCOUNT' AND IsSoftDeleted = 0), 0)
                        + ISNULL((SELECT SUM(idt.TaxAmount) FROM InvoiceDetailTax idt INNER JOIN InvoiceDetail id ON idt.InvoiceDetailId = id.Id WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0), 0) AS TotalAmount
                ) invTotal
                WHERE i.OrganizationId = {organizationId} AND i.IsSoftDeleted = 0
                
                ORDER BY Date DESC";
            var transactionsResult = await dapper.SearchByQuery<RecentTransactionModel>(recentTransactionsSQL);
            dashboard.RecentTransactions = transactionsResult?.Take(10).ToList() ?? new List<RecentTransactionModel>();

            // 9. Cash Position (Receipts and Payments by Source)
            var cashPosition = new CashPositionModel();
            
            // Cash Receipts by Source
            string cashReceiptsSQL = $@"
                SELECT 
                    'CashBook' AS Source,
                    ISNULL(SUM(Amount), 0) AS Amount,
                    COUNT(*) AS TransactionCount
                FROM Cashbook
                WHERE OrganizationId = {organizationId}
                    AND UPPER(TranType) = 'RECEIPT'
                    AND IsSoftDeleted = 0
                
                UNION ALL
                
                SELECT 
                    'PettyCash' AS Source,
                    ISNULL(SUM(Amount), 0) AS Amount,
                    COUNT(*) AS TransactionCount
                FROM PettyCash
                WHERE OrganizationId = {organizationId}
                    AND UPPER(TranType) = 'RECEIPT'
                    AND IsSoftDeleted = 0
                
                UNION ALL
                
                SELECT 
                    'Sale Invoices' AS Source,
                    ISNULL(SUM(ip.Amount), 0) AS Amount,
                    COUNT(DISTINCT i.Id) AS TransactionCount
                FROM InvoicePayments ip
                INNER JOIN Invoice i ON i.Id = ip.InvoiceId
                INNER JOIN ChartOfAccounts coa ON coa.Id = ip.AccountId
                WHERE i.OrganizationId = {organizationId}
                    AND i.InvoiceType = 'SALE'
                    AND i.IsSoftDeleted = 0
                    AND ip.IsSoftDeleted = 0
                    AND coa.InterfaceType = 'PAYMENT METHOD'
                    AND (UPPER(coa.Name) LIKE '%CASH%' OR UPPER(coa.Name) = 'CASH IN HAND')";
            var receiptsResult = await dapper.SearchByQuery<CashSourceModel>(cashReceiptsSQL);
            cashPosition.ReceiptsBySource = receiptsResult ?? new List<CashSourceModel>();
            cashPosition.TotalReceipts = cashPosition.ReceiptsBySource.Sum(r => r.Amount);

            // Cash Payments by Source
            string cashPaymentsSQL = $@"
                SELECT 
                    'CashBook' AS Source,
                    ISNULL(SUM(Amount), 0) AS Amount,
                    COUNT(*) AS TransactionCount
                FROM Cashbook
                WHERE OrganizationId = {organizationId}
                    AND UPPER(TranType) = 'PAYMENT'
                    AND IsSoftDeleted = 0
                
                UNION ALL
                
                SELECT 
                    'PettyCash' AS Source,
                    ISNULL(SUM(Amount), 0) AS Amount,
                    COUNT(*) AS TransactionCount
                FROM PettyCash
                WHERE OrganizationId = {organizationId}
                    AND UPPER(TranType) = 'PAYMENT'
                    AND IsSoftDeleted = 0
                
                UNION ALL
                
                SELECT 
                    'Purchase Invoices' AS Source,
                    ISNULL(SUM(ip.Amount), 0) AS Amount,
                    COUNT(DISTINCT i.Id) AS TransactionCount
                FROM InvoicePayments ip
                INNER JOIN Invoice i ON i.Id = ip.InvoiceId
                INNER JOIN ChartOfAccounts coa ON coa.Id = ip.AccountId
                WHERE i.OrganizationId = {organizationId}
                    AND i.InvoiceType = 'PURCHASE'
                    AND i.IsSoftDeleted = 0
                    AND ip.IsSoftDeleted = 0
                    AND coa.InterfaceType = 'PAYMENT METHOD'
                    AND (UPPER(coa.Name) LIKE '%CASH%' OR UPPER(coa.Name) = 'CASH IN HAND')";
            var paymentsResult = await dapper.SearchByQuery<CashSourceModel>(cashPaymentsSQL);
            cashPosition.PaymentsBySource = paymentsResult ?? new List<CashSourceModel>();
            cashPosition.TotalPayments = cashPosition.PaymentsBySource.Sum(p => p.Amount);
            
            cashPosition.NetCashFlow = cashPosition.TotalReceipts - cashPosition.TotalPayments;
            dashboard.CashPosition = cashPosition;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting dashboard data for organization {OrganizationId}", organizationId);
        }

        return dashboard;
    }

    public async Task<List<CashMovementModel>> GetCashMovement(int organizationId, DateTime startDate, DateTime endDate, int? locationId = null)
    {
        string locationFilter = locationId.HasValue && locationId.Value > 0 ? $"AND LocationId = {locationId.Value}" : "";
        string startDateStr = startDate.ToString("yyyy-MM-dd");
        string endDateStr = endDate.AddDays(1).ToString("yyyy-MM-dd"); // Add 1 day to include end date

        string SQL = $@"
            WITH CashTransactions AS (
                -- CashBook transactions
                SELECT 
                    CAST(cb.TranDate AS DATE) AS TranDate,
                    ISNULL(loc.Name, 'All Locations') AS LocationName,
                    'CashBook' AS Source,
                    cb.Description,
                    cb.Reference,
                    CASE WHEN cb.TranType = 'RECEIPT' THEN cb.Amount ELSE 0 END AS Receipts,
                    CASE WHEN cb.TranType = 'PAYMENT' THEN cb.Amount ELSE 0 END AS Payments
                FROM Cashbook cb
                LEFT JOIN Locations loc ON loc.Id = cb.LocationId
                WHERE cb.IsSoftDeleted = 0 
                    AND cb.OrganizationId = {organizationId}
                    AND CAST(cb.TranDate AS DATE) >= '{startDateStr}'
                    AND CAST(cb.TranDate AS DATE) < '{endDateStr}'
                    {locationFilter}
                
                UNION ALL
                
                -- PettyCash transactions
                SELECT 
                    CAST(pc.TranDate AS DATE) AS TranDate,
                    ISNULL(loc.Name, 'All Locations') AS LocationName,
                    'PettyCash' AS Source,
                    pc.Description,
                    pc.Reference,
                    CASE WHEN pc.TranType = 'RECEIPT' THEN pc.Amount ELSE 0 END AS Receipts,
                    CASE WHEN pc.TranType = 'PAYMENT' THEN pc.Amount ELSE 0 END AS Payments
                FROM PettyCash pc
                LEFT JOIN Locations loc ON loc.Id = pc.LocationId
                WHERE pc.IsSoftDeleted = 0 
                    AND pc.OrganizationId = {organizationId}
                    AND CAST(pc.TranDate AS DATE) >= '{startDateStr}'
                    AND CAST(pc.TranDate AS DATE) < '{endDateStr}'
                    {locationFilter}
                
                UNION ALL
                
                -- Sale Invoice Payments (cash payments only)
                SELECT 
                    CAST(i.TranDate AS DATE) AS TranDate,
                    ISNULL(loc.Name, 'All Locations') AS LocationName,
                    'Sale Invoice' AS Source,
                    'Payment for Invoice: ' + i.Code AS Description,
                    i.Code AS Reference,
                    ip.Amount AS Receipts,
                    0 AS Payments
                FROM InvoicePayments ip
                INNER JOIN Invoice i ON i.Id = ip.InvoiceId
                INNER JOIN ChartOfAccounts coa ON coa.Id = ip.AccountId
                LEFT JOIN Locations loc ON loc.Id = i.LocationId
                WHERE ip.IsSoftDeleted = 0 
                    AND i.IsSoftDeleted = 0
                    AND i.OrganizationId = {organizationId}
                    AND i.InvoiceType = 'SALE'
                    AND coa.InterfaceType = 'PAYMENT METHOD'
                    AND (UPPER(coa.Name) LIKE '%CASH%' OR UPPER(coa.Name) = 'CASH IN HAND')
                    AND CAST(i.TranDate AS DATE) >= '{startDateStr}'
                    AND CAST(i.TranDate AS DATE) < '{endDateStr}'
                    {(locationId.HasValue && locationId.Value > 0 ? $"AND i.LocationId = {locationId.Value}" : "")}
                
                UNION ALL
                
                -- Purchase Invoice Payments (cash payments only)
                SELECT 
                    CAST(i.TranDate AS DATE) AS TranDate,
                    ISNULL(loc.Name, 'All Locations') AS LocationName,
                    'Purchase Invoice' AS Source,
                    'Payment for Invoice: ' + i.Code AS Description,
                    i.Code AS Reference,
                    0 AS Receipts,
                    ip.Amount AS Payments
                FROM InvoicePayments ip
                INNER JOIN Invoice i ON i.Id = ip.InvoiceId
                INNER JOIN ChartOfAccounts coa ON coa.Id = ip.AccountId
                LEFT JOIN Locations loc ON loc.Id = i.LocationId
                WHERE ip.IsSoftDeleted = 0 
                    AND i.IsSoftDeleted = 0
                    AND i.OrganizationId = {organizationId}
                    AND i.InvoiceType = 'PURCHASE'
                    AND coa.InterfaceType = 'PAYMENT METHOD'
                    AND (UPPER(coa.Name) LIKE '%CASH%' OR UPPER(coa.Name) = 'CASH IN HAND')
                    AND CAST(i.TranDate AS DATE) >= '{startDateStr}'
                    AND CAST(i.TranDate AS DATE) < '{endDateStr}'
                    {(locationId.HasValue && locationId.Value > 0 ? $"AND i.LocationId = {locationId.Value}" : "")}
            )
            SELECT 
                TranDate,
                LocationName,
                Source,
                Description,
                Reference,
                SUM(Receipts) AS Receipts,
                SUM(Payments) AS Payments,
                SUM(Receipts - Payments) AS Balance
            FROM CashTransactions
            GROUP BY TranDate, LocationName, Source, Description, Reference
            ORDER BY TranDate DESC, Source, LocationName";

        var result = await dapper.SearchByQuery<CashMovementModel>(SQL);
        return result ?? new List<CashMovementModel>();
    }

    public async Task<List<PartyReceivablePayableModel>> GetPartyReceivablePayable(int organizationId, string? partyType = null)
    {
        string partyTypeFilter = !string.IsNullOrWhiteSpace(partyType) 
            ? $"AND UPPER(p.PartyType) = '{partyType.ToUpperInvariant()}'" 
            : "";

        // Use the invoice report view which already has calculated amounts
        string SQL = $@"
            SELECT 
                p.Id AS PartyId,
                ISNULL(p.Code, '') AS PartyCode,
                ISNULL(p.Name, 'Unknown') AS PartyName,
                ISNULL(p.PartyType, '') AS PartyType,
                ISNULL((SELECT TOP 1 ContactValue FROM PartyContacts WHERE PartyId = p.Id AND ContactType IN ('PHONE', 'MOBILE') AND IsPrimary = 1 AND IsSoftDeleted = 0 ORDER BY CASE WHEN ContactType = 'PHONE' THEN 1 ELSE 2 END), '') AS Phone,
                ISNULL((SELECT TOP 1 ContactValue FROM PartyContacts WHERE PartyId = p.Id AND ContactType = 'EMAIL' AND IsPrimary = 1 AND IsSoftDeleted = 0), '') AS Email,
                ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'SALE' THEN v.BillAmount ELSE 0 END), 0) AS TotalInvoiceAmount,
                ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'SALE' THEN v.TotalPaidAmount ELSE 0 END), 0) AS TotalPaidAmount,
                ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'SALE' THEN v.BalanceAmount ELSE 0 END), 0) AS ReceivableAmount,
                ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'PURCHASE' THEN v.BalanceAmount ELSE 0 END), 0) AS PayableAmount,
                COUNT(DISTINCT v.Id) AS InvoiceCount,
                MAX(v.TranDate) AS LastTransactionDate
            FROM Parties p
            INNER JOIN vw_InvoiceMasterReport v ON v.PartyId = p.Id
            WHERE v.OrganizationId = {organizationId}
                AND p.IsSoftDeleted = 0
                {partyTypeFilter}
            GROUP BY p.Id, p.Code, p.Name, p.PartyType
            HAVING (
                ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'SALE' THEN v.BalanceAmount ELSE 0 END), 0) > 0
                OR
                ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'PURCHASE' THEN v.BalanceAmount ELSE 0 END), 0) > 0
            )
            ORDER BY 
                CASE WHEN ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'SALE' THEN v.BalanceAmount ELSE 0 END), 0) > 0 THEN 1 ELSE 2 END,
                ABS(ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'SALE' THEN v.BalanceAmount ELSE 0 END), 0) - 
                    ISNULL(SUM(CASE WHEN UPPER(v.InvoiceType) = 'PURCHASE' THEN v.BalanceAmount ELSE 0 END), 0)) DESC";

        var result = await dapper.SearchByQuery<PartyReceivablePayableModel>(SQL);
        
        // Calculate OutstandingBalance (Receivable - Payable)
        if (result != null)
        {
            foreach (var item in result)
            {
                item.OutstandingBalance = item.ReceivableAmount - item.PayableAmount;
            }
        }
        
        return result ?? new List<PartyReceivablePayableModel>();
    }
}

