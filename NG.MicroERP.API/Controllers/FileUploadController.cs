using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        Directory.CreateDirectory(_uploadPath);
        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(_uploadPath, uniqueFileName);


        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return only the filename, not the full URL
        return Ok(new { fileName = uniqueFileName, fileUrl = uniqueFileName });
    }
}