using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Management;


namespace NG.MicroERP.POS.Helper;

public class Functions
{

    public static readonly string ConnectionError = "Cannot connect to database, please check connection ...";
    public static readonly string[] AllowablePC = new[] { "BFEBFBFF000406E3PFLMQ028J9U3N6", "BFEBFBFF000306A9PCWHK001X3ZQF8", "BFEBFBFF000406E3PF0KA3B8" };
    public static readonly string EncryptionKey = "VIVIDSOLUTIONS1092";
    public static PrivateFontCollection private_fonts = new PrivateFontCollection();

    #region UI Utilities

    public static void Print(string PrinterName)
    {
        try
        {
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings = new PrinterSettings { PrinterName = PrinterName, Copies = 1 };
            pd.Print();
        }
        catch { }
    }

    public static void ColorChangeWhenEnter(TextBox controlField)
    {
        controlField.BackColor = Color.FromArgb(237, 210, 191);
        controlField.ForeColor = Color.Black;
        controlField.SelectionStart = 0;
        controlField.SelectionLength = controlField.Text.Length;
    }

    public static void ColorChangeWhenLeave(TextBox controlField)
    {
        controlField.BackColor = Color.White;
        controlField.ForeColor = Color.Black;
    }

    public static void FormInMiddle(ref Form form)
    {
        int X = (Screen.PrimaryScreen.Bounds.Width - form.Width) / 2;
        int Y = (Screen.PrimaryScreen.Bounds.Height - form.Height) / 2;
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(X, Y);
    }

    public static void PanelInMiddle(ref Panel panel)
    {
        int X = (Screen.PrimaryScreen.Bounds.Width - panel.Width) / 2;
        int Y = (Screen.PrimaryScreen.Bounds.Height - panel.Height) / 2;
        panel.Anchor = AnchorStyles.None;
        panel.Location = new Point(X, Y);
    }

    public static void TransparentForm(ref Form form)
    {
        form.FormBorderStyle = FormBorderStyle.None;
        FormInMiddle(ref form);
        form.TransparencyKey = Color.Lime;
        form.BackColor = Color.Lime;
    }

    public static int[] ScreenResolution()
    {
        return new[] { Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height };
    }

    #endregion

    #region Conversion / Helpers

    public static string OpenFileDialog(ref OpenFileDialog dialogBox)
    {
        dialogBox.InitialDirectory = Globals.DefaultFilePath;
        dialogBox.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        dialogBox.FilterIndex = 2;
        dialogBox.RestoreDirectory = true;

        if (dialogBox.ShowDialog() == DialogResult.OK)
        {
            Globals.DefaultFilePath = dialogBox.FileName;
            return dialogBox.FileName;
        }
        return string.Empty;
    }

    public static string TextBox2String(ref TextBox textbox)
    {
        return textbox.Text.ToUpper().Trim();
    }

    public static string ComboBox2String(ref ComboBox combobox)
    {
        return combobox.Text.ToUpper().Trim();
    }

    public static byte[] PictureBox2ByteArray(ref PictureBox pic)
    {
        using var ms = new MemoryStream();
        pic.Image.Save(ms, pic.Image.RawFormat);
        return ms.ToArray();
    }

    #endregion

    #region Security & Identification

    public static string GetMacAddress()
    {
        string cpuID = string.Empty;
        var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
        foreach (ManagementObject mo in mc.GetInstances())
        {
            if (cpuID == string.Empty && Convert.ToBoolean(mo["IPEnabled"]))
                cpuID = mo["MacAddress"].ToString();
        }
        return cpuID;
    }

    public static string GetCPU_ID()
    {
        var mc = new ManagementClass("Win32_Processor");
        foreach (ManagementObject mo in mc.GetInstances())
        {
            return mo["processorID"].ToString();
        }
        return string.Empty;
    }

    public static string GetMotherboard()
    {
        foreach (var mo in new ManagementObjectSearcher("Select * From Win32_BaseBoard").Get())
        {
            return mo["SerialNumber"].ToString().ToUpper();
        }
        return string.Empty;
    }

    public static string GetPCInfo(string property)
    {
        foreach (ManagementObject mo in new ManagementObjectSearcher(new SelectQuery("Select * from Win32_ComputerSystem")).Get())
        {
            return mo[property]?.ToString()?.ToUpper() ?? string.Empty;
        }
        return string.Empty;
    }

    public static string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        throw new Exception("No IPv4 address found.");
    }

    public static string GetComputerID()
    {
        return (GetCPU_ID() + GetMotherboard()).Replace(":", "");
    }

    public static void GetCOMPorts(ComboBox cmb)
    {
        foreach (ManagementObject obj in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_POTSModem").Get())
        {
            cmb.Items.Add(obj["AttachedTo"].ToString());
        }
        cmb.SelectedIndex = 0;
    }

    public static void ListOfCOMPorts(ComboBox cmb)
    {
        foreach (string port in SerialPort.GetPortNames())
        {
            cmb.Items.Add(port);
        }
        cmb.SelectedIndex = 0;
    }

    public static void ListOfPrinters(ComboBox cmb)
    {
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            cmb.Items.Add(printer);
        }
        cmb.SelectedIndex = 0;
    }

    #endregion

    #region Validation & Utility

    public static string GenerateRandomPassword(int length = 8)
    {
        const string allChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890";
        const string firstChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        VBMath.Randomize();
        var password = Strings.Mid(firstChars, (int)(firstChars.Length * VBMath.Rnd()) + 1, 1);

        for (int i = 1; i < length; i++)
        {
            password += Strings.Mid(allChars, (int)(allChars.Length * VBMath.Rnd()) + 1, 1);
        }
        return password;
    }

    public static bool IsEmail(string s)
    {
        return Regex.IsMatch(s, "^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$");
    }

    public static bool IsNumeric(char c)
    {
        return char.IsDigit(c) || char.IsControl(c) || c == 8 || c == 13 || c == 37 || c == 39 || c == 46 || c == 109;
    }

    public static string CurrentTime()
    {
        return Strings.Format(DateTime.Now, "dddd MMMM, dd yyyy HH:mm:ss tt");
    }

    #endregion

    #region Encryption & Decryption

    public static string Encrypt(string clearText)
    {
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[]
            {
                    0x49, 0x76, 0x61, 0x6E,
                    0x20, 0x4D, 0x65, 0x64,
                    0x76, 0x65, 0x64, 0x65,
                    0x76
            });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(clearBytes, 0, clearBytes.Length);
                cs.Close();
            }
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public static string Decrypt(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[]
            {
                    0x49, 0x76, 0x61, 0x6E,
                    0x20, 0x4D, 0x65, 0x64,
                    0x76, 0x65, 0x64, 0x65,
                    0x76
            });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 0, cipherBytes.Length);
                cs.Close();
            }
            return Encoding.Unicode.GetString(ms.ToArray());
        }
    }

    public static void EncryptFile(string input, string output)
    {
        using FileStream fs = File.Create(output);
        TripleDESCryptoServiceProvider tc = new TripleDESCryptoServiceProvider();
        using CryptoStream cs = new CryptoStream(fs, tc.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamReader sr = new StreamReader(input);
        using StreamWriter sw = new StreamWriter(cs);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            sw.Write(line);
        }
        using FileStream fsKey = File.Create(output + ".key");
        using BinaryWriter bw = new BinaryWriter(fsKey);
        bw.Write(tc.Key);
        bw.Write(tc.IV);
        MessageBox.Show("File Encrypted Successfully");
    }

    public static void DecryptFile(string input, string output)
    {
        using FileStream fsFile = File.OpenRead(input);
        using FileStream fsKey = File.OpenRead(input + ".key");
        using FileStream fsSave = File.Create(output);
        TripleDESCryptoServiceProvider csAlgo = new TripleDESCryptoServiceProvider();
        using BinaryReader br = new BinaryReader(fsKey);
        csAlgo.Key = br.ReadBytes(24);
        csAlgo.IV = br.ReadBytes(8);
        using CryptoStream cs = new CryptoStream(fsFile, csAlgo.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader srClean = new StreamReader(cs);
        using StreamWriter swClean = new StreamWriter(fsSave);
        swClean.Write(srClean.ReadToEnd());
        MessageBox.Show("File Decrypted Successfully");
    }

    #endregion

    #region Miscellaneous

    public static string NumberToWords(int number)
    {
        if (number == 0) return "zero";
        if (number < 0) return "minus " + NumberToWords(Math.Abs(number));

        string words = "";
        if ((number / 1000000) > 0)
        {
            words += NumberToWords(number / 1000000) + " million ";
            number %= 1000000;
        }
        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " thousand ";
            number %= 1000;
        }
        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " hundred ";
            number %= 100;
        }
        if (number > 0)
        {
            if (!string.IsNullOrEmpty(words))
                words += "and ";

            var unitsMap = new[]
            {
                    "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
                    "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
                };
            var tensMap = new[]
            {
                    "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
                };
            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }
        return words;
    }

    public static Tuple<int, string, string> TupleFunctionSample()
    {
        return Tuple.Create(1001, "Rudy", "Koertson");
    }

    [Obsolete("This method is deprecated. You could use XYZ alternatively.", true)]
    public static string NullString(string s)
    {
        return string.IsNullOrEmpty(s) ?
            "This string is null or empty." :
            $"This string is not null or empty, it equals to \"{s}\" ";
    }

    public static void StringBuilderExample(string name, string age)
    {
        var builder = new StringBuilder();
        builder.Append("Name: ").Append(name).Append(", Age: ").Append(age);
        string result = builder.ToString();
    }

    public static void SendError(int errCode, string errDesc, string comments)
    {
        string emailBody =
            $"Error Code: {errCode}\r\nDescription: {errDesc}\r\nMachine ID: {Globals.MachineID}\r\nLocation ID: {Globals.LocationID}\r\nLocation Name: {Globals.LocationName}";
        // Email(...)
    }

    #endregion
}
