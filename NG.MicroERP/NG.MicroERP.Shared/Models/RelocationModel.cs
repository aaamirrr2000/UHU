using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class RelocationModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public int EmployeeId { get; set; } = 0;
    public string? EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; } = string.Empty;
    public DateTime? RelocationDate { get; set; }
    public int OldLocationId { get; set; } = 0;
    public string? OldLocationName { get; set; }
    public int NewLocationId { get; set; } = 0;
    public string? NewLocationName { get; set; }
    public string? RelocationType { get; set; } = string.Empty; // PERMANENT, TEMPORARY, PROJECT_BASED
    public string? Reason { get; set; } = string.Empty;
    public decimal RelocationAllowance { get; set; } = 0;
    public decimal TravelExpenses { get; set; } = 0;
    public decimal AccommodationAllowance { get; set; } = 0;
    public decimal TotalRelocationCost { get; set; } = 0;
    public string? Status { get; set; } = string.Empty; // PENDING, APPROVED, PROCESSED, COMPLETED
    public int ApprovedBy { get; set; } = 0;
    public DateTime? ApprovedDate { get; set; }
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
