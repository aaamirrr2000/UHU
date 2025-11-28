using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class BackupResult
{
    public bool Success { get; set; }
    public string BackupPath { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public object Details { get; set; }
}