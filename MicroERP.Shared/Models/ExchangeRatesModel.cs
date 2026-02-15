using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class ExchangeRatesModel
{
    public int Id { get; set; } = 0;
    public int BaseCurrencyId { get; set; } = 0;
    public int TargetCurrencyId { get; set; } = 0;
    public decimal Rate { get; set; } = 0;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; } = null;
    public string? Source { get; set; } = string.Empty;
    public string? Remarks { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime? UpdatedOn { get; set; } = null;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    // UI Display Fields
    public string BaseCurrencyCode { get; set; } = string.Empty;
    public string BaseCurrencyName { get; set; } = string.Empty;
    public string TargetCurrencyCode { get; set; } = string.Empty;
    public string TargetCurrencyName { get; set; } = string.Empty;
    public string DisplayName => $"{BaseCurrencyCode} → {TargetCurrencyCode} ({Rate:N6})";
    public string CurrencyPair => $"{BaseCurrencyCode}/{TargetCurrencyName}";

    public override string ToString()
    {
        return DisplayName ?? string.Empty;
    }
}

