using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class ChargesRulesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? Account { get; set; }
    public string? AmountType { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public string? ChargeCategory { get; set; } = string.Empty;
    public DateTime? EffectiveFrom { get; set; } = DateTime.Today;
    public DateTime? EffectiveTo { get; set; } = DateTime.Today;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

}
