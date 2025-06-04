
using NG.MicroERP.Shared.Models;

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NG.MicroERP.Shared.Helper;

public class Globals
{
    public static string BaseURI = "https://localhost:7019/";
    public static string key = "YourSecretKey1234";

    public static string Computer_sr = string.Empty;
    public static string Token { get; set; } = string.Empty;
    public static string ListName = string.Empty;
    public static string Icon = string.Empty;
    public static string Version = "1.0p";
    public static bool _tabsInitialized = false;
    public static bool _isDarkMode;
    public static OrganizationsModel Organization { get; set; } = null!;
    //public static EmployeesModel Emp { get; set; } = null!;
    public static UsersModel User { get; set; } = null!;
    public static List<MyMenuModel>? menu { get; set; } = null;
    public static bool isVisible { get; set; } = true;
    public static string seletctedMenuItem { get; set; } = string.Empty;

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
}
