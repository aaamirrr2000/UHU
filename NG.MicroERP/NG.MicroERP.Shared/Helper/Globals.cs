
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

    public static string BaseURI = "";
    public static string key = "aT4hmLHkxfeXaT4h";

    public static string Computer_sr = string.Empty;
    public static string Token { get; set; } = string.Empty;
    public static string ListName = string.Empty;
    public static string Icon = string.Empty;
    public static string Version = "1.0p";
    public static bool _tabsInitialized = false;
    public static bool _isDarkMode;

    public static string PageTitle { get; set; } = "";
    public static OrganizationsModel Organization { get; set; } = null!;
    //public static EmployeesModel Emp { get; set; } = null!;
    public static UsersModel User { get; set; } = null!;
    public static EmployeesModel Employee { get; set; } = null!;
    public static DepartmentsModel Department { get; set; } = null!;
    public static List<MyMenuModel>? menu { get; set; } = null;
    public static bool isVisible { get; set; } = true;
    public static string seletctedMenuItem { get; set; } = string.Empty;
    public static bool MyLeave { get; set; } = true;
    public static List<GroupMenuModel> MyPermissions { get; set; } = null;

    public Globals()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
        _ = builder.Build();

        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        BaseURI = configuration.GetValue<string>("ApiUrl:BaseUrl") ?? string.Empty;
    }

    public static string Encrypt(string text)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = new byte[16]; // Zero IV (not recommended for production, use a random IV instead)

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
            if (Globals.User == null)
            {
                navManager.NavigateTo("/", true);
                return -1;
            }

            // 👑 Admin (GroupId = 1) always has full access
            if (Globals.User.GroupId == 1)
                return 1;

            // 🗂 Validate permissions
            if (Globals.MyPermissions == null || !Globals.MyPermissions.Any())
            {
                navManager.NavigateTo("/", true);
                return -1;
            }

            string normalizedPage = pageName.Trim().ToLower();

            // 🔎 Find matching permission
            var menuPermission = Globals.MyPermissions.FirstOrDefault(p =>
                p.PageName?.Trim().ToLower() == normalizedPage &&
                p.GroupId == Globals.User.GroupId &&
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

}
