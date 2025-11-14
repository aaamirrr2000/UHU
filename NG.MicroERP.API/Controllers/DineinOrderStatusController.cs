using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NG.MicroERP.Shared.Models;

using NG.MicroERP.API.Services;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DineinOrderStatusController : ControllerBase
{
    DineinOrderStatusService Srv = new DineinOrderStatusService();

    [HttpGet("Search/{LocationId}/{Criteria?}")]
    public async Task<IActionResult> Search(int LocationId, string Criteria = "")
    {
        string Condition = $"LocationId={LocationId} AND {Criteria}";
        var result = await Srv.Search(Condition)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }
}