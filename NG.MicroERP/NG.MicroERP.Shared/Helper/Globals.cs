
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Services;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace NG.MicroERP.Shared.Helper;

public class Globals
{
    public string BaseURI = "";
    public string key = "aT4hmLHkxfeXaT4h";

    public  string Computer_sr = string.Empty;
    public  string Token { get; set; } = string.Empty;
    public  string ListName = string.Empty;
    public  string Icon = string.Empty;
    public  string Version = "1.0p";
    public  bool _tabsInitialized = false;
    public  bool _isDarkMode;

    public  string PageTitle { get; set; } = "";
    public  OrganizationsModel Organization { get; set; } = new();
    public  UsersModel User { get; set; } = new();
    public  EmployeesModel Employee { get; set; } = new()!;
    public  DepartmentsModel Department { get; set; } = new()!;
    public  List<MyMenuModel>? menu { get; set; } = new();
    public  bool isVisible { get; set; } = true;
    public  string seletctedMenuItem { get; set; } = string.Empty;
    public  bool MyLeave { get; set; } = true;
    public  List<GroupMenuModel> MyPermissions { get; set; } = new();
    public string? ClientInfo { get; set; }
    public string? SelectedMenuItem { get; set; }
    public bool IsSidebarExpanded { get; set; } = true;
    
    public event Action? OnSidebarToggle;
    public int AccessLevel { get; set; } = 0;
    
    // Service Charge and Tax for SwiftServe (Restaurant Management)
    public ServiceChargeInfo ServiceCharge { get; set; } = new ServiceChargeInfo();
    public double GST { get; set; } = 0;
    public string _serviceType { get; set; } = "Dine-In";
    public RestaurantTablesModel? _selectedTable { get; set; }

    public Globals()
    {
        try
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true);
            _ = builder.Build();

            IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

            BaseURI = configuration.GetValue<string>("ApiUrl:BaseUrl") ?? string.Empty;
        }
        catch
        {
            // If appsettings.json doesn't exist or can't be read, just use empty BaseURI
            // It will be set from Preferences in App.xaml.cs or by user in LoginPage
            BaseURI = string.Empty;
        }
    }


    public void NotifySidebarToggle()
    {
        OnSidebarToggle?.Invoke();
    }
    public  string Encrypt(string text)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = new byte[16];

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        cryptoStream.Write(textBytes, 0, textBytes.Length);
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public  string Decrypt(string cipherText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = new byte[16];

        using MemoryStream memoryStream = new(cipherBytes);
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        byte[] textBytes = new byte[cipherBytes.Length];
        int bytesRead = cryptoStream.Read(textBytes, 0, textBytes.Length);
        return Encoding.UTF8.GetString(textBytes, 0, bytesRead);
    }

    public  int PageAccess(NavigationManager? navManager, string pageName)
    {
        try
        {
            // 🧭 Validate navigation manager and input
            if (navManager == null || string.IsNullOrWhiteSpace(pageName))
            {
                // Avoid throwing NavigationException if navManager is null
                Console.WriteLine("⚠️ NavigationManager is null or pageName is empty — redirect skipped.");
                return -1;
            }

            // 🧑‍💼 Validate user
            if (User == null)
            {
                navManager.NavigateTo("/", true);
                return -1;
            }

            // 👑 Admin (GroupId = 1) always has full access - check BEFORE permissions validation
            if (User.GroupId == 1)
                return 1;

            // 🗂 Validate permissions for non-admin users
            if (MyPermissions == null || !MyPermissions.Any())
            {
                // For non-admin users, if permissions aren't loaded yet, deny access
                // This prevents unauthorized access while permissions are loading
                Console.WriteLine("⚠️ Permissions not loaded yet for non-admin user.");
                navManager.NavigateTo("/", true);
                return -1;
            }

            string normalizedPage = pageName.Trim().ToLower();

            // 🔎 Find matching permission
            var menuPermission = MyPermissions.FirstOrDefault(p =>
                p.PageName?.Trim().ToLower() == normalizedPage &&
                p.GroupId == User.GroupId &&
                p.IsActive == 1);

            if (menuPermission == null)
            {
                navManager.NavigateTo("/", true);
                return -1;
            }

            // ✅ Return based on privilege
            switch (menuPermission.My_Privilege?.Trim().ToLower())
            {
                case "full access":
                    return 1;
                case "read only":
                    return 0;
                default:
                    navManager.NavigateTo("/", true);
                    return -1;
            }
        }
        catch (Exception ex)
        {
            // 🛑 Catch any unexpected issues safely
            Console.WriteLine($"⚠️ PageAccess error: {ex.Message}");
            // If user is admin and there's an error, still allow access
            if (User != null && User.GroupId == 1)
                return 1;
            return -1;
        }
    }

    public  List<int> GetAllSubDepartmentIds(List<DepartmentsModel> allDepartments, int parentId)
    {
        var result = new List<int> { parentId };

        var childDepartments = allDepartments
            .Where(x => x.ParentId == parentId)
            .ToList();

        foreach (var child in childDepartments)
        {
            result.AddRange(GetAllSubDepartmentIds(allDepartments, child.Id));
        }

        return result;
    }

    public InvoicesModel InvoiceGridTotals(InvoicesModel Invoice)
    {
        //Invoice Detail Total
        double TotalInvoiceAmount = 0;
        foreach (var i in Invoice.InvoiceDetails)
        {
            TotalInvoiceAmount += (i.Qty * i.UnitPrice) + i.TaxAmount - i.DiscountAmount;
        }

        //Payments Total
        decimal TotalPaymentsAmount = 0;
        foreach (var i in Invoice.InvoicePayments)
        {
            TotalPaymentsAmount += i.Amount;
        }

        return Invoice;
    }

    /// <summary>
    /// Gets the full image URL from a filename. Handles both filename-only and full URL formats for backward compatibility.
    /// </summary>
    /// <param name="pic">The picture value from database (can be filename or full URL)</param>
    /// <returns>Full URL to the image, or empty string if pic is null/empty</returns>
    public string GetImageUrl(string? pic)
    {
        if (string.IsNullOrWhiteSpace(pic))
            return string.Empty;

        // If it's already a full URL (contains http:// or https://), return as is for backward compatibility
        if (pic.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            pic.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return pic;
        }

        // If it's already a relative path starting with /, return as is
        if (pic.StartsWith("/"))
        {
            return pic;
        }

        // Otherwise, construct the full URL from BaseURI
        // BaseURI format is typically "https://api.example.com/api/" or "https://api.example.com/"
        if (string.IsNullOrWhiteSpace(BaseURI))
            return $"/files/{pic}"; // Fallback to relative path if BaseURI not set

        string baseUrl = BaseURI.TrimEnd('/');
        // Remove /api if present to get the base domain
        if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 4);
        }

        // Construct URL: baseUrl/files/filename
        return $"{baseUrl}/files/{pic}";
    }
}
