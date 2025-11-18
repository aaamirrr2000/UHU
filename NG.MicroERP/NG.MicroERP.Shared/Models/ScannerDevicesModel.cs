using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class ScannerDevicesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? DeviceIpAddress { get; set; } = string.Empty;
    public string? UserName { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; }
    public string? Make { get; set; } = string.Empty;
    public string? Model { get; set; } = string.Empty;
    public string? Serial { get; set; } = string.Empty;
    public int IsActive { get; set; }
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public bool IsSoftDeleted { get; set; }
    public string? InOutAll { get; set; } = string.Empty;
}