using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class TreeItem
{
    public int Id { get; set; }
    public string? Text { get; set; } = string.Empty;
    public string? Icon { get; set; } = Icons.Material.Filled.Folder;
    public bool Expanded { get; set; } = false;
    public List<TreeItem> Children { get; set; } = new();
}
