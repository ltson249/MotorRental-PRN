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

        var claims = new List<Claim>
        {
            new("sub", user.UserId.ToString()),
            new("name", user.Username),
            new("fullName", user.FullName),
            new("role", user.Role)
        };

        if (user.CustomerId.HasValue)
        {
            claims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
