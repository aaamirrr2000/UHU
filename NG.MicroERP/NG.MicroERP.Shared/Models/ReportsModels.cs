using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

// Report Models
public class DailyFundsClosingModel
{
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Receipts { get; set; }
    public decimal Payments { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class InventoryClosingModel
{
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? LocationName { get; set; }
    public decimal OpeningQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal IssuedQty { get; set; }
    public decimal ClosingQty { get; set; }
    public decimal ClosingValue { get; set; }
}

public class CashByLocationUserModel
{
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserFullName { get; set; }
    public decimal CashAmount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

public class InventoryByLocationModel
{
    public string? LocationName { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
}

public class TrialBalanceModel
{
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class AccountAnalysisModel
{
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class ProfitLossReportModel
{
    public List<ProfitLossItemModel> Revenue { get; set; } = new();
    public List<ProfitLossItemModel> Expenses { get; set; } = new();
}

public class ProfitLossItemModel
{
    public string? AccountName { get; set; }
    public decimal Amount { get; set; }
}

public class BalanceSheetReportModel
{
    public List<BalanceSheetItemModel> Assets { get; set; } = new();
    public List<BalanceSheetItemModel> Liabilities { get; set; } = new();
    public List<BalanceSheetItemModel> Equity { get; set; } = new();
}

public class BalanceSheetItemModel
{
    public string? AccountName { get; set; }
    public decimal Amount { get; set; }
}
