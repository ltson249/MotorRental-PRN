using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Data;
using MotorRental.Api.DTOs;
using MotorRental.Api.Models;
using MotorRental.Api.Services;

namespace MotorRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly MotorRentalContext db;
    public UsersController(MotorRentalContext db) { this.db = db; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> Get() =>
        Ok(await db.Users.OrderBy(u => u.Username).Select(u => new UserDTO
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName,
            Role = u.Role,
            IsActive = u.IsActive
        }).ToListAsync());

    [HttpPost]
    public async Task<ActionResult<UserDTO>> Create(CreateUserDTO dto)
    {
        if (await db.Users.AnyAsync(u => u.Username == dto.Username))
            return Conflict(new { message = "Tên đăng nhập đã tồn tại" });

        var u = new AppUser
        {
            Username = dto.Username.Trim(),
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FullName = dto.FullName.Trim(),
            Role = dto.Role,
            IsActive = true
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return Created($"/api/users/{u.UserId}", new UserDTO
        {
            UserId = u.UserId, Username = u.Username, FullName = u.FullName, Role = u.Role, IsActive = u.IsActive
        });
    }

    // Khoa / mo khoa tai khoan
    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        if (id == int.Parse(User.FindFirstValue("sub")!))
            return BadRequest(new { message = "Không thể khóa chính tài khoản đang đăng nhập" });

        var u = await db.Users.FindAsync(id);
        if (u == null) return NotFound(new { message = "Không tìm thấy tài khoản" });
        u.IsActive = !u.IsActive;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
