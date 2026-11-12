using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Data;
using MotorRental.Api.DTOs;
using MotorRental.Api.Models;

namespace MotorRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class MaintenancesController : ControllerBase
{
    private readonly MotorRentalContext db;
    public MaintenancesController(MotorRentalContext db) { this.db = db; }

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceDTO>>> GetAll([FromQuery] string? status)
    {
        var query = db.Maintenances.Include(m => m.Motorbike).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        var list = await query
            .OrderByDescending(m => m.MaintenanceId)
            .Select(m => new MaintenanceDTO
            {
                MaintenanceId = m.MaintenanceId,
                MotorbikeId = m.MotorbikeId,
                LicensePlate = m.Motorbike.LicensePlate,
                MotorbikeName = m.Motorbike.Name,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Description = m.Description,
                Cost = m.Cost,
                OdometerAtMaint = m.OdometerAtMaint,
                Status = m.Status
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceDTO>> Create(MaintenanceSaveDTO dto)
    {
        var bike = await db.Motorbikes.FindAsync(dto.MotorbikeId);
        if (bike == null) return NotFound(new { message = "Không tìm thấy xe" });

        var m = new Maintenance
        {
            MotorbikeId = dto.MotorbikeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description.Trim(),
            Cost = dto.Cost,
            OdometerAtMaint = dto.OdometerAtMaint ?? bike.CurrentOdometer,
            Status = dto.Status
        };

        if (dto.Status == "InProgress")
        {
            bike.Status = "Maintenance";
        }

        db.Maintenances.Add(m);
        await db.SaveChangesAsync();

        var res = new MaintenanceDTO
        {
            MaintenanceId = m.MaintenanceId,
            MotorbikeId = m.MotorbikeId,
            LicensePlate = bike.LicensePlate,
            MotorbikeName = bike.Name,
            StartDate = m.StartDate,
            EndDate = m.EndDate,
            Description = m.Description,
            Cost = m.Cost,
            OdometerAtMaint = m.OdometerAtMaint,
            Status = m.Status
        };

        return CreatedAtAction(nameof(GetAll), new { id = m.MaintenanceId }, res);
    }

    [HttpPost("{id:int}/finish")]
    public async Task<IActionResult> Finish(int id)
    {
        var m = await db.Maintenances.Include(x => x.Motorbike).FirstOrDefaultAsync(x => x.MaintenanceId == id);
        if (m == null) return NotFound(new { message = "Không tìm thấy hồ sơ bảo dưỡng" });

        m.Status = "Completed";
        m.EndDate = DateTime.Now;

        if (m.Motorbike.Status == "Maintenance")
        {
            m.Motorbike.Status = "Available";
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await db.Maintenances.FindAsync(id);
        if (m == null) return NotFound(new { message = "Không tìm thấy hồ sơ bảo dưỡng" });

        db.Maintenances.Remove(m);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
