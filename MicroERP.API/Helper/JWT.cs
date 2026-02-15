using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroERP.API.Helper;

public class JWT
{
    private readonly IConfiguration _config;

    public JWT(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwtToken(string username)
    {
        byte[] key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.Name, username), // Required for User.Identity.Name
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        JwtSecurityToken token = new(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
