using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class DisbursementModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public int EmployeeId { get; set; } = 0;
    public string? EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; } = string.Empty;
    public int DepartmentId { get; set; } = 0;
    public string? DepartmentName { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public string? DisbursementType { get; set; } = string.Empty; // ADVANCE, LOAN, BONUS, ALLOWANCE, OVERTIME, OTHER
    public decimal Amount { get; set; } = 0;
    public string? Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; } = 1;
    public decimal AmountInBaseCurrency { get; set; } = 0;
    public string? PaymentMethod { get; set; } = string.Empty; // CASH, BANK_TRANSFER, CHEQUE
    public int BankAccountId { get; set; } = 0;
    public string? BankAccountNumber { get; set; } = string.Empty;
    public string? ChequeNumber { get; set; } = string.Empty;
    public DateTime? ChequeDate { get; set; }
    public string? ReferenceNo { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty; // PENDING, APPROVED, PAID, CANCELLED
    public int ApprovedBy { get; set; } = 0;
    public DateTime? ApprovedDate { get; set; }
    public int PaidBy { get; set; } = 0;
    public DateTime? PaidDate { get; set; }
    public string? Remarks { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Now;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    // UI Display Fields
    public string? ApprovedByName { get; set; }
    public string? PaidByName { get; set; }
}

