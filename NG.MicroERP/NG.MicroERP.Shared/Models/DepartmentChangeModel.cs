using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class DepartmentChangeModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public int EmployeeId { get; set; } = 0;
    public string? EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; } = string.Empty;
    public DateTime? ChangeDate { get; set; }
    public int OldDepartmentId { get; set; } = 0;
    public string? OldDepartmentName { get; set; }
    public int NewDepartmentId { get; set; } = 0;
    public string? NewDepartmentName { get; set; }
    public string? ChangeType { get; set; } = string.Empty; // TRANSFER, RESTRUCTURE, REORGANIZATION
    public string? Reason { get; set; } = string.Empty;
    public string? Remarks { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty; // PENDING, APPROVED, EFFECTED
    public int ApprovedBy { get; set; } = 0;
    public DateTime? ApprovedDate { get; set; }
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
