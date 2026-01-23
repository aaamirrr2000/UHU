using System.Security.Cryptography;
using System.Text;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.WorkHub.Helper;

public static class WorkHubGlobals
{
    public static string BaseURI { get; set; } = "";
    public static string key = "aT4hmLHkxfeXaT4h";

    // Authentication and User Data
    public static string Token { get; set; } = string.Empty;
    public static OrganizationsModel Organization { get; set; } = new();
    public static UsersModel User { get; set; } = new();
    
    // UI State
    public static string PageTitle { get; set; } = "";

    static WorkHubGlobals()
    {
        try
        {
            string savedUrl = Preferences.Get("BaseURI", "").Trim();
            if (!string.IsNullOrEmpty(savedUrl))
            {
                BaseURI = savedUrl;
            }
            else
            {
                BaseURI = "https://localhost:7019/api/";
            }
        }
        catch
        {
            BaseURI = "https://localhost:7019/api/";
        }
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
