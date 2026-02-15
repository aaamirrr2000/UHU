using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroERP.API.Helper;

namespace MicroERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class testController : ControllerBase
    {

        private readonly JWT _jwt;

        public testController(JWT jwt)
        {
            _jwt = jwt;
        }

        [HttpGet("TestAnonymous")]
        [AllowAnonymous]
        public IActionResult TestAnonymous()
        {
            string token = _jwt.GenerateJwtToken("aamir");
            return Ok(token);
        }

        [HttpPost("TestAuthorized")]
        public IActionResult TestAuthorized()
        {
            return Ok("Your Controller is authorized");
        }


        [HttpPost("Post")]
        public IActionResult Post([FromBody] YourModel model)
        {
            return model == null ? BadRequest("Invalid data.") : Ok("Data received successfully.");
        }
    }
}

public class YourModel
{
    public required string Field1 { get; set; }
    public required string Field2 { get; set; }
}
