namespace NG.MicroERP.Shared.Models;

public class SystemConfigurationTreeItem
{
    public bool IsCategory { get; set; }
    public string? Category { get; set; }
    public int ChildrenCount { get; set; }
    public SystemConfigurationModel? ConfigItem { get; set; }
}
