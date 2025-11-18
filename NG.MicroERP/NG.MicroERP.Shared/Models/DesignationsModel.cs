using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class DesignationsModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? DesignationName { get; set; } = string.Empty;
    public int ParentId { get; set; } = 0;
    public string? ReportTo { get; set; }
    public int DepartmentId { get; set; } = 0;
    public string? DepartmentName { get; set; }
    public string? Description { get; set; } = string.Empty;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public bool IsSoftDeleted { get; set; }

}
