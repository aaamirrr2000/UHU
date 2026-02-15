using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class InterfaceSettingsModel
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? Category { get; set; }
    public string? SettingKey { get; set; }
    public string? SettingValue { get; set; }
    public string? DataType { get; set; } = "STRING"; // STRING, BOOLEAN, NUMBER
    public string? Description { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public int IsSoftDeleted { get; set; }
}

public class InterfaceSettingsTreeItem
{
    public bool IsCategory { get; set; }
    public string? Category { get; set; }
    public int ChildrenCount { get; set; }
    public InterfaceSettingsModel? SettingItem { get; set; }
}

