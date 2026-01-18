using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NG.MicroERP.Shared.Models;

public class EmailResponseModel
{
    [JsonProperty("success")]
    public bool Success { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonProperty("error")]
    public string? Error { get; set; }
}
