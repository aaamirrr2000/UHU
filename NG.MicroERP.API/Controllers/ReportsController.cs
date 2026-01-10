using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    ReportsService Srv = new ReportsService();

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

    [HttpGet("TrialBalance/{organizationId}/{asOfDate}")]
    public async Task<IActionResult> GetTrialBalance(int organizationId, DateTime asOfDate)
    {
        try
        {
            var result = await Srv.GetTrialBalance(organizationId, asOfDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating report: {ex.Message}");
        }
    }

    [HttpGet("AccountAnalysis/{organizationId}/{startDate}/{endDate}")]
    public async Task<IActionResult> GetAccountAnalysis(int organizationId, DateTime startDate, DateTime endDate)
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
}
