using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class SystemConfigurationModel
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? Category { get; set; }
    public string? ConfigKey { get; set; }
    public string? ConfigValue { get; set; }
    public string? Description { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public bool IsSoftDeleted { get; set; }
}
