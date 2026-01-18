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

namespace NG.MicroERP.App.SwiftServe.Helper;

public static class MyFunctions
{
    public static async Task<T?> GetAsync<T>(string url, bool useTokenAuthorize = false)
    {
        try
        {
            string uri = $"{NG.MicroERP.App.SwiftServe.Helper.MyGlobals.BaseURI}{url}";

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (useTokenAuthorize && !string.IsNullOrEmpty(NG.MicroERP.App.SwiftServe.Helper.MyGlobals.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", NG.MicroERP.App.SwiftServe.Helper.MyGlobals.Token);
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
            string uri = $"{NG.MicroERP.App.SwiftServe.Helper.MyGlobals.BaseURI}{url}";

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (useTokenAuthorize && !string.IsNullOrEmpty(NG.MicroERP.App.SwiftServe.Helper.MyGlobals.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", NG.MicroERP.App.SwiftServe.Helper.MyGlobals.Token);
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
                // Try to extract meaningful error message from response
                string errorMessage = "Request failed";

                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        // Try to parse as JSON object (ProblemDetails or custom error format)
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (errorObj != null)
                        {
                            // Check for various error message fields (in order of preference)
                            if (errorObj.message != null)
                                errorMessage = errorObj.message.ToString();
                            else if (errorObj.Message != null)
                                errorMessage = errorObj.Message.ToString();
                            else if (errorObj.error != null)
                                errorMessage = errorObj.error.ToString();
                            else if (errorObj.Error != null)
                                errorMessage = errorObj.Error.ToString();
                            else if (errorObj.title != null)
                                errorMessage = errorObj.title.ToString();
                            else if (errorObj.detail != null)
                                errorMessage = errorObj.detail.ToString();
                            else if (errorObj.Detail != null)
                                errorMessage = errorObj.Detail.ToString();
                        }

                        // If it's a plain JSON string, use it directly
                        if (errorMessage == "Request failed" && responseContent.StartsWith("\"") && responseContent.EndsWith("\""))
                        {
                            errorMessage = JsonConvert.DeserializeObject<string>(responseContent) ?? responseContent;
                        }
                        // If it's not JSON, use the raw content (might be a plain string)
                        else if (errorMessage == "Request failed" && !responseContent.TrimStart().StartsWith("{") && !responseContent.TrimStart().StartsWith("["))
                        {
                            errorMessage = responseContent.Trim('"');
                        }
                    }
                    catch
                    {
                        // If parsing fails, use raw content
                        errorMessage = responseContent.Trim('"');
                    }
                }

                return (false, default, errorMessage);
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
            httpClient.BaseAddress = new Uri(NG.MicroERP.App.SwiftServe.Helper.MyGlobals.BaseURI);

            if (Authorized == true)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", NG.MicroERP.App.SwiftServe.Helper.MyGlobals.Token);
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
}
