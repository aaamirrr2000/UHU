using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MicroERP.API.Helper;
using MicroERP.API.Services;
using MicroERP.Shared.Models;

namespace MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GroupMenuController : ControllerBase
{
    GroupMenuService Srv = new GroupMenuService();

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

    [HttpGet("GetAccessLevel/{UserId}/{PageName}")]
    public async Task<IActionResult> GetAccessLevel(int UserId, string PageName)
    {
        var result = await Srv.AccessLevel(UserId, PageName)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);

    }
}
