using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikConnect.Models;

public class ScannerDeviceModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();

    public string DeviceIpAddress { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public int? LocationId { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Serial { get; set; }

    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string? CreatedFrom { get; set; }

    public int? UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; } = DateTime.Now;
    public string? UpdatedFrom { get; set; }

    public bool IsSoftDeleted { get; set; } = false;

    public DateTime? RowVersion { get; set; }
}