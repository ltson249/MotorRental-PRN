using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Data;
using MotorRental.Api.DTOs;
using MotorRental.Api.Models;

namespace MotorRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]   // Admin + Staff deu xem duoc; Them/Sua/Xoa chi Admin
public class MotorbikesController : ControllerBase
{
    private readonly MotorRentalContext db;
    public MotorbikesController(MotorRentalContext db) { this.db = db; }

    // GET api/motorbikes?$filter=Status eq 'Available'&$orderby=Name   (OData)
    [HttpGet]
    [EnableQuery]
    public IQueryable<MotorbikeDTO> Get() =>
        db.Motorbikes.Select(m => new MotorbikeDTO
        {
            MotorbikeId = m.MotorbikeId,
            LicensePlate = m.LicensePlate,
            Name = m.Name,
            Brand = m.Brand,
            EngineCapacity = m.EngineCapacity,
            PricePerDay = m.PricePerDay,
            Status = m.Status,
            ImageUrl = m.ImageUrl,
            Category = m.Category,
            Color = m.Color,
            CurrentOdometer = m.CurrentOdometer
        });

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MotorbikeDTO>> GetById(int id)
    {
        var m = await db.Motorbikes.FindAsync(id);
        return m == null ? NotFound(new { message = "Không tìm thấy xe" }) : Ok(ToDto(m));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MotorbikeDTO>> Create(MotorbikeSaveDTO dto)
    {
        var plate = dto.LicensePlate.Trim().ToUpper();
        if (await db.Motorbikes.AnyAsync(m => m.LicensePlate == plate))
            return Conflict(new { message = $"Biển số {plate} đã tồn tại" });

        var m = new Motorbike
        {
            LicensePlate = plate,
            Name = dto.Name.Trim(),
            Brand = dto.Brand.Trim(),
            EngineCapacity = dto.EngineCapacity,
            PricePerDay = dto.PricePerDay,
            Status = dto.Status,
            ImageUrl = dto.ImageUrl?.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Xe số" : dto.Category.Trim(),
            Color = dto.Color?.Trim(),
            CurrentOdometer = dto.CurrentOdometer
        };
        db.Motorbikes.Add(m);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = m.MotorbikeId }, ToDto(m));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, MotorbikeSaveDTO dto)
    {
        var m = await db.Motorbikes.FindAsync(id);
        if (m == null) return NotFound(new { message = "Không tìm thấy xe" });

        var plate = dto.LicensePlate.Trim().ToUpper();
        if (await db.Motorbikes.AnyAsync(x => x.LicensePlate == plate && x.MotorbikeId != id))
            return Conflict(new { message = $"Biển số {plate} đã tồn tại" });

        m.LicensePlate = plate;
        m.Name = dto.Name.Trim();
        m.Brand = dto.Brand.Trim();
        m.EngineCapacity = dto.EngineCapacity;
        m.PricePerDay = dto.PricePerDay;
        m.ImageUrl = dto.ImageUrl?.Trim();
        m.Category = string.IsNullOrWhiteSpace(dto.Category) ? "Xe số" : dto.Category.Trim();
        m.Color = dto.Color?.Trim();
        m.CurrentOdometer = dto.CurrentOdometer;

        if (m.Status != "Rented") m.Status = dto.Status;   // xe đang thuê thì không tự đổi trạng thái
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await db.Motorbikes.FindAsync(id);
        if (m == null) return NotFound(new { message = "Không tìm thấy xe" });
        if (await db.Rentals.AnyAsync(r => r.MotorbikeId == id))
            return Conflict(new { message = "Xe đã có lịch sử thuê, không thể xóa (hãy chuyển sang Bảo trì)" });

        db.Motorbikes.Remove(m);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static MotorbikeDTO ToDto(Motorbike m) => new()
    {
        MotorbikeId = m.MotorbikeId,
        LicensePlate = m.LicensePlate,
        Name = m.Name,
        Brand = m.Brand,
        EngineCapacity = m.EngineCapacity,
        PricePerDay = m.PricePerDay,
        Status = m.Status,
        ImageUrl = m.ImageUrl,
        Category = m.Category,
        Color = m.Color,
        CurrentOdometer = m.CurrentOdometer
    };
}
