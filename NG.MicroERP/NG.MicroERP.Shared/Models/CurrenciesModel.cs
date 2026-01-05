using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class CurrenciesModel
{
    public int Id { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? Symbol { get; set; } = string.Empty;
    public string? Country { get; set; } = string.Empty;
    public int IsBaseCurrency { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public string? DisplayName => $"{Code} - {Name}";

    public override string ToString()
    {
        return DisplayName ?? string.Empty;
    }
}