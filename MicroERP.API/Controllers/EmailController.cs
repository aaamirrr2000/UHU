using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MicroERP.API.Helper;
using Serilog;

namespace MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EmailController : ControllerBase
{
    [HttpPost("Send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ToEmail))
            {
                return BadRequest(new { success = false, message = "To email address is required" });
            }

            if (string.IsNullOrEmpty(request.Subject))
            {
                return BadRequest(new { success = false, message = "Email subject is required" });
            }

            if (string.IsNullOrEmpty(request.Body))
            {
                return BadRequest(new { success = false, message = "Email body is required" });
            }

            Log.Information($"Sending email to: {request.ToEmail}, Subject: {request.Subject}");

            bool emailSent = Config.sendEmail(request.ToEmail, request.Subject, request.Body, request.Attachment);

            if (emailSent)
            {
                return Ok(new { success = true, message = $"Email sent successfully to {request.ToEmail}" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Failed to send email to {request.ToEmail}. Please check the email configuration and recipient address." });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending email to {ToEmail}", request.ToEmail);
            return StatusCode(500, new { success = false, message = $"An error occurred while sending email to {request.ToEmail}", error = ex.Message });
        }
    }
}

public class EmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Attachment { get; set; } = string.Empty;
}

