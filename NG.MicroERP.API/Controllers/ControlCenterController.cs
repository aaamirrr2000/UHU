using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NG.MicroERP.API.Services.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ControlCenterController : ControllerBase
{
    private readonly IControlCenterService _controlCenterService;
    private readonly IConfiguration _configuration;

    public ControlCenterController(IControlCenterService controlCenterService, IConfiguration configuration)
    {
        _controlCenterService = controlCenterService;
        _configuration = configuration;
    }

    [HttpGet("CheckAccountStatus/{code}")]
    public async Task<IActionResult> CheckAccountStatus(string code)
    {
        if (string.IsNullOrEmpty(code) || code == "Current")
        {
            // Get organization code from current database
            code = await GetCurrentOrganizationCodeAsync();
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Organization code could not be determined");
            }
        }

        var result = await _controlCenterService.CheckAccountStatusAsync(code);
        
        return Ok(new
        {
            IsActive = result.IsActive,
            Message = result.Message
        });
    }

    [HttpGet("IsAccountActive/{code}")]
    public async Task<IActionResult> IsAccountActive(string code)
    {
        if (string.IsNullOrEmpty(code) || code == "Current")
        {
            // Get organization code from current database
            code = await GetCurrentOrganizationCodeAsync();
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Organization code could not be determined");
            }
        }

        var isActive = await _controlCenterService.IsAccountActiveAsync(code);
        
        return Ok(new { IsActive = isActive });
    }

    private async Task<string?> GetCurrentOrganizationCodeAsync()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return null;

            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "SELECT TOP 1 Code FROM Organizations WHERE IsSoftDeleted = 0 ORDER BY Id";
            var code = await connection.QueryFirstOrDefaultAsync<string>(sql);
            return code;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error getting organization code from current database");
            return null;
        }
    }
}
