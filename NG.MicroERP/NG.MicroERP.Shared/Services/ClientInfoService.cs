using Microsoft.AspNetCore.Http;
using NG.MicroERP.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Services;

public class ClientInfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientInfoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClientInfo GetClientInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return new ClientInfo();

        return new ClientInfo
        {
            IPAddress = GetClientIPAddress(httpContext),
            UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
            AcceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString(),
            ConnectionId = httpContext.Connection.Id,
            IsLocal = httpContext.Connection.RemoteIpAddress?.IsIPv4MappedToIPv6 == true ||
                     IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress),
            Headers = httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        };
    }

    private string GetClientIPAddress(HttpContext httpContext)
    {
        // Check headers for forwarded IP (behind proxy)
        var headers = new[] { "X-Forwarded-For", "X-Real-IP", "CF-Connecting-IP" };

        foreach (var header in headers)
        {
            var headerValue = httpContext.Request.Headers[header].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValue))
            {
                return headerValue.Split(',').First().Trim();
            }
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    public string GetDetailedClientInfo()
    {
        var clientInfo = GetClientInfo();
        var httpContext = _httpContextAccessor.HttpContext;

        var info = $"IP:{clientInfo.IPAddress} | OS:{ParseUserAgent(clientInfo.UserAgent).OS} | {GetWindowsUserInfo()} | {GetUserEmail(httpContext.User)} | {GetAuthProvider(httpContext.User)} | Device:{ParseUserAgent(clientInfo.UserAgent).DeviceType} | Browser:{ParseUserAgent(clientInfo.UserAgent).Browser} | Conn:{clientInfo.ConnectionId} | Local:{clientInfo.IsLocal} | Port:{httpContext?.Connection.RemotePort}";

        return info.Length <= 255 ? info : info.Substring(0, 255);
    }

    private string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }

    private (string OS, string Browser, string DeviceType) ParseUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return ("Unknown", "Unknown", "Unknown");

        var os = "Unknown";
        var browser = "Unknown";
        var deviceType = "Desktop";

        // Detect OS
        if (userAgent.Contains("Windows NT 10.0")) os = "Windows 10/11";
        else if (userAgent.Contains("Windows NT 6.3")) os = "Windows 8.1";
        else if (userAgent.Contains("Windows NT 6.2")) os = "Windows 8";
        else if (userAgent.Contains("Windows NT 6.1")) os = "Windows 7";
        else if (userAgent.Contains("Mac OS X")) os = "macOS";
        else if (userAgent.Contains("Linux")) os = "Linux";
        else if (userAgent.Contains("Android")) os = "Android";
        else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) os = "iOS";

        // Detect Browser
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg")) browser = "Chrome";
        else if (userAgent.Contains("Firefox")) browser = "Firefox";
        else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) browser = "Safari";
        else if (userAgent.Contains("Edg")) browser = "Edge";
        else if (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) browser = "Internet Explorer";
        else if (userAgent.Contains("Opera")) browser = "Opera";

        // Detect Device Type
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            deviceType = "Mobile";
        else if (userAgent.Contains("Tablet"))
            deviceType = "Tablet";

        return (os, browser, deviceType);
    }

    public string GetWindowsUserInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity is System.Security.Principal.WindowsIdentity windowsIdentity)
        {
            return $"Windows User: {windowsIdentity.Name} | Groups: {windowsIdentity.Groups?.Count} | Authenticated: {windowsIdentity.IsAuthenticated}";
        }

        return "Not Windows Authentication";
    }

    private string GetUserEmail(System.Security.Claims.ClaimsPrincipal user)
    {
        // Check multiple possible claim types for email
        var emailClaims = new[]
        {
        "email",
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
        "preferred_username",
        "upn",
        "unique_name"
    };

        foreach (var claimType in emailClaims)
        {
            var email = user.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
            if (!string.IsNullOrEmpty(email) && email.Contains("@"))
                return email;
        }

        return user.Identity?.Name ?? "Unknown";
    }

    private string GetAuthProvider(System.Security.Claims.ClaimsPrincipal user)
    {
        // Detect common authentication providers
        var identityProvider = user.Claims.FirstOrDefault(c => c.Type == "idp" || c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider")?.Value;

        if (!string.IsNullOrEmpty(identityProvider))
        {
            return identityProvider.ToLower() switch
            {
                "google" => "Google",
                "facebook" => "Facebook",
                "microsoft" => "Microsoft",
                "github" => "GitHub",
                "twitter" => "Twitter",
                _ => identityProvider
            };
        }

        return user.Identity?.AuthenticationType ?? "Application";
    }
}