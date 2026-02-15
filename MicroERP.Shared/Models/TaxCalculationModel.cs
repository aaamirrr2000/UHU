using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class ItemPartyTaxCalculationModel
{
    public int OrganizationId { get; set; }
    public int AccountId { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int PartyId { get; set; }
    public string? PartyName { get; set; }
    public double BasePrice { get; set; }
    public string? TaxName { get; set; }
    public double Percentage { get; set; }
    public double TaxableAmount { get; set; }
    public double TaxAmount { get; set; }
    public double FinalPrice { get; set; }
    public string? AppliesTo { get; set; }
    public int SequenceNo { get; set; } = 0; // Sequence number for tax calculation order

}
