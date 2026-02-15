using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroERP.Shared.Models;
using MicroERP.API.Services;
using MicroERP.API.Helper;
using Serilog;

namespace MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PeriodCloseController : ControllerBase
{
    PeriodCloseService Srv = new PeriodCloseService();

    [HttpGet("Search/{Criteria?}")]
    public async Task<IActionResult> Search(string Criteria = "")
    {
        if (!string.IsNullOrEmpty(Criteria) && !SQLInjectionHelper.IsSafeSearchCriteria(Criteria))
            return BadRequest("Invalid search criteria");

        var result = await Srv.Search(Criteria)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }

    [HttpGet("Get/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await Srv.Get(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }

    [HttpGet("GetOpenPeriod/{organizationId}/{date}/{moduleType?}")]
    public async Task<IActionResult> GetOpenPeriod(int organizationId, string date, string moduleType = "ALL")
    {
        try
        {
            // Parse date string explicitly to avoid route parameter parsing issues
            // ASP.NET Core route model binding might have issues with DateTime parsing
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                Log.Warning("GetOpenPeriod: Invalid date format '{Date}' for OrganizationId={OrganizationId}", date, organizationId);
                return BadRequest(new { message = $"Invalid date format: {date}. Expected format: yyyy-MM-dd" });
            }
            
            // Normalize moduleType to uppercase for consistency
            moduleType = string.IsNullOrWhiteSpace(moduleType) ? "ALL" : moduleType.ToUpper().Trim();
            
            Log.Information("GetOpenPeriod Controller: OrganizationId={OrganizationId}, DateString={DateString}, ParsedDate={ParsedDate}, ModuleType={ModuleType}", 
                organizationId, date, parsedDate.ToString("yyyy-MM-dd"), moduleType);
            
            var result = await Srv.GetOpenPeriod(organizationId, parsedDate, moduleType)!;
            if (result.Item1 == false || result.Item2 == null)
            {
                Log.Warning("GetOpenPeriod Controller: No period found for OrganizationId={OrganizationId}, Date={Date}, ModuleType={ModuleType}", 
                    organizationId, parsedDate.ToString("yyyy-MM-dd"), moduleType);
                return NotFound(new { message = $"No open period found for OrganizationId={organizationId}, Date={parsedDate:yyyy-MM-dd}, ModuleType={moduleType}" });
            }

            Log.Information("GetOpenPeriod Controller: Found period Id={PeriodId}, ModuleType={ModuleType}, Status={Status}", 
                result.Item2.Id, result.Item2.ModuleType, result.Item2.Status);
            return Ok(result.Item2);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetOpenPeriod Controller Error: OrganizationId={OrganizationId}, Date={Date}, ModuleType={ModuleType}", 
                organizationId, date, moduleType);
            return BadRequest(new { message = $"Error retrieving open period: {ex.Message}", error = ex.ToString() });
        }
    }

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(PeriodCloseModel obj)
    {
        var result = await Srv.Post(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(PeriodCloseModel obj)
    {
        var result = await Srv.Put(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Srv.Delete(id)!;
        if (result.Item1 == true)
        {
            return Ok(result.Item2);
        }
        else
        {
            return BadRequest(result.Item2);
        }
    }

    [HttpPost("SoftDelete")]
    public async Task<IActionResult> SoftDelete(PeriodCloseModel obj)
    {
        var result = await Srv.SoftDelete(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("ClosePeriod/{id}")]
    public async Task<IActionResult> ClosePeriod(int id, [FromQuery] int userId)
    {
        var result = await Srv.ClosePeriod(id, userId)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }
}


