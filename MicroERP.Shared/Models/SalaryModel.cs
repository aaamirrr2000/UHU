using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class SalaryModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? SalaryMonth { get; set; } = string.Empty; // Format: YYYY-MM
    public int EmployeeId { get; set; } = 0;
    public string? EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; } = string.Empty;
    public int DepartmentId { get; set; } = 0;
    public string? DepartmentName { get; set; }
    public int DesignationId { get; set; } = 0;
    public string? DesignationName { get; set; }
    public DateTime? PayDate { get; set; }
    public decimal BasicSalary { get; set; } = 0;
    public decimal Allowances { get; set; } = 0;
    public decimal Bonuses { get; set; } = 0;
    public decimal Overtime { get; set; } = 0;
    public decimal GrossSalary { get; set; } = 0;
    public decimal Tax { get; set; } = 0;
    public decimal ProvidentFund { get; set; } = 0;
    public decimal Insurance { get; set; } = 0;
    public decimal Loans { get; set; } = 0;
    public decimal Deductions { get; set; } = 0;
    public decimal TotalDeductions { get; set; } = 0;
    public decimal NetSalary { get; set; } = 0;
    public string? PaymentMethod { get; set; } = string.Empty; // CASH, BANK_TRANSFER, CHEQUE
    public int BankAccountId { get; set; } = 0;
    public string? BankAccountNumber { get; set; } = string.Empty;
    public string? ChequeNumber { get; set; } = string.Empty;
    public DateTime? ChequeDate { get; set; }
    public string? Status { get; set; } = string.Empty; // PENDING, APPROVED, PAID, CANCELLED
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Now;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    // UI Display Fields
    public string? LocationName { get; set; }
    public string? UserName { get; set; }
}

public class SalaryReportModel
{
    public int Id { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? SalaryMonth { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public DateTime? PayDate { get; set; }
    public decimal BasicSalary { get; set; } = 0;
    public decimal Allowances { get; set; } = 0;
    public decimal Bonuses { get; set; } = 0;
    public decimal Overtime { get; set; } = 0;
    public decimal GrossSalary { get; set; } = 0;
    public decimal TotalDeductions { get; set; } = 0;
    public decimal NetSalary { get; set; } = 0;
    public string? PaymentMethod { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
}

