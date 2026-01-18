using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.API.Services.Services;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ControlCenterOrganizationsController : ControllerBase
{
    private readonly IControlCenterService _controlCenterService;

    public ControlCenterOrganizationsController(IControlCenterService controlCenterService)
    {
        _controlCenterService = controlCenterService;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationModel model)
    {
        try
        {
            if (model.OwnerUserId <= 0 || string.IsNullOrEmpty(model.OrganizationName))
            {
                return BadRequest("OwnerUserId and OrganizationName are required");
            }

            var result = await _controlCenterService.CreateOrganizationAsync(
                model.OwnerUserId,
                model.OrganizationName,
                model.Email,
                model.Phone
            );

            if (result.Success)
            {
                return Ok(new { OrganizationId = result.OrganizationId, Message = result.Message });
            }

            return BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error creating organization");
            return StatusCode(500, "An error occurred while creating the organization");
        }
    }
}

public class CreateOrganizationModel
{
    public int OwnerUserId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
