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
[Authorize(Roles = "Admin,Staff")]
public class RentalsController : ControllerBase
{
    private readonly MotorRentalContext db;
    public RentalsController(MotorRentalContext db) { this.db = db; }

    // Lấy UserId của nhân viên đang đăng nhập (có fallback an toàn)
    private int CurrentUserId
    {
        get
        {
            var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : 1;
        }
    }

    // Mapping sang DTO hiển thị
    private IQueryable<RentalDTO> Project() =>
        db.Rentals.Select(r => new RentalDTO
        {
            RentalId = r.RentalId,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer.FullName,
            MotorbikeId = r.MotorbikeId,
            LicensePlate = r.Motorbike.LicensePlate,
            MotorbikeName = r.Motorbike.Name,
            MotorbikeImage = r.Motorbike.ImageUrl,
            Category = r.Motorbike.Category,
            StaffName = r.Staff.FullName,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            ReturnDate = r.ReturnDate,
            PricePerDay = r.PricePerDay,
            Deposit = r.Deposit,
            TotalPrice = r.TotalPrice,
            Status = r.Status,
            Note = r.Note,
            OdometerOut = r.OdometerOut,
            OdometerIn = r.OdometerIn,
            LateFee = r.LateFee,
            DamageFee = r.DamageFee,
            OtherFee = r.OtherFee,
            ActualPayment = r.ActualPayment,
            ConditionNote = r.ConditionNote
        });

    // Tính tiền thuê: Làm tròn lên theo ngày, tối thiểu 1 ngày
    private static decimal Calc(decimal pricePerDay, DateTime start, DateTime end)
    {
        if (end <= start) return pricePerDay;
        var days = (int)Math.Ceiling((end - start).TotalDays);
        return pricePerDay * Math.Max(days, 1);
    }

    /// <summary>
    /// Lấy danh sách phiếu thuê (có hỗ trợ lọc theo trạng thái, từ khóa, quá hạn)
    /// GET /api/rentals?status=Active&q=wave&overdueOnly=true
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RentalDTO>>> GetAll([FromQuery] string? status, [FromQuery] string? q, [FromQuery] bool overdueOnly = false)
    {
        var query = Project();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(r =>
                r.CustomerName.Contains(q) ||
                r.LicensePlate.Contains(q) ||
                r.MotorbikeName.Contains(q));
        }

        if (overdueOnly)
        {
            var now = DateTime.Now;
            query = query.Where(r => r.Status == "Active" && r.EndDate < now);
        }

        var list = await query.OrderByDescending(r => r.RentalId).ToListAsync();
        return Ok(list);
    }

    /// <summary>
    /// Lấy chi tiết 1 phiếu thuê
    /// GET /api/rentals/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RentalDTO>> GetById(int id)
    {
        var r = await Project().FirstOrDefaultAsync(x => x.RentalId == id);
        return r == null ? NotFound(new { message = "Không tìm thấy phiếu thuê" }) : Ok(r);
    }

    /// <summary>
    /// Lấy chi tiết đầy đủ phục vụ In Hợp Đồng / Biên Nhận
    /// GET /api/rentals/{id}/detail
    /// </summary>
    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<RentalDetailDTO>> GetDetailForPrint(int id)
    {
        var r = await db.Rentals
            .Include(x => x.Customer)
            .Include(x => x.Motorbike)
            .Include(x => x.Staff)
            .FirstOrDefaultAsync(x => x.RentalId == id);

        if (r == null) return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        var detail = new RentalDetailDTO
        {
            RentalId = r.RentalId,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer.FullName,
            CustomerPhone = r.Customer.Phone,
            IdentityNumber = r.Customer.IdentityNumber,
            DriverLicense = r.Customer.DriverLicense,
            CustomerAddress = r.Customer.Address,
            MotorbikeId = r.MotorbikeId,
            LicensePlate = r.Motorbike.LicensePlate,
            MotorbikeName = r.Motorbike.Name,
            MotorbikeImage = r.Motorbike.ImageUrl,
            Category = r.Motorbike.Category,
            StaffName = r.Staff.FullName,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            ReturnDate = r.ReturnDate,
            PricePerDay = r.PricePerDay,
            Deposit = r.Deposit,
            TotalPrice = r.TotalPrice,
            Status = r.Status,
            Note = r.Note,
            OdometerOut = r.OdometerOut,
            OdometerIn = r.OdometerIn,
            LateFee = r.LateFee,
            DamageFee = r.DamageFee,
            OtherFee = r.OtherFee,
            ActualPayment = r.ActualPayment,
            ConditionNote = r.ConditionNote
        };

        return Ok(detail);
    }

    /// <summary>
    /// Tạo phiếu thuê mới
    /// POST /api/rentals
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RentalDTO>> Create(CreateRentalDTO dto)
    {
        var bike = await db.Motorbikes.FindAsync(dto.MotorbikeId);
        if (bike == null)
            return NotFound(new { message = "Không tìm thấy xe" });

        if (bike.Status != "Available")
            return Conflict(new { message = $"Xe {bike.LicensePlate} đang ở trạng thái '{bike.Status}', không thể cho thuê" });

        if (!await db.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId))
            return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });

        // Tự động lấy OdometerOut nếu không nhập
        var odoOut = dto.OdometerOut ?? (bike.CurrentOdometer > 0 ? bike.CurrentOdometer : 0);
        if (bike.CurrentOdometer == 0 && odoOut > 0)
        {
            bike.CurrentOdometer = odoOut;
        }

        var baseTotal = Calc(bike.PricePerDay, dto.StartDate, dto.EndDate);
        var rental = new Rental
        {
            CustomerId = dto.CustomerId,
            MotorbikeId = dto.MotorbikeId,
            UserId = CurrentUserId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PricePerDay = bike.PricePerDay,
            Deposit = dto.Deposit,
            TotalPrice = baseTotal,
            Status = "Active",
            Note = dto.Note,
            OdometerOut = odoOut,
            ActualPayment = Math.Max(baseTotal - dto.Deposit, 0)
        };

        // Chuyển trạng thái xe sang đang thuê
        bike.Status = "Rented";
        db.Rentals.Add(rental);
        await db.SaveChangesAsync();

        var result = await Project().FirstAsync(x => x.RentalId == rental.RentalId);
        return CreatedAtAction(nameof(GetById), new { id = rental.RentalId }, result);
    }

    /// <summary>
    /// Nhận trả xe và thanh toán quyết toán hợp đồng
    /// POST /api/rentals/{id}/return
    /// </summary>
    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<RentalDTO>> Return(int id, [FromBody] ReturnRentalDTO? dto = null)
    {
        var r = await db.Rentals.Include(x => x.Motorbike).FirstOrDefaultAsync(x => x.RentalId == id);
        if (r == null)
            return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        if (r.Status != "Active")
            return Conflict(new { message = "Phiếu thuê này không ở trạng thái Đang thuê (có thể đã trả hoặc đã hủy)" });

        var now = DateTime.Now;
        r.ReturnDate = now;

        // 1. Tính tiền ngày thuê (nếu trả quá hạn thì tính tới thời điểm trả thực tế)
        var billEnd = now > r.EndDate ? now : r.EndDate;
        var rentalFee = Calc(r.PricePerDay, r.StartDate, billEnd);

        // 2. Tính các khoản phụ thu nếu có dữ liệu gửi lên
        decimal extraFees = 0;
        if (dto != null)
        {
            r.LateFee = Math.Max(dto.LateFee, 0);
            r.DamageFee = Math.Max(dto.DamageFee, 0);
            r.OtherFee = Math.Max(dto.OtherFee, 0);
            extraFees = r.LateFee + r.DamageFee + r.OtherFee;

            if (dto.OdometerIn.HasValue && dto.OdometerIn.Value > 0)
            {
                r.OdometerIn = dto.OdometerIn.Value;
                if (dto.OdometerIn.Value > r.Motorbike.CurrentOdometer)
                {
                    r.Motorbike.CurrentOdometer = dto.OdometerIn.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.ConditionNote))
            {
                r.ConditionNote = dto.ConditionNote.Trim();
                r.Note = string.IsNullOrWhiteSpace(r.Note)
                    ? $"[Trả xe]: {dto.ConditionNote}"
                    : $"{r.Note} | [Trả xe]: {dto.ConditionNote}";
            }
        }

        // 3. Tổng chi phí cuối cùng của phiếu thuê và số tiền khách thực trả/cửa hàng hoàn trả
        r.TotalPrice = rentalFee + extraFees;
        r.ActualPayment = r.TotalPrice - r.Deposit;
        r.Status = "Completed";

        // 4. Giải phóng xe để tiếp tục phục vụ khách khác
        r.Motorbike.Status = "Available";

        await db.SaveChangesAsync();

        return Ok(await Project().FirstAsync(x => x.RentalId == id));
    }

    /// <summary>
    /// Gia hạn thời gian thuê xe
    /// POST /api/rentals/{id}/extend
    /// </summary>
    [HttpPost("{id:int}/extend")]
    public async Task<ActionResult<RentalDTO>> Extend(int id, [FromBody] ExtendRentalDTO dto)
    {
        var r = await db.Rentals.Include(x => x.Motorbike).FirstOrDefaultAsync(x => x.RentalId == id);
        if (r == null) return NotFound(new { message = "Không tìm thấy phiếu thuê" });
        if (r.Status != "Active") return Conflict(new { message = "Chỉ có thể gia hạn phiếu thuê đang hoạt động" });

        if (dto.NewEndDate <= r.EndDate)
            return BadRequest(new { message = "Ngày gia hạn mới phải sau ngày trả hiện tại" });

        r.EndDate = dto.NewEndDate;
        r.TotalPrice = Calc(r.PricePerDay, r.StartDate, r.EndDate);
        r.ActualPayment = r.TotalPrice - r.Deposit;
        r.Note = string.IsNullOrWhiteSpace(r.Note) 
            ? $"[Gia hạn đến {dto.NewEndDate:dd/MM/yyyy HH:mm}]" 
            : $"{r.Note} | [Gia hạn đến {dto.NewEndDate:dd/MM/yyyy HH:mm}]";

        await db.SaveChangesAsync();
        return Ok(await Project().FirstAsync(x => x.RentalId == id));
    }

    /// <summary>
    /// Hủy phiếu thuê (Chỉ áp dụng khi phiếu chưa bắt đầu hoặc khách đổi ý)
    /// POST /api/rentals/{id}/cancel
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<RentalDTO>> Cancel(int id)
    {
        var r = await db.Rentals.Include(x => x.Motorbike).FirstOrDefaultAsync(x => x.RentalId == id);
        if (r == null)
            return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        if (r.Status != "Active")
            return Conflict(new { message = "Chỉ có thể hủy phiếu thuê đang ở trạng thái Đang thuê" });

        r.Status = "Cancelled";
        r.TotalPrice = 0; // Đã hủy hợp đồng
        r.ActualPayment = 0;
        r.Motorbike.Status = "Available"; // Trả trạng thái xe về sẵn sàng

        await db.SaveChangesAsync();

        return Ok(await Project().FirstAsync(x => x.RentalId == id));
    }
}
