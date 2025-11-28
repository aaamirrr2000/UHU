using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using NG.MicroERP.Shared.Helper;

public class FileUploadService
{
    private readonly HttpClient _httpClient;
    private readonly Globals Globals;
    

    public FileUploadService(HttpClient httpClient, Globals _globals)
    {
        _httpClient = httpClient;
        Globals = _globals;
    }

    public async Task<(bool, string)> UploadFileAsync(IBrowserFile file, bool useTokenAuthorize = false, int FileSizeinMB=2)
    {
        if (file == null)
        {
            return (false, "No File Selected");
        }

        long maxFileSize = FileSizeinMB * 1024 * 1024;
        if (file.Size > maxFileSize)
        {
            return (false, $"File is too large. Max size is {maxFileSize / 1024 / 1024}MB.");
        }

        string uri = $"{Globals.BaseURI}FileUpload";
        var content = new MultipartFormDataContent();

        using var fileStream = file.OpenReadStream();

        if (fileStream.Length == 0)
        {
            return (false, "The file is empty.");
        }

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        if (useTokenAuthorize && !string.IsNullOrEmpty(Globals.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.Token);
        }

        content.Add(fileContent, "file", file.Name);

        try
        {
            var response = await _httpClient.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (result.TryGetProperty("fileUrl", out JsonElement fileUrlElement))
                {
                    return (true, fileUrlElement.GetString());
                }
                else
                {
                    return (false, "The file not uploaded.");
                }
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return (false, $"Upload failed. Status: {response.StatusCode}, Error: {errorResponse}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred while uploading the file: {ex.Message}");
        }
    }

}