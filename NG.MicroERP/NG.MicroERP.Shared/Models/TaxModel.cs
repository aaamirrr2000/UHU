using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;
public class TaxModel
{
    public int Id { get; set; } = 0;
    public Guid GUID { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? TaxName { get; set; } = string.Empty;
    public string? TaxCode { get; set; } = string.Empty;
    public double RatePercent { get; set; } = 0;
    public int IsCompound { get; set; } = 0;
    public string? AppliesTo { get; set; } = string.Empty;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

}


