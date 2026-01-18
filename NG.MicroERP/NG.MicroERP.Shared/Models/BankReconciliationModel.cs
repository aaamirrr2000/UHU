using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class BankReconciliationModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? ReconciliationNo { get; set; } = string.Empty;
    public int BankAccountId { get; set; } = 0;
    public string? BankAccountName { get; set; } = string.Empty;
    public string? BankAccountCode { get; set; } = string.Empty;
    public DateTime? StatementDate { get; set; } = DateTime.Today;
    public double OpeningBalance { get; set; } = 0;
    public double StatementBalance { get; set; } = 0;
    public double BookBalance { get; set; } = 0;
    public double Difference { get; set; } = 0;
    public string? Status { get; set; } = "OPEN"; // OPEN, RECONCILED, CLOSED
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public List<BankReconciliationDetailModel> Details { get; set; } = new List<BankReconciliationDetailModel>();

    public string? DisplayName => $"{ReconciliationNo} - {BankAccountName} ({StatementDate:yyyy-MM-dd})";
}

public class BankReconciliationDetailModel
{
    public int Id { get; set; } = 0;
    public int ReconciliationId { get; set; } = 0;
    public string? TransactionType { get; set; } = string.Empty; // DEPOSIT, WITHDRAWAL, CHARGE, INTEREST
    public DateTime? TransactionDate { get; set; } = DateTime.Today;
    public string? ReferenceNo { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public int IsMatched { get; set; } = 0; // 0 = Unmatched, 1 = Matched
    public int MatchedTransactionId { get; set; } = 0; // Reference to CashBook or GL entry
    public string? MatchedTransactionType { get; set; } = string.Empty; // CASHBOOK, GENERALLEDGER
    public string? Source { get; set; } = string.Empty; // STATEMENT, BOOK
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
}
