using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TypeCodeController : ControllerBase
{
    TypeCodeService Srv = new TypeCodeService();

    [HttpGet("Search/{Value?}")]
    public async Task<IActionResult> Search(string Value="")
    {
        // Escape single quotes to prevent SQL injection
        string safeValue = Value?.Replace("'", "''") ?? "";
        
        // Use case-insensitive comparison to match stored uppercase values
        var result = await Srv.Search($"UPPER(ListName) = UPPER('{safeValue}')")!;
        
        // Return empty list if no records found instead of 404
        if (result.Item1 == false || result.Item2 == null || result.Item2.Count == 0)
            return Ok(new List<TypeCodeModel>());

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

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(TypeCodeModel obj)
    {
        var result = await Srv.Post(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(TypeCodeModel obj)
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
}