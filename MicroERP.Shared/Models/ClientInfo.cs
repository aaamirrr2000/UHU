using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class ClientInfo
{
    public string IPAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string AcceptLanguage { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

