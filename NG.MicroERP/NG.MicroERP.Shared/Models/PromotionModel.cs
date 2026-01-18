using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class PromotionModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public int EmployeeId { get; set; } = 0;
    public string? EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; } = string.Empty;
    public DateTime? PromotionDate { get; set; }
    public int OldDepartmentId { get; set; } = 0;
    public string? OldDepartmentName { get; set; }
    public int NewDepartmentId { get; set; } = 0;
    public string? NewDepartmentName { get; set; }
    public int OldDesignationId { get; set; } = 0;
    public string? OldDesignationName { get; set; }
    public int NewDesignationId { get; set; } = 0;
    public string? NewDesignationName { get; set; }
    public decimal OldBasicSalary { get; set; } = 0;
    public decimal NewBasicSalary { get; set; } = 0;
    public string? PromotionType { get; set; } = string.Empty; // PROMOTION, DEMOTION, TRANSFER
    public string? Reason { get; set; } = string.Empty;
    public string? Remarks { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty; // PENDING, APPROVED, REJECTED
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
