using HikConnect.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class HikvisionApi
{
    private readonly HttpClient _httpClient;

    public HikvisionApi(string ipAddress, string username, string password)
    {
        var handler = new HttpClientHandler
        {
            Credentials = new NetworkCredential(username, password),
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri($"http://{ipAddress}/") // or https:// if required
        };
    }

    public async Task<string> GetAccessEventsAsync(DateTime startDate, DateTime endDate)
    {
        string endpoint = "ISAPI/AccessControl/AcsEvent?format=json";
        int pageSize = 30;
        string searchId = Guid.NewGuid().ToString();
        int totalRecordsCollected = 0;
        int pageCount = 0;
        int maxPages = 50; // Maximum pages to prevent infinite loops

        List<object> allEvents = new List<object>();

        try
        {
            while (pageCount < maxPages)
            {
                pageCount++;

                var requestBody = new
                {
                    AcsEventCond = new
                    {
                        searchID = searchId,
                        searchResultPosition = totalRecordsCollected,
                        maxResults = pageSize,
                        major = 5,
                        minor = 0,
                        startTime = startDate.ToString("yyyy-MM-ddTHH:mm:ss+05:00"),
                        endTime = endDate.ToString("yyyy-MM-ddTHH:mm:ss+05:00"),
                        picEnable = true,
                        timeReverseOrder = true,
                        isAttendanceInfo = true,
                        hasRecordInfo = true
                    }
                };

                var json = System.Text.Json.JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                Console.WriteLine($"Page {pageCount}: Requesting position {totalRecordsCollected}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add timeout to the request
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.PostAsync(endpoint, content, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"Request timeout at page {pageCount}, position {totalRecordsCollected}");
                    break;
                }

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP error {response.StatusCode} at page {pageCount}");
                    break;
                }

                string result = await response.Content.ReadAsStringAsync();

                // Parse response
                int eventsInThisPage = 0;
                string responseStatus = "OK";
                int totalMatches = 0;

                using (JsonDocument doc = JsonDocument.Parse(result))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("AcsEvent", out JsonElement acsEvent))
                    {
                        // Get total matches if available
                        if (acsEvent.TryGetProperty("totalMatches", out JsonElement totalMatchesElement))
                        {
                            totalMatches = totalMatchesElement.GetInt32();
                            Console.WriteLine($"Total matches: {totalMatches}");
                        }

                        // Get response status
                        if (acsEvent.TryGetProperty("responseStatusStrg", out JsonElement statusElement))
                        {
                            responseStatus = statusElement.GetString();
                            Console.WriteLine($"Response status: {responseStatus}");
                        }

                        // Extract events
                        if (acsEvent.TryGetProperty("InfoList", out JsonElement infoList) &&
                            infoList.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement eventItem in infoList.EnumerateArray())
                            {
                                var eventObj = ConvertJsonElementToObject(eventItem);
                                allEvents.Add(eventObj);
                                eventsInThisPage++;
                            }
                        }
                    }
                }

                Console.WriteLine($"Page {pageCount}: Got {eventsInThisPage} events, Status: {responseStatus}");

                // **SIMPLE AND RELIABLE TERMINATION LOGIC**

                // 1. If no events in this page, stop
                if (eventsInThisPage == 0)
                {
                    Console.WriteLine("No events in this page, stopping pagination");
                    break;
                }

                // 2. If we have totalMatches and we've collected enough, stop
                if (totalMatches > 0 && allEvents.Count >= totalMatches)
                {
                    Console.WriteLine($"Collected {allEvents.Count} out of {totalMatches} total matches, stopping");
                    break;
                }

                // 3. If response status is not "MORE", stop
                if (responseStatus != "MORE")
                {
                    Console.WriteLine($"Response status is '{responseStatus}', stopping pagination");
                    break;
                }

                // 4. Update position for next request
                totalRecordsCollected += eventsInThisPage;

                // Small delay between requests
                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during pagination: {ex.Message}");
        }

        Console.WriteLine($"Completed: {allEvents.Count} events from {pageCount} pages");

        // Build final JSON response
        var finalResult = new
        {
            AcsEvent = new
            {
                searchID = searchId,
                totalMatches = allEvents.Count,
                responseStatusStrg = "OK",
                numOfMatches = allEvents.Count,
                InfoList = allEvents
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(finalResult, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });
    }

    private object ConvertJsonElementToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    dict[property.Name] = ConvertJsonElementToObject(property.Value);
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElementToObject(item));
                }
                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                    return intValue;
                if (element.TryGetInt64(out long longValue))
                    return longValue;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return null;
        }
    }
}
