using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class DigitalInvoiceConfigModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Country { get; set; } = string.Empty;
    public string? PosId { get; set; } = string.Empty;
    public string? ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; } = string.Empty;
    public string? Username { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public string? Target { get; set; } = string.Empty;
    public string? TargetApi { get; set; } = string.Empty;
    public int IsDefault { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

}