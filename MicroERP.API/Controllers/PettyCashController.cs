using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MicroERP.Shared.Models;
using MicroERP.API.Services.Services;
using MicroERP.API.Helper;

namespace MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PettyCashController : ControllerBase
{
    PettyCashService Srv = new PettyCashService();

    [HttpGet("Search/{Criteria?}")]
    public async Task<IActionResult> Search(string Criteria = "")
    {
        if (!string.IsNullOrEmpty(Criteria) && !SQLInjectionHelper.IsSafeSearchCriteria(Criteria))
            return BadRequest("Invalid search criteria. The search criteria contains potentially unsafe SQL characters or keywords.");

        var result = await Srv.Search(Criteria)!;
        if (result.Item1 == false)
            return NotFound("No PettyCash entries found matching the search criteria.");

        return Ok(result.Item2);
    }

    [HttpGet("Get/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        if (id <= 0)
            return BadRequest($"Invalid PettyCash entry ID: {id}. ID must be greater than zero.");

        var result = await Srv.Get(id)!;
        if (result.Item1 == false)
            return NotFound($"PettyCash entry with ID {id} not found or has been deleted.");

        return Ok(result.Item2);
    }

    [HttpGet("GetPettyCashReport/{id}")]
    public async Task<IActionResult> GetPettyCashReport(int id)
    {
        if (id <= 0)
            return BadRequest($"Invalid PettyCash entry ID: {id}. ID must be greater than zero.");

        var result = await Srv.GetPettyCashReport(id)!;
        if (result.Item1 == false)
            return NotFound($"PettyCash report for entry ID {id} not found. The entry may not exist or the report view may not be available.");

        return Ok(result.Item2);
    }

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(PettyCashModel obj)
    {
        var result = await Srv.Post(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(PettyCashModel obj)
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
    public async Task<IActionResult> SoftDelete(PettyCashModel obj)
    {
        var result = await Srv.SoftDelete(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }
}

