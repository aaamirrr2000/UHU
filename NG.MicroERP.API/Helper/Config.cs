using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MudBlazor;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using NG.MicroERP.API.Services;


using Serilog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Helper;


namespace NG.MicroERP.API.Helper;

public class Config
{
    public static string BaseURI { get; set; } = "";
    public static string key = "aT4hmLHkxfeXaT4h";

    public string DefaultConnectionString { get; private set; } = "";
    public string GlobalConnectionString { get; private set; } = "";

    public Config()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
        _ = builder.Build();

        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        DefaultConnectionString = configuration.GetValue<string>("ConnectionStrings:Default") ?? string.Empty;
        GlobalConnectionString = configuration.GetValue<string>("ConnectionStrings:Global") ?? string.Empty;
    }

    public string DefaultDB() => DefaultConnectionString;
    public string GlobalDB() => GlobalConnectionString;

    public static bool IsSafeSearchCriteria(string criteria)
    {
        if (string.IsNullOrEmpty(criteria))
            return true;

        if (criteria.Length > 500)
            return false;

        var upperCriteria = criteria.ToUpperInvariant();

        // Only block the most dangerous patterns
        var dangerous = new[] { ";", "--", "/*", "*/", "DROP ", "DELETE ", "UPDATE ", "INSERT ", "EXEC " };

        foreach (var danger in dangerous)
        {
            if (upperCriteria.Contains(danger))
                return false;
        }

        return true;
    }

    public static string GenerateRandomPassword(int iNumChars = 8)
    {
        string strDefault = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890";
        string strFirstChar = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int iCount;
        string strReturn;
        int iNumber;
        int iLength;

        VBMath.Randomize();

        iLength = Strings.Len(strFirstChar);
        iNumber = (int)((iLength * VBMath.Rnd()) + 1);
        strReturn = Strings.Mid(strFirstChar, iNumber, 1);

        iLength = Strings.Len(strDefault);
        for (iCount = 1; iCount <= iNumChars - 1; iCount++)
        {
            iNumber = (int)((iLength * VBMath.Rnd()) + 1);
            strReturn += Strings.Mid(strDefault, iNumber, 1);
        }

        return strReturn;
    }

    public static bool sendEmail(string toEmail, string subject, string content, string attachment = "")
    {
        try
        {
            MailMessage message = new();
            SmtpClient smtp = new()!;

            message.From = new MailAddress("mail@nexgentechstudios.com", "NexGen Technology Studios");
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = content;

            if (attachment != null)
            {
                if (attachment.Length > 0)
                {
                    message.Attachments.Add(new Attachment(attachment));
                }
            }

            smtp.Port = 587;
            smtp.Host = "mail.nexgentechstudios.com";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("mail@nexgentechstudios.com", "Solution_00");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
        }
        catch
        {
            return false;
        }
        return true;
    }


    public static string Base64ToString(string base64String)
    {
        byte[] bytes = Convert.FromBase64String(base64String);
        return Encoding.UTF8.GetString(bytes);
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

    //public static async Task<int> Page_Authorized(string Uri)
    //{

    //    if (Uri.EndsWith("/"))
    //    {
    //        Uri = Uri[..^1];
    //    }

    //    string PageName = Uri.Split('/').LastOrDefault()!;
    //    PermissionsService groupsService = new();

    //    if (Globals.User.GroupId != 1)
    //    {
    //        List<GroupMenuModel> res = await groupsService.SearchGroupMenu(Globals.User.OrganizationId, $@"pagename = '{PageName}' and groupid={Globals.User.GroupId}")!;
    //        if (res != null)
    //        {
    //            if (res.Count > 0)
    //            {
    //                GroupMenuModel result = res.FirstOrDefault()!;
    //                return result.My_Privilege == "VIEW ONLY" ? 1 : result.My_Privilege == "FULL ACCESS" ? 2 : 0;
    //            }
    //        }
    //        return 0;
    //    }
    //    else
    //    {
    //        return 2;
    //    }
    //}

    //public static async Task<(bool isAuthorized, int result)> AllowThisPage(string uri)
    //{
    //    if (Globals.User == null)
    //        return (false, 0);

    //    Log.Information($"User {Globals.User.FullName} tried to access {uri}");
    //    int res = await Page_Authorized(uri);
    //    bool isAuthorized = res != 0 && (Globals.isVisible = res >= 1);
    //    return (isAuthorized, res);
    //}

}
