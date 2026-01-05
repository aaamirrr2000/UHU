using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.Latency;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Services.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TaxMasterController : ControllerBase
{
    TaxMasterService Srv = new TaxMasterService();


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

    [HttpPost("Insert")]
    public async Task<IActionResult> Insert(TaxMasterModel obj)
    {
        var result = await Srv.Post(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item3);
    }


    [HttpPost("Update")]
    public async Task<IActionResult> Update(TaxMasterModel obj)
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
    public async Task<IActionResult> SoftDelete(TaxMasterModel obj)
    {
        var result = await Srv.SoftDelete(obj)!;
        if (result.Item1 == true)
            return Ok(result.Item2);
        else
            return BadRequest(result.Item2);
    }

    [AllowAnonymous]
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateTaxes([FromBody] TaxCalculationModel request)
    {
        try
        {
            /*
             
             -- 1. Get tax calculation for a specific item
            SELECT * FROM vw_TaxCalculation WHERE ItemId = 1001;

            -- 2. Get simplified pricing with taxes
            SELECT * FROM vw_ItemPricingWithTax WHERE ItemId = 1001;

            -- 3. See all tax rules applicable to items
            SELECT * FROM vw_ItemTaxMatrix ORDER BY CategoryId, ItemCode;

            -- 4. Check tax compliance rules
            SELECT * FROM vw_TaxComplianceRules WHERE AppliesTo = 'SALE' AND Status = 'Active';

            -- 5. Calculate tax for a sales transaction
            SELECT 
                ItemId,
                ItemCode,
                ItemName,
                BasePrice,
                FinalPrice,
                TotalTaxPerItem,
                GSTAmount,
                WHTAmount,
                FEDAmount
            FROM vw_ItemPricingWithTax
            WHERE ItemId IN (1001, 1002, 1003);

             */

            return Ok();

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}