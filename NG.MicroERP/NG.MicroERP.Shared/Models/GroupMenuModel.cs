using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class GroupMenuModel
{
    public int GroupId { get; set; } = 0;
    public string? GroupName { get; set; } = string.Empty;
    public int IsActive { get; set; } = 0;
    public int MenuId { get; set; } = 0;
    public string? MenuCaption { get; set; } = string.Empty;
    public string? Tooltip { get; set; } = string.Empty;
    public string? Parameter { get; set; } = string.Empty;
    public int ParentId { get; set; } = 0;
    public string? PageName { get; set; } = string.Empty;
    public string? Icon { get; set; } = string.Empty;
    public int SeqNo { get; set; } = 0;
    public int Live { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? My_Privilege { get; set; }
    public bool IsToggled { get; set; } = false;

}
