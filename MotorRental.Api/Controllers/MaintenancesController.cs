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
[Authorize(Roles = "Admin,Staff,Technician")]
public class MaintenancesController : ControllerBase
{
    private readonly MotorRentalContext db;
    public MaintenancesController(MotorRentalContext db) { this.db = db; }

    private int CurrentUserId
    {
        get
        {
            var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : 1;
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceDTO>>> GetAll([FromQuery] string? status)
    {
        var query = db.Maintenances.Include(m => m.Motorbike).Include(m => m.Technician).AsQueryable();
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
                Status = m.Status,
                TechnicianId = m.TechnicianId,
                TechnicianName = m.Technician != null ? m.Technician.FullName : "Xưởng Anh Tuấn Hòa Lạc",
                PaymentStatus = m.PaymentStatus,
                PaidDate = m.PaidDate,
                PaidByStaff = m.PaidByStaff,
                TechnicianBankInfo = m.TechnicianBankInfo
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceDTO>> Create(MaintenanceSaveDTO dto)
    {
        var bike = await db.Motorbikes.FindAsync(dto.MotorbikeId);
        if (bike == null) return NotFound(new { message = "Không tìm thấy xe" });

        int? techId = dto.TechnicianId;
        if (!techId.HasValue && User.IsInRole("Technician"))
        {
            techId = CurrentUserId;
        }

        var m = new Maintenance
        {
            MotorbikeId = dto.MotorbikeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description.Trim(),
            Cost = dto.Cost,
            OdometerAtMaint = dto.OdometerAtMaint ?? bike.CurrentOdometer,
            Status = dto.Status,
            TechnicianId = techId,
            TechnicianBankInfo = dto.TechnicianBankInfo ?? "Vietcombank 1014306428 - LE TIEN SON",
            PaymentStatus = "Unpaid"
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
            Status = m.Status,
            TechnicianId = m.TechnicianId,
            PaymentStatus = m.PaymentStatus,
            TechnicianBankInfo = m.TechnicianBankInfo
        };

        return CreatedAtAction(nameof(GetAll), new { id = m.MaintenanceId }, res);
    }

    /// <summary>
    /// Kỹ thuật viên / Xưởng xác nhận hoàn tất sửa chữa
    /// </summary>
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

    /// <summary>
    /// Cửa hàng / Nhân viên thanh toán tiền sửa chữa cho Bên đối tác bảo trì
    /// POST /api/maintenances/{id}/pay
    /// </summary>
    [HttpPost("{id:int}/pay")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> PayMaintenance(int id, [FromBody] PayMaintenanceDTO? dto = null)
    {
        var m = await db.Maintenances.FindAsync(id);
        if (m == null) return NotFound(new { message = "Không tìm thấy hồ sơ bảo dưỡng" });

        var staffName = dto?.PaidByStaff ?? User.FindFirstValue("fullName") ?? "Nhân viên Showroom";

        m.PaymentStatus = "Paid";
        m.PaidDate = DateTime.Now;
        m.PaidByStaff = staffName;

        await db.SaveChangesAsync();
        return Ok(new { message = $"Đã xác nhận thanh toán chi phí bảo dưỡng {m.Cost:N0} ₫ cho xưởng kỹ thuật!" });
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
