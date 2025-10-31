using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class EmployeesModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? EmpId { get; set; } = string.Empty;
    public string? Fullname { get; set; } = string.Empty;
    public string? Pic { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Cnic { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? EmpType { get; set; } = string.Empty;
    public int DepartmentId { get; set; } = 0;
    public int DesignationId { get; set; } = 0;
    public int ShiftId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public int ParentId { get; set; } = 0;
    public int ExcludeFromAttendance { get; set; } = 0;
    public int IsActive { get; set; }
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public bool IsSoftDeleted { get; set; }
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();
}