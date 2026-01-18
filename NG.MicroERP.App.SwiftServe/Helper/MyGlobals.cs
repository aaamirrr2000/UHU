
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

namespace NG.MicroERP.App.SwiftServe.Helper;

public static class MyGlobals
{
    public static string BaseURI { get; set; } = "";
    public static string key = "aT4hmLHkxfeXaT4h";

    // Authentication and User Data
    public static string Token { get; set; } = string.Empty;
    public static OrganizationsModel Organization { get; set; } = new();
    public static UsersModel User { get; set; } = new();
    
    // UI State
    public static string PageTitle { get; set; } = "";
    public static bool _isDarkMode;
    public static bool IsSidebarExpanded { get; set; } = true;
    public static event Action? OnSidebarToggle;
    
    // Permissions
    public static List<GroupMenuModel> MyPermissions { get; set; } = new();
    
    // Restaurant Management (SwiftServe specific)
    public static ServiceChargeInfo ServiceCharge { get; set; } = new ServiceChargeInfo();
    public static double GST { get; set; } = 0;
    public static string _serviceType { get; set; } = "Dine-In";
    public static RestaurantTablesModel? _selectedTable { get; set; }

    static MyGlobals()
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


    public static void NotifySidebarToggle()
    {
        OnSidebarToggle?.Invoke();
    }
    public static string Encrypt(string text)
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

    public static string Decrypt(string cipherText)
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

    public static int PageAccess(NavigationManager? navManager, string pageName)
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

    public static List<int> GetAllSubDepartmentIds(List<DepartmentsModel> allDepartments, int parentId)
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

    public static InvoicesModel InvoiceGridTotals(InvoicesModel Invoice)
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
}
