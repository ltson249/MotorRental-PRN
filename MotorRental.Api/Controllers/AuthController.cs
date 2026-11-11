using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Data;
using MotorRental.Api.DTOs;
using MotorRental.Api.Services;

namespace MotorRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MotorRentalContext db;
    private readonly JwtService jwt;
    public AuthController(MotorRentalContext db, JwtService jwt) { this.db = db; this.jwt = jwt; }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

        // Kiểm tra: cho phép trùng mật khẩu thô HOẶC qua PasswordHasher.Verify
        bool isPasswordValid = user != null && (user.PasswordHash == dto.Password || PasswordHasher.Verify(dto.Password, user.PasswordHash));

        if (user == null || !user.IsActive || !isPasswordValid)
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        var (token, expires) = jwt.CreateToken(user);
        return Ok(new LoginResponseDTO
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            ExpiresAt = expires
        });
    }
}
