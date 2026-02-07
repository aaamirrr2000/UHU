using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class TaxCalculationResultModel
{
    public int SequenceNo { get; set; }
    public string? TaxCode { get; set; }         // e.g., GST, ADV_TAX
    public string? TaxName { get; set; }         // e.g., General Sales Tax
    public string? TaxType { get; set; }          // PERCENTAGE or FLAT
    public double TaxRate { get; set; }           // Tax rate/amount, e.g., 18.0
    public double TaxableAmount { get; set; }     // Amount on which tax is calculated
    public double TaxAmount { get; set; }         // Calculated tax amount
    public string? ImpactType { get; set; }       // ADD / DEDUCT
    public int IsConditional { get; set; }        // 0 = Not conditional, 1 = Conditional
    public int? IsRegistered { get; set; }       // NULL, 1, or 0
    public int? IsFiler { get; set; }            // NULL, 1, or 0
}


public class TaxCalculationModel
{
    public int OrganizationId { get; set; } = 1;

    public decimal Amount { get; set; }

    public int? ItemId { get; set; }

    public string AppliesTo { get; set; }   // SALE / PURCHASE

    public int IsGSTRegistered { get; set; } // NULL = unknown / ALL
    public int IsFiler { get; set; }          // NULL = unknown / ALL

    public DateTime TransactionDate { get; set; } = DateTime.Today;
}

