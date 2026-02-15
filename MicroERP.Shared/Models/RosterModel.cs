using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class RosterModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public int EmployeeId { get; set; } = 0;
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public int ShiftId { get; set; } = 0;
    public string? ShiftName { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public DateTime RosterDate { get; set; } = DateTime.Today;
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public int IsActive { get; set; } = 1;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
}

