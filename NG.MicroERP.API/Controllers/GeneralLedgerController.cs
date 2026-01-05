using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GeneralLedgerController : ControllerBase
{
    GeneralLedgerService Srv = new GeneralLedgerService();

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

    [HttpGet("GetGeneralLedgerReport/{id}")]
    public async Task<IActionResult> GetGeneralLedgerReport(int id)
    {
        var result = await Srv.GetGeneralLedgerReport(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(GeneralLedgerHeaderModel obj)
    {
        var result = await Srv.Post(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(GeneralLedgerHeaderModel obj)
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
    public async Task<IActionResult> SoftDelete(GeneralLedgerHeaderModel obj)
    {
        var result = await Srv.SoftDelete(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("PostEntry")]
    public async Task<IActionResult> PostEntry([FromBody] string entryNo)
    {
        var result = await Srv.PostEntry(entryNo)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("CreateGLFromInvoice/{invoiceId}")]
    public async Task<IActionResult> CreateGLFromInvoice(int invoiceId, [FromQuery] int userId, [FromQuery] string clientInfo)
    {
        var result = await Srv.CreateGLFromInvoice(invoiceId, userId, clientInfo)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("CreateGLFromCashBook/{cashBookId}")]
    public async Task<IActionResult> CreateGLFromCashBook(int cashBookId, [FromQuery] int userId, [FromQuery] string clientInfo)
    {
        var result = await Srv.CreateGLFromCashBook(cashBookId, userId, clientInfo)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }

    [HttpPost("PreviewGLFromInvoice")]
    public async Task<IActionResult> PreviewGLFromInvoice([FromBody] InvoicesModel invoice)
    {
        try
        {
            if (invoice == null || invoice.Invoice == null)
            {
                return BadRequest(new { message = "Invoice data is required", error = "Invalid invoice model" });
            }

            var result = await Srv.PreviewGLFromInvoice(invoice)!;
            if (result.Item1 == true && result.Item2 != null)
                return Ok(result.Item2);
            else
                return BadRequest(new { message = "Unable to generate preview. Please ensure all required accounts are configured in Chart of Accounts.", error = "Preview generation failed" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error generating preview: {ex.Message}", error = ex.Message });
        }
    }

    [HttpPost("PreviewGLFromCashBook")]
    public async Task<IActionResult> PreviewGLFromCashBook([FromBody] CashBookModel cashBook)
    {
        try
        {
            if (cashBook == null)
            {
                return BadRequest(new { message = "CashBook data is required", error = "Invalid cashbook model" });
            }

            var result = await Srv.PreviewGLFromCashBook(cashBook)!;
            if (result.Item1 == true && result.Item2 != null)
                return Ok(result.Item2);
            else
                return BadRequest(new { message = "Unable to generate preview. Please ensure all required accounts are configured in Chart of Accounts.", error = "Preview generation failed" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error generating preview: {ex.Message}", error = ex.Message });
        }
    }
}
