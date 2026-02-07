using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class TaxRuleModel
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? RuleName { get; set; }
    public string? AppliesTo { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public int IsSoftDeleted { get; set; }
}

public class TaxRuleDetailModel
{
    public int Id { get; set; }
    public int TaxRuleId { get; set; }
    public int TaxId { get; set; }
    public int SequenceNo { get; set; }
    public string? TaxType { get; set; } // PERCENTAGE or FLAT (required in rule detail)
    public string? TaxBaseType { get; set; } // BASE_ONLY / BASE_PLUS_SELECTED / RUNNING_TOTAL (required in rule detail)
    public decimal? TaxAmount { get; set; } // Tax rate/amount (required in rule detail)
    public int? IsRegistered { get; set; } = null; // NULL = Not applicable, 1 = Registered, 0 = Unregistered
    public int? IsFiler { get; set; } = null; // NULL = Not applicable, 1 = Filer, 0 = Non-Filer

    // Display/Join fields
    public string? TaxName { get; set; }
    public string? Description { get; set; }
    public string? RuleName { get; set; }
    public string? AppliesTo { get; set; }
    public string? Account { get; set; }
    public string? ConditionType { get; set; } // From TaxMaster (text value)
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}