using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface IReportsService
{
    Task<List<DailyFundsClosingModel>> GetDailyFundsClosing(int organizationId, DateTime reportDate);
    Task<List<InventoryClosingModel>> GetInventoryClosing(int organizationId, DateTime reportDate);
    Task<List<CashByLocationUserModel>> GetCashByLocationUser(int organizationId, DateTime asOfDate);
    Task<List<InventoryByLocationModel>> GetInventoryByLocation(int organizationId, DateTime asOfDate);
    Task<List<TrialBalanceModel>> GetTrialBalance(int organizationId, DateTime asOfDate);
    Task<List<AccountAnalysisModel>> GetAccountAnalysis(int organizationId, DateTime startDate, DateTime endDate);
    Task<ProfitLossReportModel> GetProfitLoss(int organizationId, DateTime startDate, DateTime endDate);
    Task<BalanceSheetReportModel> GetBalanceSheet(int organizationId, DateTime asOfDate);
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
                
                UNION ALL
                
                SELECT 
                    LocationId,
                    TranDate,
                    SUM(CASE WHEN TranType = 'RECEIPT' THEN Amount ELSE 0 END) AS Receipts,
                    SUM(CASE WHEN TranType = 'PAYMENT' THEN Amount ELSE 0 END) AS Payments
                FROM PettyCash
                WHERE IsSoftDeleted = 0 AND OrganizationId = {organizationId}
                GROUP BY LocationId, TranDate
                
                UNION ALL
                
                SELECT 
                    LocationId,
                    TranDate,
                    SUM(CASE WHEN TranType = 'RECEIPT' THEN Amount ELSE 0 END) AS Receipts,
                    SUM(CASE WHEN TranType = 'PAYMENT' THEN Amount ELSE 0 END) AS Payments
                FROM Advances
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
                
                UNION ALL
                
                SELECT 
                    pc.LocationId,
                    pc.CreatedBy AS UserId,
                    SUM(CASE WHEN pc.TranType = 'RECEIPT' THEN pc.Amount ELSE -pc.Amount END) AS CashAmount,
                    MAX(pc.TranDate) AS LastTransactionDate
                FROM PettyCash pc
                WHERE pc.IsSoftDeleted = 0 AND pc.OrganizationId = {organizationId}
                    AND pc.TranDate <= '{dateStr}'
                GROUP BY pc.LocationId, pc.CreatedBy
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

    public async Task<List<TrialBalanceModel>> GetTrialBalance(int organizationId, DateTime asOfDate)
    {
        string dateStr = asOfDate.ToString("yyyy-MM-dd");
        
        string SQL = $@"
            SELECT 
                coa.Code AS AccountCode,
                coa.Name AS AccountName,
                SUM(CASE WHEN glr.DebitAmount > 0 THEN glr.DebitAmount ELSE 0 END) AS Debit,
                SUM(CASE WHEN glr.CreditAmount > 0 THEN glr.CreditAmount ELSE 0 END) AS Credit
            FROM vw_GeneralLedgerReport glr
            INNER JOIN ChartOfAccounts coa ON coa.Id = glr.AccountId
            WHERE glr.OrganizationId = {organizationId}
                AND glr.EntryDate <= '{dateStr}'
                AND glr.IsPosted = 1
            GROUP BY coa.Code, coa.Name
            HAVING SUM(glr.DebitAmount) <> 0 OR SUM(glr.CreditAmount) <> 0
            ORDER BY coa.Code";

        var result = await dapper.SearchByQuery<TrialBalanceModel>(SQL);
        return result ?? new List<TrialBalanceModel>();
    }

    public async Task<List<AccountAnalysisModel>> GetAccountAnalysis(int organizationId, DateTime startDate, DateTime endDate)
    {
        string startStr = startDate.ToString("yyyy-MM-dd");
        string endStr = endDate.ToString("yyyy-MM-dd");
        
        string SQL = $@"
            WITH OpeningBalances AS (
                SELECT 
                    AccountId,
                    SUM(DebitAmount - CreditAmount) AS OpeningBalance
                FROM vw_GeneralLedgerReport
                WHERE OrganizationId = {organizationId}
                    AND EntryDate < '{startStr}'
                    AND IsPosted = 1
                GROUP BY AccountId
            ),
            PeriodTransactions AS (
                SELECT 
                    AccountId,
                    SUM(DebitAmount) AS Debit,
                    SUM(CreditAmount) AS Credit
                FROM vw_GeneralLedgerReport
                WHERE OrganizationId = {organizationId}
                    AND EntryDate >= '{startStr}' AND EntryDate <= '{endStr}'
                    AND IsPosted = 1
                GROUP BY AccountId
            )
            SELECT 
                coa.Code AS AccountCode,
                coa.Name AS AccountName,
                ISNULL(ob.OpeningBalance, 0) AS OpeningBalance,
                ISNULL(pt.Debit, 0) AS Debit,
                ISNULL(pt.Credit, 0) AS Credit,
                ISNULL(ob.OpeningBalance, 0) + ISNULL(pt.Debit, 0) - ISNULL(pt.Credit, 0) AS ClosingBalance
            FROM ChartOfAccounts coa
            LEFT JOIN OpeningBalances ob ON ob.AccountId = coa.Id
            LEFT JOIN PeriodTransactions pt ON pt.AccountId = coa.Id
            WHERE coa.OrganizationId = {organizationId}
                AND coa.IsActive = 1
                AND (ob.OpeningBalance <> 0 OR pt.Debit <> 0 OR pt.Credit <> 0)
            ORDER BY coa.Code";

        var result = await dapper.SearchByQuery<AccountAnalysisModel>(SQL);
        return result ?? new List<AccountAnalysisModel>();
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
}
