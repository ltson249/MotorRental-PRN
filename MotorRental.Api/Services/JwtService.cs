using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MotorRental.Api.Models;

namespace MotorRental.Api.Services;

public class JwtService
{
    private readonly IConfiguration config;
    public JwtService(IConfiguration config) { this.config = config; }

    public (string Token, DateTime Expires) CreateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpireMinutes"] ?? "120"));

        var claims = new[]
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("name", user.Username),
            new Claim("fullName", user.FullName),
            new Claim("role", user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
