using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Controllers;

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
    public async Task<IActionResult> GetOpenPeriod(int organizationId, DateTime date, string moduleType = "ALL")
    {
        var result = await Srv.GetOpenPeriod(organizationId, date, moduleType)!;
        if (result.Item1 == false)
            return NotFound("No open period found for the specified date and module");

        return Ok(result.Item2);
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

