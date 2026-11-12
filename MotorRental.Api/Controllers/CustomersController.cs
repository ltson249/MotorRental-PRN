using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Data;
using MotorRental.Api.DTOs;
using MotorRental.Api.Models;

namespace MotorRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly MotorRentalContext db;
    public CustomersController(MotorRentalContext db) { this.db = db; }

    private int CurrentUserId
    {
        get
        {
            var idClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> Get([FromQuery] string? q)
    {
        var query = db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.FullName.Contains(q) || c.Phone.Contains(q) || c.IdentityNumber.Contains(q));

        var list = await query.OrderBy(c => c.FullName).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    /// <summary>
    /// Lấy thông tin hồ sơ của chính khách hàng đang đăng nhập (Role: Customer)
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerDTO>> GetMyProfile()
    {
        var c = await db.Customers.FirstOrDefaultAsync(x => x.UserId == CurrentUserId);
        return c == null ? NotFound(new { message = "Chưa có thông tin khách hàng liên kết với tài khoản này" }) : Ok(ToDto(c));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<ActionResult<CustomerDTO>> GetById(int id)
    {
        var c = await db.Customers.FindAsync(id);
        if (c == null) return NotFound(new { message = "Không tìm thấy khách hàng" });

        if (User.IsInRole("Customer") && c.UserId != CurrentUserId)
        {
            return Forbid();
        }

        return Ok(ToDto(c));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<CustomerDTO>> Create(CustomerSaveDTO dto)
    {
        if (await db.Customers.AnyAsync(c => c.Phone == dto.Phone))
            return Conflict(new { message = "Số điện thoại đã tồn tại" });
        if (await db.Customers.AnyAsync(c => c.IdentityNumber == dto.IdentityNumber))
            return Conflict(new { message = "CCCD/CMND đã tồn tại" });

        var c = new Customer
        {
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone,
            IdentityNumber = dto.IdentityNumber,
            DriverLicense = dto.DriverLicense,
            Address = dto.Address
        };
        db.Customers.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = c.CustomerId }, ToDto(c));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, CustomerSaveDTO dto)
    {
        var c = await db.Customers.FindAsync(id);
        if (c == null) return NotFound(new { message = "Không tìm thấy khách hàng" });

        if (await db.Customers.AnyAsync(x => x.Phone == dto.Phone && x.CustomerId != id))
            return Conflict(new { message = "Số điện thoại đã tồn tại" });
        if (await db.Customers.AnyAsync(x => x.IdentityNumber == dto.IdentityNumber && x.CustomerId != id))
            return Conflict(new { message = "CCCD/CMND đã tồn tại" });

        c.FullName = dto.FullName.Trim();
        c.Phone = dto.Phone;
        c.IdentityNumber = dto.IdentityNumber;
        c.DriverLicense = dto.DriverLicense;
        c.Address = dto.Address;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Customers.FindAsync(id);
        if (c == null) return NotFound(new { message = "Không tìm thấy khách hàng" });
        if (await db.Rentals.AnyAsync(r => r.CustomerId == id))
            return Conflict(new { message = "Khách hàng đã có lịch sử thuê, không thể xóa" });

        db.Customers.Remove(c);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static CustomerDTO ToDto(Customer c) => new()
    {
        CustomerId = c.CustomerId,
        FullName = c.FullName,
        Phone = c.Phone,
        IdentityNumber = c.IdentityNumber,
        DriverLicense = c.DriverLicense,
        Address = c.Address
    };
}
