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
    public int IsRegistered { get; set; }
    public int IsFiler { get; set; }
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

    //
    public string? TaxName { get; set; }
    public string? TaxType { get; set; }
    public decimal? Rate { get; set; }
    public string? Description { get; set; }
    public string? RuleName { get; set; }
    public string? AppliesTo { get; set; }
    public string? TaxBaseType { get; set; }
    public string? Account { get; set; }
    public int IsRegistered { get; set; } = 0;
    public int IsFiler { get; set; } = 0;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}