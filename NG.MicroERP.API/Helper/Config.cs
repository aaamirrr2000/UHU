using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using MudBlazor;
using Newtonsoft.Json;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;


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
        MailMessage message = null;
        Attachment emailAttachment = null;
        SmtpClient smtp = null;

        try
        {
            Log.Information("=== Starting email send process ===");

            message = new MailMessage();
            smtp = new SmtpClient();

            message.From = new MailAddress("mail@nexgentechstudios.com", "Cybercom Support");
            message.To.Add(new MailAddress(toEmail));
            message.Bcc.Add(new MailAddress("aamir.rashid.1973@gmail.com"));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = content;

            if (!string.IsNullOrEmpty(attachment))
            {
                if (File.Exists(attachment))
                {
                    FileInfo fileInfo = new FileInfo(attachment);
                    long fileSize = fileInfo.Length;
                    Log.Information($"Attachment found: {attachment}, Size: {fileSize} bytes ({fileSize / 1024}KB)");

                    emailAttachment = new Attachment(attachment);
                    message.Attachments.Add(emailAttachment);
                    Log.Information("Attachment successfully added to message");
                }
            }

            smtp.Port = 587;
            smtp.Host = "mail.nexgentechstudios.com";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("mail@nexgentechstudios.com", "Solution_00");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Timeout = 120000;

            Log.Information("Attempting to send email...");
            smtp.Send(message);
            Log.Information("=== Email sent successfully! ===");

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Email failed: {ex.Message}");
            return false;
        }
        finally
        {
            emailAttachment?.Dispose();
            message?.Dispose();
            smtp?.Dispose();
            Log.Information("Email resources disposed");
        }
    }

    public static async Task<bool> UploadToFtpAsync(string ftpUrl, string username, string password, string localFilePath)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                Log.Error($"File not found: {localFilePath}");
                return false;
            }

            string filename = Path.GetFileName(localFilePath);
            string fullUrl = $"{ftpUrl.TrimEnd('/')}/{filename}";

            Log.Information($"Uploading to FTP: {fullUrl}");

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = false;
            request.Timeout = 60000;

            byte[] fileBytes = await File.ReadAllBytesAsync(localFilePath);

            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            }

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                Log.Information($"FTP Upload Success: {response.StatusDescription}");
                return true;
            }
        }
        catch (WebException ex)
        {
            string ftpError = (ex.Response is FtpWebResponse ftpResponse)
                ? ftpResponse.StatusDescription
                : "No FTP response";

            Log.Error($"FTP Upload Failed: {ex.Message} | FTP: {ftpError}");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"FTP Error: {ex.Message}");
            return false;
        }
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
}
