using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroERP.API.Helper;
using MicroERP.API.Services;
using MicroERP.Shared.Models;

namespace MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportsService Srv = new();

    [HttpGet("DailyFundsClosing/{organizationId}/{reportDate}")]
    public async Task<IActionResult> GetDailyFundsClosing(int organizationId, DateTime reportDate)
    {
        try
        {
            var result = await Srv.GetDailyFundsClosing(organizationId, reportDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("InventoryClosing/{organizationId}/{reportDate}")]
    public async Task<IActionResult> GetInventoryClosing(int organizationId, DateTime reportDate)
    {
        try
        {
            var result = await Srv.GetInventoryClosing(organizationId, reportDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("CashByLocationUser/{organizationId}/{asOfDate}")]
    public async Task<IActionResult> GetCashByLocationUser(int organizationId, DateTime asOfDate)
    {
        try
        {
            var result = await Srv.GetCashByLocationUser(organizationId, asOfDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("InventoryByLocation/{organizationId}/{asOfDate}")]
    public async Task<IActionResult> GetInventoryByLocation(int organizationId, DateTime asOfDate)
    {
        try
        {
            var result = await Srv.GetInventoryByLocation(organizationId, asOfDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("TrialBalance/{organizationId}")]
    public async Task<IActionResult> GetTrialBalance(int organizationId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest("Start date cannot be after end date.");
            var result = await Srv.GetTrialBalance(organizationId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("AccountAnalysis/{organizationId}")]
    public async Task<IActionResult> GetAccountAnalysis(int organizationId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest("Start date cannot be after end date");
            var result = await Srv.GetAccountAnalysis(organizationId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("ProfitLoss/{organizationId}/{startDate}/{endDate}")]
    public async Task<IActionResult> GetProfitLoss(int organizationId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest("Start date cannot be after end date");

            var result = await Srv.GetProfitLoss(organizationId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("BalanceSheet/{organizationId}/{asOfDate}")]
    public async Task<IActionResult> GetBalanceSheet(int organizationId, DateTime asOfDate)
    {
        try
        {
            var result = await Srv.GetBalanceSheet(organizationId, asOfDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("CashReconciliation/{organizationId}/{reportDate}")]
    public async Task<IActionResult> GetCashReconciliation(int organizationId, DateTime reportDate, [FromQuery] int? locationId = null, [FromQuery] int? countedBy = null, [FromQuery] DateTime? countedOn = null)
    {
        try
        {
            var result = await Srv.GetCashReconciliation(organizationId, reportDate, locationId, countedBy, countedOn);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("PhysicalCashCountSessions/{organizationId}/{reportDate}")]
    public async Task<IActionResult> GetPhysicalCashCountSessions(int organizationId, DateTime reportDate, [FromQuery] int? locationId = null)
    {
        try
        {
            var result = await Srv.GetPhysicalCashCountSessions(organizationId, reportDate, locationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error getting physical cash count sessions: {ex.Message}");
        }
    }

    [HttpGet("EmployeeAdvances/{organizationId}")]
    public async Task<IActionResult> GetEmployeeAdvances(int organizationId, [FromQuery] int? locationId = null, [FromQuery] int? employeeId = null)
    {
        try
        {
            var result = await Srv.GetEmployeeAdvances(organizationId, locationId, employeeId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("GetDashboardData/{organizationId}")]
    public async Task<IActionResult> GetDashboardData(int organizationId)
    {
        try
        {
            var result = await Srv.GetDashboardData(organizationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error getting dashboard data: {ex.Message}");
        }
    }

    [HttpGet("CashMovement/{organizationId}/{startDate}/{endDate}")]
    public async Task<IActionResult> GetCashMovement(int organizationId, DateTime startDate, DateTime endDate, [FromQuery] int? locationId = null)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest("Start date cannot be after end date");

            var result = await Srv.GetCashMovement(organizationId, startDate, endDate, locationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("PartyReceivablePayable/{organizationId}")]
    public async Task<IActionResult> GetPartyReceivablePayable(int organizationId, [FromQuery] string? partyType = null)
    {
        try
        {
            var result = await Srv.GetPartyReceivablePayable(organizationId, partyType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }
}

