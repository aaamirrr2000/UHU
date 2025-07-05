using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class ServiceChargesModel
{
    public int Id { get; set; } = 0;
    public string? ChargeName { get; set; } = string.Empty;
    public string? ChargeType { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public int? AppliesTo { get; set; } = 0;
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    public DateTime EffectiveTo { get; set; } = DateTime.Today;
    public int OrganizationId { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public string? CreatedSource { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

}