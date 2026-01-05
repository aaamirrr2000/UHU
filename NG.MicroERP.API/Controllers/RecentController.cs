using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RecentController : ControllerBase
{
    ItemsService Srv = new ItemsService();

    [HttpGet("Items/{TopN?}")]
    public async Task<IActionResult> GetRecentItems(string TopN = "")
    {
        var result = await Srv.SearchRecent(TopN)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }
}

