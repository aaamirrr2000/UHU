using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroERP.API.Helper;
using MicroERP.API.Services;
using MicroERP.Shared.Models;

namespace MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class WebsiteRegistrationController : ControllerBase
{
    private readonly UsersService _usersService = new();

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] WebsiteRegistrationModel model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.FullName))
            {
                return BadRequest("Email, Password, and FullName are required");
            }

            // Check if user already exists before attempting to create
            var emailUpper = model.Email.ToUpper();
            var existingUser = await _usersService.Search($"UPPER(Username) = '{emailUpper.Replace("'", "''")}' AND IsSoftDeleted = 0");
            if (existingUser.Item1 && existingUser.Item2 != null && existingUser.Item2.Any())
            {
                return BadRequest("Email already registered. Please use a different email or sign in.");
            }

            // Create user with encrypted password
            // Store email in uppercase as username (for consistency with existing system)
            var userModel = new UsersModel
            {
                Username = emailUpper, // Store as uppercase for username consistency
                Password = Config.Encrypt(model.Password), // Encrypt the password
                UserType = "DESKTOP USER",
                EmpId = 0,
                GroupId = 1, // Default group
                LocationId = 1, // Default location
                IsActive = 1,
                CreatedBy = 1,
                CreatedFrom = "WebSite",
                UpdatedBy = 1,
                UpdatedFrom = "WebSite",
                IsSoftDeleted = 0
            };

            var result = await _usersService.Post(userModel);
            
            if (result.Item1)
            {
                return Ok(new { UserId = result.Item2.Id, Message = "User registered successfully" });
            }

            // If duplicate found, return a more specific error
            if (result.Item3 == "Duplicate Record Found.")
            {
                return BadRequest("Email already registered. Please use a different email or sign in.");
            }

            return BadRequest(result.Item3 ?? "Registration failed");
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error registering user from website");
            return StatusCode(500, "An error occurred while registering the user");
        }
    }

    [HttpPost("CheckEmail")]
    public async Task<IActionResult> CheckEmail([FromBody] WebsiteCheckEmailModel model)
    {
        try
        {
            var result = await _usersService.Search($"UPPER(Username) = '{model.Email.ToUpper()}'");
            bool exists = result.Item1 && result.Item2 != null && result.Item2.Any();
            return Ok(new { Exists = exists });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error checking email");
            return StatusCode(500, "An error occurred while checking email");
        }
    }
}

public class WebsiteRegistrationModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class WebsiteCheckEmailModel
{
    public string Email { get; set; } = string.Empty;
}

