using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.WorkHub.Helper;

public static class WorkHubFunctions
{
    public static async Task<T?> GetAsync<T>(string url, bool useTokenAuthorize = false)
    {
        try
        {
            string uri = $"{WorkHubGlobals.BaseURI}{url}";

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (useTokenAuthorize && !string.IsNullOrEmpty(WorkHubGlobals.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", WorkHubGlobals.Token);
            }

            HttpResponseMessage response = await httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string resultMessage = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(resultMessage) || resultMessage == "null")
                {
                    if (typeof(T).IsClass && Activator.CreateInstance(typeof(T)) is T emptyObj)
                        return emptyObj;
                    return default;
                }

                T? result = JsonConvert.DeserializeObject<T>(resultMessage);
                return result;
            }
            else
            {
                return default;
            }
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    public static async Task<(bool Success, T? Result, string Message)> PostAsync<T>(string url, object? data = null, bool useTokenAuthorize = false)
    {
        try
        {
            string uri = $"{WorkHubGlobals.BaseURI}{url}";

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (useTokenAuthorize && !string.IsNullOrEmpty(WorkHubGlobals.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", WorkHubGlobals.Token);
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
                string errorMessage = "Request failed";
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (errorObj != null)
                        {
                            if (errorObj.message != null)
                                errorMessage = errorObj.message.ToString();
                            else if (errorObj.Message != null)
                                errorMessage = errorObj.Message.ToString();
                            else if (errorObj.error != null)
                                errorMessage = errorObj.error.ToString();
                        }
                    }
                    catch
                    {
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
}
