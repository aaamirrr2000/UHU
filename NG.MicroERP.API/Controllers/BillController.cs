using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MySqlX.XDevAPI.Common;

using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Models;

using static MudBlazor.CategoryTypes;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BillController : ControllerBase
{
    BillService Srv = new BillService();

    [HttpGet("Search/{Criteria?}")]
    public async Task<IActionResult> Search(string Criteria = "")
    {
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

    [HttpGet("GetBillReport")]
    public async Task<IActionResult> GetBillReport(int id)
    {
        var result = await Srv.GetBillReport(id)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);

    }

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(BillsModel obj)
    {
        var result = await Srv.Post(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(BillsModel obj)
    {
        var result = await Srv.Put(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    //[HttpPost("StatusUpdate/{Status}")]
    //public async Task<IActionResult> StatusUpdate(BillModel bill, string Status)
    //{
    //    var result = await Srv.PutStatus(bill, Status)!;
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
    public async Task<IActionResult> SoftDelete(BillModel obj)
    {
        var result = await Srv.SoftDelete(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("ClientComments")]
    public async Task<IActionResult> ClientComments(BillsModel obj)
    {
        var result = await Srv.ClientComments(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpPost("GenerateBill")]
    public async Task<IActionResult> GenerateBill(BillModel obj)
    {
        //var result = await Srv.GenerateBill(obj)!;
        //if (result.Item1 == true)
        //    return Ok(result.Item2);
        //else
        //    return BadRequest(result.Item2);

        return BadRequest();
    }

    [HttpPost("BillStatus/{Id}/{Status}/{SoftDelete?}")]
    public async Task<IActionResult> BillStatus(int Id, string Status, int SoftDelete = 0)
    {
        var result = await Srv.BillStatus(Id, Status, SoftDelete)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [HttpGet("CalculateBillCharges/{StartDate}/{EndDate}/{BilledAmount}")]
    public async Task<IActionResult> CalculateBillCharges(DateTime StartDate, DateTime EndDate, decimal BilledAmount = 0)
    {
        var result = await Srv.CalculateBillCharges(StartDate, EndDate, BilledAmount)!;
        return Ok(result);
    }

}