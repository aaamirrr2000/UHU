using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;


namespace NG.MicroERP.Shared.Services;

public class ClientInfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientInfoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetClientInfoString()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return $"Anonymous|0.0.0.0|Unknown|Unknown|{DateTime.UtcNow:MMddHHmm}";

        // Username
        string username = "Anonymous";
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            username = context.User.Identity.Name ?? "Anonymous";

            // Remove domain if present
            if (username.Contains('\\'))
                username = username.Split('\\').Last();
            else if (username.Contains('@'))
                username = username.Split('@')[0];
        }

        // IP Address
        string ip = context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        if (ip == "::1") ip = "127.0.0.1";

        // Browser & OS from User-Agent
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var browser = GetSimpleBrowser(userAgent);
        var os = GetSimpleOS(userAgent);

        // Timestamp
        var time = DateTime.UtcNow.ToString("MMddHHmm");

        return $"{username}|{ip}|{browser}|{os}|{time}";
    }

    private string GetSimpleBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        var ua = userAgent.ToLower();

        if (ua.Contains("edge")) return "Edge";
        if (ua.Contains("chrome") && !ua.Contains("edge")) return "Chrome";
        if (ua.Contains("firefox")) return "Firefox";
        if (ua.Contains("safari") && !ua.Contains("chrome")) return "Safari";

        return "Other";
    }

    private string GetSimpleOS(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        var ua = userAgent.ToLower();

        // Windows 11 often reports as Windows NT 10.0
        if (ua.Contains("windows nt 10.0"))
            return "Windows 10/11"; // Combine since they report similarly

        if (ua.Contains("windows nt 11.0")) return "Windows 11";
        if (ua.Contains("windows")) return "Windows";
        if (ua.Contains("mac")) return "macOS";
        if (ua.Contains("linux")) return "Linux";
        if (ua.Contains("android")) return "Android";
        if (ua.Contains("iphone") || ua.Contains("ipad")) return "iOS";

        return "Unknown";
    }
}