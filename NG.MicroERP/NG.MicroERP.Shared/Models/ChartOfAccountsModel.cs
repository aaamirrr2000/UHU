using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class ChartOfAccountsModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? Pic { get; set; } = string.Empty;
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;
    public string? InterfaceType { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public int ParentId { get; set; } = 0;
    public double OpeningBalance { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public string? DisplayName =>
        string.Format("{0} ({1}), {2}",
            Name, Code, Type);

    public override string ToString()
    {
        return Name ?? string.Empty;
    }
}