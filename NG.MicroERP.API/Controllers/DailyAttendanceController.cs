using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services.Services;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DailyAttendanceController : ControllerBase
{
    DailyAttendanceService Srv = new DailyAttendanceService();

    [HttpGet("Search/{Criteria?}")]
    public async Task<IActionResult> Search(string Criteria = "")
    {

        if (!string.IsNullOrEmpty(Criteria) && !Config.IsSafeSearchCriteria(Criteria))
            return BadRequest("Invalid search criteria");

        var result = await Srv.Search(Criteria)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }

    [HttpGet("Get")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await Srv.Get(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);

    }
}