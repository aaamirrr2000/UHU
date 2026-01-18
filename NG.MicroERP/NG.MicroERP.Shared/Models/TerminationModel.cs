using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class TerminationModel
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
    public DateTime? TerminationDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
    public string? TerminationType { get; set; } = string.Empty; // RESIGNATION, TERMINATION, RETIREMENT, END_OF_CONTRACT
    public string? TerminationReason { get; set; } = string.Empty;
    public string? NoticePeriod { get; set; } = string.Empty;
    public decimal SeverancePay { get; set; } = 0;
    public decimal OutstandingSalary { get; set; } = 0;
    public decimal OutstandingLeaves { get; set; } = 0;
    public decimal FinalSettlement { get; set; } = 0;
    public string? Status { get; set; } = string.Empty; // PENDING, APPROVED, PROCESSED
    public int ApprovedBy { get; set; } = 0;
    public DateTime? ApprovedDate { get; set; }
    public string? HandoverNotes { get; set; } = string.Empty;
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
}
