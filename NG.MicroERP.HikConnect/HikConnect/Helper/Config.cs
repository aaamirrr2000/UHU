using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HikConnect.Helper;

public class Config
{

    public static bool sendEmail(string toEmail, string subject, string content, string attachment = "")
    {
        try
        {
            MailMessage message = new();
            SmtpClient smtp = new()!;

            message.From = new MailAddress("mail@nexgentechstudios.com", "Cybercom Support");
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

    public static string GetPCUniqueId()
    {
        try
        {
            string cpuId = GetCpuId();
            string boardId = GetMotherboardId();
            string diskId = GetDiskId();
            string winId = GetWindowsProductId();

            string rawId = $"{cpuId}-{boardId}-{diskId}-{winId}";

            // Hash the result for a clean, consistent ID
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawId));
                return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 20);
            }
        }
        catch
        {
            return "UNKNOWN-PC";
        }
    }

    private static string GetCpuId()
    {
        try
        {
            using (var mc = new ManagementClass("win32_processor"))
            {
                foreach (var mo in mc.GetInstances())
                    return mo["ProcessorId"]?.ToString() ?? "NOCPU";
            }
        }
        catch { }
        return "NOCPU";
    }

    private static string GetMotherboardId()
    {
        try
        {
            using (var mc = new ManagementClass("Win32_BaseBoard"))
            {
                foreach (var mo in mc.GetInstances())
                    return mo["SerialNumber"]?.ToString() ?? "NOBOARD";
            }
        }
        catch { }
        return "NOBOARD";
    }

    private static string GetDiskId()
    {
        try
        {
            using (var mc = new ManagementClass("Win32_PhysicalMedia"))
            {
                foreach (var mo in mc.GetInstances())
                    return mo["SerialNumber"]?.ToString().Trim() ?? "NODISK";
            }
        }
        catch { }
        return "NODISK";
    }

    private static string GetWindowsProductId()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (var obj in searcher.Get())
                    return obj["SerialNumber"]?.ToString() ?? "NOWIN";
            }
        }
        catch { }
        return "NOWIN";
    }
}
