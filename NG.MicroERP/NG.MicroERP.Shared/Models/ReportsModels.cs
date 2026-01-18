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

public class EmployeeAdvanceReportModel
{
    public int PartyId { get; set; }
    public string? PartyCode { get; set; }
    public string? PartyName { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public decimal TotalAdvanceGiven { get; set; }
    public decimal TotalAdvanceRecovered { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int TransactionCount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

public class CashReconciliationReportModel
{
    public DateTime? CountDate { get; set; }
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? Locker { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CashReceipts { get; set; }
    public decimal CashPayments { get; set; }
    public decimal ExpectedBalance { get; set; } // Opening + Receipts - Payments
    public decimal PhysicalCount { get; set; } // Total from PhysicalCashCount
    public decimal Variance { get; set; } // PhysicalCount - ExpectedBalance
    public string? Status { get; set; } // RECONCILED, NOT RECONCILED
    public string? CountedByName { get; set; }
    public DateTime? CountedOn { get; set; }
    public int CountedBy { get; set; } // Added for filtering
}

public class PhysicalCashCountSessionModel
{
    public DateTime? CountDate { get; set; }
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? Locker { get; set; }
    public int CountedBy { get; set; }
    public string? CountedByName { get; set; }
    public DateTime? CountedOn { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? DisplayText { get; set; }
    
    // Unique key for comparison
    public string SessionKey => $"{CountDate:yyyy-MM-dd}_{LocationId}_{Locker}_{CountedBy}_{CountedOn:yyyy-MM-dd HH:mm:ss}";
    
    public override bool Equals(object? obj)
    {
        if (obj is PhysicalCashCountSessionModel other)
        {
            return SessionKey == other.SessionKey;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return SessionKey?.GetHashCode() ?? 0;
    }
}
