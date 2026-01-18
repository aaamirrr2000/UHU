using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RestaurantTablesController : ControllerBase
{
    private readonly RestaurantTablesService Srv = new();

    [HttpGet("Search/{Criteria?}")]

    public async Task<IActionResult> Search(string Criteria = "")
    {
        try
        {
            string searchCriteria = Criteria ?? "";
            
            if (!string.IsNullOrEmpty(searchCriteria) && !SQLInjectionHelper.IsSafeSearchCriteria(searchCriteria))
            {
                Serilog.Log.Warning($"Invalid search criteria: {searchCriteria}");
                return BadRequest("Invalid search criteria");
            }

            var result = await Srv.Search(searchCriteria)!;
            if (result.Item1 == false)
            {
                Serilog.Log.Warning("No restaurant tables found");
                return NotFound("Record Not Found");
            }

            Serilog.Log.Information($"Returning {result.Item2?.Count ?? 0} restaurant tables");
            return Ok(result.Item2);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error in RestaurantTablesController.Search: {ex.Message}");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("Get")]
    public async Task<IActionResult> Get([FromQuery] int id)
    {
        var result = await Srv.Get(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(RestaurantTablesModel obj)
    {
        var result = await Srv.Post(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(RestaurantTablesModel obj)
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
    public async Task<IActionResult> SoftDelete(RestaurantTablesModel obj)
    {
        var result = await Srv.SoftDelete(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }
}
