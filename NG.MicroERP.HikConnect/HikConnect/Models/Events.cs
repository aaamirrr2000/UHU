using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikConnect.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

public class FaceRect
{
    [JsonPropertyName("height")] public double Height { get; set; }
    [JsonPropertyName("width")] public double Width { get; set; }
    [JsonPropertyName("x")] public double X { get; set; }
    [JsonPropertyName("y")] public double Y { get; set; }
}

public class Info
{
    [JsonPropertyName("major")] public int Major { get; set; }
    [JsonPropertyName("minor")] public int Minor { get; set; }
    [JsonPropertyName("time")] public DateTime Time { get; set; }
    [JsonPropertyName("doorNo")] public int DoorNo { get; set; }
    [JsonPropertyName("serialNo")] public long SerialNo { get; set; }
    [JsonPropertyName("cardNo")] public string? CardNo { get; set; }
    [JsonPropertyName("cardType")] public int? CardType { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("cardReaderNo")] public int? CardReaderNo { get; set; }
    [JsonPropertyName("employeeNoString")] public string? EmployeeNoString { get; set; }
    [JsonPropertyName("userType")] public string? UserType { get; set; }
    [JsonPropertyName("currentVerifyMode")] public string? CurrentVerifyMode { get; set; }
    [JsonPropertyName("mask")] public string? Mask { get; set; }
    [JsonPropertyName("FaceRect")] public FaceRect? FaceRect { get; set; }
}

public class AcsEvent
{
    [JsonPropertyName("searchID")] public string SearchID { get; set; } = string.Empty;
    [JsonPropertyName("totalMatches")] public int TotalMatches { get; set; }
    [JsonPropertyName("responseStatusStrg")] public string ResponseStatusStrg { get; set; } = string.Empty;
    [JsonPropertyName("numOfMatches")] public int NumOfMatches { get; set; }
    [JsonPropertyName("InfoList")] public List<Info> InfoList { get; set; } = new();
}

public class Root
{
    [JsonPropertyName("AcsEvent")] public AcsEvent AcsEvent { get; set; } = new();
}

public class HikvisionEventParser
{
    public static Root ParseAcsEventJson(string json)
    {
        try
        {
            // Step 1: Create objects manually
            Root root = new Root
            {
                AcsEvent = new AcsEvent
                {
                    InfoList = new List<Info>()
                }
            };

            // Step 2: Deserialize JSON into object
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var result = JsonSerializer.Deserialize<Root>(json, options);

            if (result == null)
            {
                Console.WriteLine("⚠️ No data parsed from JSON.");
                return root;
            }

            // Step 3: Copy data safely into created object
            root.AcsEvent.SearchID = result.AcsEvent.SearchID;
            root.AcsEvent.TotalMatches = result.AcsEvent.TotalMatches;
            root.AcsEvent.ResponseStatusStrg = result.AcsEvent.ResponseStatusStrg;
            root.AcsEvent.NumOfMatches = result.AcsEvent.NumOfMatches;

            foreach (var ev in result.AcsEvent.InfoList)
            {
                string name = string.IsNullOrEmpty(ev.Name) ? "Unknown" : ev.Name.ToUpper();
                string verifyMode = string.IsNullOrEmpty(ev.CurrentVerifyMode) ? "invalid" : ev.CurrentVerifyMode;

                if (name == "UNKNOWN" || verifyMode.ToUpper() == "INVALID")
                {
                    
                }
                else
                {
                    Info info = new Info
                    {
                        Major = ev.Major,
                        Minor = ev.Minor,
                        Time = ev.Time,
                        DoorNo = ev.DoorNo,
                        SerialNo = ev.SerialNo,
                        CardNo = ev.CardNo,
                        CardType = ev.CardType,
                        Name = string.IsNullOrEmpty(ev.Name) ? "Unknown" : ev.Name.ToUpper(),
                        CardReaderNo = ev.CardReaderNo,
                        EmployeeNoString = ev.EmployeeNoString,
                        UserType = ev.UserType,
                        CurrentVerifyMode = string.IsNullOrEmpty(ev.CurrentVerifyMode) ? "invalid" : ev.CurrentVerifyMode,
                        Mask = ev.Mask,
                        FaceRect = ev.FaceRect
                    };

                    root.AcsEvent.InfoList.Add(info);
                }
            }

            Console.WriteLine($"✅ Parsed {root.AcsEvent.InfoList.Count} records successfully.");
            return root;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ JSON parsing error: {ex.Message}");
            return new Root();
        }
    }
}
