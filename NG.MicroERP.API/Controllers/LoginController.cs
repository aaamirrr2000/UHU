using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;

namespace NG.MicroERP.API.Controllers;


[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class LoginController : ControllerBase
{
    private readonly UsersService Srv = new();

    private readonly JWT _jwt;

    public LoginController(JWT jwt)
    {
        _jwt = jwt;
    }

    [HttpGet("Login/{Username}/{Password}")]
    public async Task<IActionResult> Login(string Username, string Password)
    {
        (bool, UsersModel) result = await Srv.Login(Username.ToUpper(), Password)!;
        if (result.Item1 == true)
        {
            UsersModel? usr = result.Item2;
            List<UsersModel> resUsersInfo = Srv.Search($"Users.Id={usr.Id}")!.Result.Item2;
            if (resUsersInfo.Count == 0)
                return NotFound("Record Not Found");

            UsersModel user = resUsersInfo[0];
            user.Token = _jwt.GenerateJwtToken(user.Username!);
            return Ok(user);
        }
        return NotFound("Record Not Found");
    }

    [HttpGet("ForgotPassword/{Email}")]
    public async Task<IActionResult> ForgotPassword(string Email)
    {
        var result = await Srv.ForgotPassword(Email)!;
        if (result.Item1 == true)
        {
            return Ok();
        }
        return NotFound("Record Not Found");
    }
}


