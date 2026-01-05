using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MySqlX.XDevAPI.Common;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Models;

using static MudBlazor.CategoryTypes;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InvoiceController : ControllerBase
{
    InvoiceService Srv = new InvoiceService();

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

    [HttpGet("Get")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await Srv.Get(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);

    }

    [HttpGet("GetInvoiceReport")]
    public async Task<IActionResult> GetInvoiceReport(int id)
    {
        var result = await Srv.GetInvoiceReport(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }
    
    [HttpGet("GetInvoiceCompleteReport/{id}")]
    public async Task<IActionResult> GetInvoiceCompleteReport(int id)
    {
        var result = await Srv.GetInvoiceCompleteReport(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(InvoicesModel obj)
    {
        try
        {
            var result = await Srv.Post(obj)!;
            if (result.Item1 == true)
                return Ok(result.Item2);
            else
            {
                string errorMessage = result.Item3 ?? "Failed to save invoice";
                return BadRequest(new { message = errorMessage, error = errorMessage });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message, error = ex.Message });
        }
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(InvoicesModel obj)
    {
        try
        {
            var result = await Srv.Put(obj)!;
            if (result.Item1 == true)
                return Ok(result.Item2);
            else
            {
                string errorMessage = result.Item3 ?? "Failed to update invoice";
                return BadRequest(new { message = errorMessage, error = errorMessage });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message, error = ex.Message });
        }
    }

    //[HttpPost("StatusUpdate/{Status}")]
    //public async Task<IActionResult> StatusUpdate(InvoiceModel Invoice, string Status)
    //{
    //    var result = await Srv.PutStatus(Invoice, Status)!;
    //    if (result.Item1 == true)
    //        return Ok(result.Item2);
    //    else
    //        return BadRequest(result.Item2);
    //}

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
    public async Task<IActionResult> SoftDelete(InvoiceModel obj)
    {
        var result = await Srv.SoftDelete(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("ClientComments")]
    public async Task<IActionResult> ClientComments(InvoicesModel obj)
    {
        var result = await Srv.ClientComments(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("GenerateInvoice")]
    public async Task<IActionResult> GenerateInvoice(InvoiceModel obj)
    {
        //var result = await Srv.GenerateInvoice(obj)!;
        //if (result.Item1 == true)
        //    return Ok(result.Item2);
        //else
        //    return BadRequest(result.Item2);

        return BadRequest();
    }

    [HttpPost("InvoiceStatus/{Id}/{Status}/{SoftDelete?}")]
    public async Task<IActionResult> InvoiceStatus(int Id, string Status, int SoftDelete = 0)
    {
        var result = await Srv.InvoiceStatus(Id, Status, SoftDelete)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }
}