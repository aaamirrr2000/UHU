using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class CustomerItemDiscountRulesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public int? PartyId { get; set; } = null;
    public int? ItemId { get; set; } = null;
    public string? AmountType { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
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

    // Display fields
    public string? PartyName { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
}
