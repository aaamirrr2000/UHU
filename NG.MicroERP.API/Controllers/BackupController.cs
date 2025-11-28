using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.API.Services.Services;
using Serilog;
using System.Configuration;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BackupController : ControllerBase
{
    BackupService _backupService = new BackupService();

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromQuery] bool email = true, [FromQuery] bool ftp = false)
    {
        try
        {
            Log.Information("Backup requested by user: {User}", User.Identity?.Name);

            var result = await _backupService.BackupAllAsync(email, ftp);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    backupPath = result.BackupPath,
                    timestamp = result.Timestamp,
                    details = result.Details
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    error = result.Error,
                    timestamp = result.Timestamp,
                    details = result.Details
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during backup process");
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error during backup",
                error = ex.Message
            });
        }
    }

    // Optional: Individual backup endpoints
    [HttpPost("database")]
    public async Task<IActionResult> BackupDatabase()
    {
        var result = await _backupService.BackupSqlServerDatabaseAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("images")]
    public async Task<IActionResult> BackupImages()
    {
        var result = await _backupService.BackupImagesFolderAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}