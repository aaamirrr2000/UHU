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

        if (!string.IsNullOrEmpty(Criteria) && !SQLInjectionHelper.IsSafeSearchCriteria(Criteria))
            return BadRequest("Invalid search criteria");

        var result = await Srv.Search(Criteria)!;
        if (result.Item1 == false)
            return NotFound("Record Not Found");

        return Ok(result.Item2);
    }

    [HttpGet("GetAttendanceReport/{startDate}/{endDate}")]
    public async Task<IActionResult> GetAttendanceReport(string startDate,  string endDate)
    {
        try
        {
            if (!DateTime.TryParse(startDate, out DateTime startDateValue) || !DateTime.TryParse(endDate, out DateTime endDateValue))
            {
                return BadRequest("Invalid date format. Use yyyy-MM-dd format.");
            }

            if (startDateValue > endDateValue)
                return BadRequest("Start date cannot be after end date");

            if ((endDateValue - startDateValue).Days > 90)
                return BadRequest("Date range cannot exceed 90 days");

            var result = await Srv.GetAttendanceReportAsync(startDate, endDate);

            if (result == null || result.Count == 0)
                return NotFound("No attendance records found for the specified date range");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while generating the attendance report, {ex.Message}");
        }
    }
}