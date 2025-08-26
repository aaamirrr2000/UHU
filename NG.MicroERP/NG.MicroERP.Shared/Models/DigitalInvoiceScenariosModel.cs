using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class DigitalInvoiceScenariosModel
{
    public int Id { get; set; } = 0;
    public string? ScenarioID { get; set; } = string.Empty;
    public string? SaleType { get; set; } = string.Empty;
    public string? BuyerType { get; set; } = string.Empty;
    public string? TaxContext { get; set; } = string.Empty;

}