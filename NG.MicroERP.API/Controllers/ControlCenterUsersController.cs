using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.API.Services.Services;
using NG.MicroERP.API.Helper;
using System.Security.Cryptography;
using System.Text;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ControlCenterUsersController : ControllerBase
{
    private readonly IControlCenterService _controlCenterService;

    public ControlCenterUsersController(IControlCenterService controlCenterService)
    {
        _controlCenterService = controlCenterService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] ControlCenterUserRegistrationModel model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("FullName, Email, and Password are required");
            }

            // Hash password
            string passwordHash = HashPassword(model.Password);

            // Create user in ControlCenter database
            var result = await _controlCenterService.RegisterUserAsync(
                model.FullName,
                model.Email,
                model.Phone,
                passwordHash
            );

            if (result.Success)
            {
                return Ok(new { UserId = result.UserId, Message = "User registered successfully" });
            }

            return BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error registering user");
            return StatusCode(500, "An error occurred while registering the user");
        }
    }

    [HttpPost("CheckEmail")]
    public async Task<IActionResult> CheckEmail([FromBody] CheckEmailModel model)
    {
        try
        {
            var exists = await _controlCenterService.CheckEmailExistsAsync(model.Email);
            return Ok(new { Exists = exists });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error checking email");
            return StatusCode(500, "An error occurred while checking email");
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

public class ControlCenterUserRegistrationModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class CheckEmailModel
{
    public string Email { get; set; } = string.Empty;
}
