using Azure;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;

using MudBlazor;

using Newtonsoft.Json;

using NG.MicroERP.Shared.Models;

using Serilog;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace NG.MicroERP.Shared.Helper;

public class Functions
{
    public static async Task<T?> GetAsync<T>(string url, bool useTokenAuthorize = false)
    {
        try
        {
            string uri = $"{Globals.BaseURI}{url}";

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (useTokenAuthorize && !string.IsNullOrEmpty(Globals.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Globals.Token);
            }

            HttpResponseMessage response = await httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string resultMessage = await response.Content.ReadAsStringAsync();

                // ✅ If empty or "null", treat as success (return an empty T instance)
                if (string.IsNullOrWhiteSpace(resultMessage) || resultMessage == "null")
                {
                    // Try creating an empty instance if T is a class
                    if (typeof(T).IsClass && Activator.CreateInstance(typeof(T)) is T emptyObj)
                        return emptyObj;

                    return default;
                }

                T? result = JsonConvert.DeserializeObject<T>(resultMessage);
                return result;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();

                // ✅ For error response, still handle safely
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return default;

                T? errorResponse = JsonConvert.DeserializeObject<T>(content);
                return errorResponse;
            }
        }
        catch (Exception ex)
        {
            _ = ex.Message;
            return default;
        }
    }


    public static async Task<(bool Success, T? Result, string Message)> PostAsync<T>(string url, object? data = null, bool useTokenAuthorize = false)
    {
        try
        {
            string uri = $"{Globals.BaseURI}{url}";

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (useTokenAuthorize && !string.IsNullOrEmpty(Globals.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.Token);
            }

            string jsonData = data != null ? JsonConvert.SerializeObject(data) : string.Empty;
            StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(responseContent) || responseContent == "null")
                {
                    return (true, default, "Success");
                }

                if (typeof(T) == typeof(string))
                {
                    object plainText = responseContent;
                    return (true, (T)plainText, "Success");
                }
                else
                {
                    try
                    {
                        T? result = JsonConvert.DeserializeObject<T>(responseContent);
                        return (true, result, "Success");
                    }
                    catch (JsonException ex)
                    {
                        return (true, default, $"Deserialization error: {ex.Message}");
                    }
                }
            }
            else
            {
                return (false, default, $"HTTP Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            return (false, default, $"Exception: {ex.Message}");
        }
    }


    public static async Task<(int?, string?)> DeleteAsync(string url, bool Authorized = true)
    {
        try
        {
            using HttpClient httpClient = new();
            httpClient.Timeout = TimeSpan.FromMinutes(30);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.BaseAddress = new Uri(Globals.BaseURI);

            if (Authorized == true)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.Token);
            }

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage responseMessage = await httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                string resultMessage = responseMessage.Content.ReadAsStringAsync().Result;
                return (1, resultMessage);
            }
            else
            {
                string content = await responseMessage.Content.ReadAsStringAsync();
                string? response = JsonConvert.DeserializeObject<string>(content);
                return (0, response);
            }
        }
        catch (Exception ex)
        {
            return (-1, ex.Message);
        }
    }

    public static async Task<bool> ShowConfirmation(
        IDialogService dialogService,
        string message,
        string icon = "🔔",
        string color = "#FF9800",
        string titleText = "Confirmation",
        string confirmLabel = "Confirm",
        string cancelLabel = "Cancel")
    {
        string html = $@"
            <style>
              .cg-card {{
                width: 100%;
                max-width: 420px;
                margin: 0 auto;
                background: #ffffff;
                border-radius: 12px;
                box-shadow: none; /* removed shadow */
                overflow: hidden;
                font-family: 'Roboto', Arial, sans-serif;
                color: #263238;
              }}
              .cg-body {{
                padding: 22px 20px;
                text-align: center;
              }}
              .cg-icon {{
                width: 72px;
                height: 72px;
                margin: 0 auto 12px auto;
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 34px;
                background: linear-gradient(135deg, {color}33, {color}55);
              }}
              .cg-title {{
                font-size: 20px;
                font-weight: 600;
                margin-bottom: 8px;
                color: {color};
              }}
              .cg-message {{
                font-size: 15px;
                line-height: 1.45;
                margin-bottom: 6px;
                color: #455A64;
                padding: 0 6px;
                white-space: pre-wrap;
              }}

              @media (max-width: 420px) {{
                .cg-card {{ border-radius: 10px; }}
                .cg-icon {{ width:64px; height:64px; font-size:30px; }}
              }}
            </style>

            <div class='cg-card' role='dialog' aria-labelledby='cg-title' aria-describedby='cg-message'>
              <div class='cg-body'>
                <div class='cg-icon' aria-hidden='true'>{icon}</div>
                <div id='cg-title' class='cg-title'>{titleText}</div>
                <div id='cg-message' class='cg-message'>{System.Net.WebUtility.HtmlEncode(message)}</div>
              </div>
            </div>
            ";

        bool? result = await dialogService.ShowMessageBox(
            title: "", // keep host title blank so our HTML stays central
            markupMessage: (MarkupString)html,
            yesText: $"✅ {confirmLabel}",
            cancelText: $"✖️ {cancelLabel}"
        );

        return result ?? false;
    }

    public static async Task ShowMessage(IDialogService dialogService, string type, string message)
    {
        string icon = type.ToLower() switch
        {
            "error" => "❌",
            "success" => "✅",
            "warning" => "⚠️",
            "info" => "ℹ️",
            _ => "🔔"
        };

        string titleText = type.ToLower() switch
        {
            "error" => "Error",
            "success" => "Success",
            "warning" => "Warning",
            "info" => "Information",
            _ => "Message"
        };

        string color = type.ToLower() switch
        {
            "error" => "#e74c3c",
            "success" => "#27ae60",
            "warning" => "#f39c12",
            "info" => "#2980b9",
            _ => "#2c3e50"
        };

        string html = $@"
                        <style>
                         .cg-container {{
                                width: 600px;
                                height: 320px;
                                display: flex;
                                align-items: center;
                                justify-content: center;
                                margin: auto;
                            }}
                          .cg-card {{
                            width: 100%;
                            max-width: 420px;
                            margin: 0 auto;
                            background: #ffffff;
                            border-radius: 12px;
                            box-shadow: none;
                            overflow: hidden;
                            font-family: 'Roboto', Arial, sans-serif;
                            color: #263238;
                          }}
                          .cg-body {{
                            padding: 22px 20px;
                            text-align: center;
                          }}
                          .cg-icon {{
                            width: 72px;
                            height: 72px;
                            margin: 0 auto 12px auto;
                            border-radius: 50%;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            font-size: 34px;
                            background: linear-gradient(135deg, {color}33, {color}55);
                          }}
                          .cg-title {{
                            font-size: 20px;
                            font-weight: 600;
                            margin-bottom: 8px;
                            color: {color};
                          }}
                          .cg-message {{
                            font-size: 15px;
                            line-height: 1.45;
                            color: #455A64;
                            padding: 0 6px;
                            white-space: pre-wrap;
                          }}

                          @media (max-width: 420px) {{
                            .cg-card {{ border-radius: 10px; }}
                            .cg-icon {{ width:64px; height:64px; font-size:30px; }}
                          }}
                        </style>

                        <div class='cg-card' role='dialog' aria-labelledby='cg-title' aria-describedby='cg-message'>
                          <div class='cg-body'>
                            <div class='cg-icon' aria-hidden='true'>{icon}</div>
                            <div id='cg-title' class='cg-title'>{titleText}</div>
                            <div id='cg-message' class='cg-message'>{System.Net.WebUtility.HtmlEncode(message)}</div>
                          </div>
                        </div>
                        ";

        await dialogService.ShowMessageBox(
            title: "",
            markupMessage: (MarkupString)html,
            yesText: "OK"
        );
    }


    public static async Task<(bool, string)> FileUpload(InputFileChangeEventArgs fileuploadevent, FileUploadService fs, int FileSizeinMB = 2)
    {
        try
        {
            var file = fileuploadevent.File;
            if (file is not null)
            {
                var result = await fs.UploadFileAsync(file, true, FileSizeinMB);
                if (result.Item1 == true)
                    return (true, result.Item2);
                else
                    return (false, result.Item2);
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }

        return (false, "No file selected");
    }

    public static string GetComputerDetails()
    {

        string ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList
            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "Unknown";

        string pcName = Environment.MachineName;

        string macAddress = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault() ?? "Unknown";

        return $"IP: {ipAddress}, MAC: {macAddress}, Device: {pcName}";

    }


    [Obsolete]
    public static async Task<(bool, string)> UploadAsync(InputFileChangeEventArgs e, Microsoft.AspNetCore.Hosting.IHostingEnvironment webHostEnvironment, string folder = "Images")
    {
        try
        {
            const long MaxFileSizeInBytes = 512 * 1024;

            IBrowserFile file = e.File;
            long fileSizeInBytes = file.Size;

            if (fileSizeInBytes > MaxFileSizeInBytes)
            {
                string readableFileSize = GetReadableFileSize(fileSizeInBytes);
                string err = $"File size ({readableFileSize}) exceeds the maximum allowed size of {GetReadableFileSize(MaxFileSizeInBytes)}.";
                return (false, err);
            }

            string fileName = file.Name;
            string wwwrootPath = webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(wwwrootPath, folder);

            if (!Directory.Exists(folderPath))
            {
                _ = Directory.CreateDirectory(folderPath);
            }

            string newFileName = GenerateNewFileName(fileName);
            string filePath = Path.Combine(folderPath, newFileName);

            using (FileStream fileStream = new(filePath, FileMode.Create))
            {
                await file.OpenReadStream().CopyToAsync(fileStream);
            }

            return (true, $"{folder}/{newFileName}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static string GetReadableFileSize(long fileSizeInBytes)
    {
        return fileSizeInBytes < 1024
            ? $"{fileSizeInBytes} bytes"
            : fileSizeInBytes < 1048576 ? $"{fileSizeInBytes / 1024} KB" : $"{fileSizeInBytes / 1048576} MB";
    }

    public static string GenerateNewFileName(string fileName)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string extension = Path.GetExtension(fileName);
        return $"{timestamp}_{Guid.NewGuid():N}{extension}";
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


    public static string NumberToWords(int number)
    {
        string[] UnitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                                                  "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen",
                                                  "Eighteen", "Nineteen" };

        string[] TensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        if (number == 0)
        {
            return "Zero";
        }

        if (number < 0)
        {
            return "Minus " + NumberToWords(Math.Abs(number));
        }

        string words = "";

        if ((number / 1000000000) > 0)
        {
            words += NumberToWords(number / 1000000000) + " Billion ";
            number %= 1000000000;
        }

        if ((number / 1000000) > 0)
        {
            words += NumberToWords(number / 1000000) + " Million ";
            number %= 1000000;
        }

        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " Thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " Hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            if (words != "")
            {
                words += "and ";
            }

            if (number < 20)
            {
                words += UnitsMap[number];
            }
            else
            {
                words += TensMap[number / 10];
                if ((number % 10) > 0)
                {
                    words += "-" + UnitsMap[number % 10];
                }
            }
        }

        return words.Trim();
    }

    public static string ConvertMoneyToWords(decimal amount)
    {
        int dollars = (int)amount;
        int cents = (int)((amount - dollars) * 100);

        string dollarPart = NumberToWords(dollars) + (dollars == 1 ? " Rupee" : " Ruppes");
        string centPart = cents > 0 ? " and " + NumberToWords(cents) + (cents == 1 ? " Paisa" : " Paisa") : "";

        return dollarPart + centPart + " Only";
    }

    public static List<string> GetPageNamesInFolder(string folderNamespace)
    {
        List<string> pages = [];
        // Get all types in the current assembly
        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] types = assembly.GetTypes();

        foreach (Type type in types)
        {
            // Check if the type is a Blazor component
            if (type.IsSubclassOf(typeof(ComponentBase)))
            {
                // Check for the RouteAttribute
                object[] routeAttributes = type.GetCustomAttributes(typeof(RouteAttribute), true);
                if (routeAttributes.Any())
                {
                    // Filter by folder namespace
                    if (type.Namespace != null && type.Namespace.StartsWith(folderNamespace))
                    {
                        foreach (RouteAttribute route in routeAttributes)
                        {
                            pages.Add(route.Template);
                        }
                    }
                }
            }
        }
        return pages;
    }

    public static void NavigateToEncryptedPage(NavigationManager navManager, string pageName)
    {
        if (navManager == null)
        {
            throw new ArgumentNullException(nameof(navManager));
        }

        string encryptedPage = Globals.Encrypt(pageName);
        if (string.IsNullOrWhiteSpace(encryptedPage))
        {
            throw new InvalidOperationException("Encryption failed");
        }

        string url = $"securepage/{encryptedPage}";
        navManager.NavigateTo(url, forceLoad: true);
    }

    /*
    public static List<string> GetDashboardPages()
    {
        try
        {
            // Load the shared project assembly where the dashboards live
            var assembly = Assembly.Load("NG.MicroERP.Shared");

            // Find all Razor component classes in namespace "Pages.Dashboards"
            var pageTypes = assembly.GetTypes()
                .Where(t =>
                    t.IsSubclassOf(typeof(ComponentBase)) &&
                    t.Namespace != null &&
                    (t.Namespace.Contains("Pages.Dashboards", StringComparison.OrdinalIgnoreCase) ||
                     t.Namespace.EndsWith(".Dashboards", StringComparison.OrdinalIgnoreCase)))
                .Select(t => t.Name.Replace("Page", "").Replace("Component", ""))
                .OrderBy(name => name)
                .ToList();

            return pageTypes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting dashboard pages: {ex.Message}");
            return new List<string>();
        }
    }
    */

    public static List<string> GetFilesFromFolder()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
        _ = builder.Build();

        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        var folderPath = configuration.GetValue<string>("DashboardPages:DashboardPagesPath") ?? string.Empty;

        var pageList = new List<string>();

        if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
        {
            var files = Directory.GetFiles(folderPath, "*.razor", SearchOption.AllDirectories);

            pageList = files
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .OrderBy(name => name)
                .ToList();
        }

        return pageList;
    }

}
