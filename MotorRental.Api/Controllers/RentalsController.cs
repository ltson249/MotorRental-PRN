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
public class RentalsController : ControllerBase
{
    private readonly MotorRentalContext db;
    public RentalsController(MotorRentalContext db) { this.db = db; }

    private int CurrentUserId
    {
        get
        {
            var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : 1;
        }
    }

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
            ConditionNote = r.ConditionNote,
            PaymentStatus = r.PaymentStatus,
            PaymentMethod = r.PaymentMethod,
            PaymentDate = r.PaymentDate
        });

    private static decimal Calc(decimal pricePerDay, DateTime start, DateTime end)
    {
        if (end <= start) return pricePerDay;
        var days = (int)Math.Ceiling((end - start).TotalDays);
        return pricePerDay * Math.Max(days, 1);
    }

    /// <summary>
    /// Lấy danh sách phiếu thuê (Admin, Staff)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
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
    /// Lấy danh sách phiếu thuê của chính khách hàng đang đăng nhập (Role: Customer)
    /// </summary>
    [HttpGet("my-rentals")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<List<RentalDTO>>> GetMyRentals()
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == CurrentUserId);
        if (customer == null)
        {
            return Ok(new List<RentalDTO>());
        }

        var list = await Project()
            .Where(r => r.CustomerId == customer.CustomerId)
            .OrderByDescending(r => r.RentalId)
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// Lấy chi tiết 1 phiếu thuê
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<ActionResult<RentalDTO>> GetById(int id)
    {
        var r = await Project().FirstOrDefaultAsync(x => x.RentalId == id);
        if (r == null) return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        if (User.IsInRole("Customer"))
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == CurrentUserId);
            if (customer == null || r.CustomerId != customer.CustomerId)
                return Forbid();
        }

        return Ok(r);
    }

    /// <summary>
    /// Lấy chi tiết đầy đủ phục vụ In Hợp Đồng / Biên Nhận
    /// </summary>
    [HttpGet("{id:int}/detail")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<ActionResult<RentalDetailDTO>> GetDetailForPrint(int id)
    {
        var r = await db.Rentals
            .Include(x => x.Customer)
            .Include(x => x.Motorbike)
            .Include(x => x.Staff)
            .FirstOrDefaultAsync(x => x.RentalId == id);

        if (r == null) return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        if (User.IsInRole("Customer"))
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == CurrentUserId);
            if (customer == null || r.CustomerId != customer.CustomerId)
                return Forbid();
        }

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
            ConditionNote = r.ConditionNote,
            PaymentStatus = r.PaymentStatus,
            PaymentMethod = r.PaymentMethod,
            PaymentDate = r.PaymentDate
        };

        return Ok(detail);
    }

    /// <summary>
    /// Tạo phiếu thuê mới (Staff, Admin lập hợp đồng; Customer đặt thuê online)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<ActionResult<RentalDTO>> Create(CreateRentalDTO dto)
    {
        var bike = await db.Motorbikes.FindAsync(dto.MotorbikeId);
        if (bike == null)
            return NotFound(new { message = "Không tìm thấy xe" });

        if (bike.Status != "Available")
            return Conflict(new { message = $"Xe {bike.LicensePlate} đang ở trạng thái '{bike.Status}', không thể cho thuê" });

        int handlingUserId;
        int targetCustomerId;

        if (User.IsInRole("Customer"))
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == CurrentUserId);
            if (customer == null)
            {
                return BadRequest(new { message = "Không tìm thấy thông tin khách hàng liên kết với tài khoản này" });
            }
            targetCustomerId = customer.CustomerId;

            // Gán nhân viên showroom phụ trách giao nhận
            var staffUser = await db.Users.FirstOrDefaultAsync(u => u.Role == "Staff" && u.IsActive)
                         ?? await db.Users.FirstOrDefaultAsync(u => u.Role == "Admin" && u.IsActive);
            handlingUserId = staffUser?.UserId ?? CurrentUserId;
        }
        else
        {
            if (!await db.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId))
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });

            targetCustomerId = dto.CustomerId;
            handlingUserId = CurrentUserId;
        }

        var odoOut = dto.OdometerOut ?? (bike.CurrentOdometer > 0 ? bike.CurrentOdometer : 0);
        if (bike.CurrentOdometer == 0 && odoOut > 0)
        {
            bike.CurrentOdometer = odoOut;
        }

        var baseTotal = Calc(bike.PricePerDay, dto.StartDate, dto.EndDate);
        var rental = new Rental
        {
            CustomerId = targetCustomerId,
            MotorbikeId = dto.MotorbikeId,
            UserId = handlingUserId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PricePerDay = bike.PricePerDay,
            Deposit = dto.Deposit,
            TotalPrice = baseTotal,
            Status = "Active",
            Note = User.IsInRole("Customer")
                ? (string.IsNullOrWhiteSpace(dto.Note) ? "Khách hàng đặt thuê trực tuyến qua Web Portal ĐH FPT" : $"Khách đặt online: {dto.Note}")
                : dto.Note,
            OdometerOut = odoOut,
            ActualPayment = Math.Max(baseTotal - dto.Deposit, 0),
            PaymentStatus = dto.Deposit > 0 ? "DepositPaid" : "Unpaid",
            PaymentMethod = "VietQR",
            PaymentDate = DateTime.Now
        };

        bike.Status = "Rented";
        db.Rentals.Add(rental);
        await db.SaveChangesAsync();

        var result = await Project().FirstAsync(x => x.RentalId == rental.RentalId);
        return CreatedAtAction(nameof(GetById), new { id = rental.RentalId }, result);
    }

    /// <summary>
    /// Xác nhận thanh toán từ khách hàng (VietQR / Tiền mặt)
    /// POST /api/rentals/{id}/pay
    /// </summary>
    [HttpPost("{id:int}/pay")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<ActionResult<RentalDTO>> Pay(int id, [FromBody] PayRentalDTO dto)
    {
        var r = await db.Rentals.FindAsync(id);
        if (r == null) return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        if (User.IsInRole("Customer"))
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == CurrentUserId);
            if (customer == null || r.CustomerId != customer.CustomerId)
                return Forbid();
        }

        r.PaymentStatus = dto.PaymentStatus;
        r.PaymentMethod = dto.PaymentMethod;
        r.PaymentDate = DateTime.Now;

        await db.SaveChangesAsync();
        return Ok(await Project().FirstAsync(x => x.RentalId == id));
    }

    /// <summary>
    /// Nhận trả xe và thanh toán quyết toán hợp đồng
    /// </summary>
    [HttpPost("{id:int}/return")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<RentalDTO>> Return(int id, [FromBody] ReturnRentalDTO? dto = null)
    {
        var r = await db.Rentals.Include(x => x.Motorbike).FirstOrDefaultAsync(x => x.RentalId == id);
        if (r == null)
            return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        if (r.Status != "Active")
            return Conflict(new { message = "Phiếu thuê này không ở trạng thái Đang thuê" });

        var now = DateTime.Now;
        r.ReturnDate = now;

        var billEnd = now > r.EndDate ? now : r.EndDate;
        var rentalFee = Calc(r.PricePerDay, r.StartDate, billEnd);

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

            if (dto.MarkPaid)
            {
                r.PaymentStatus = "FullyPaid";
                r.PaymentDate = now;
            }
        }

        r.TotalPrice = rentalFee + extraFees;
        r.ActualPayment = r.TotalPrice - r.Deposit;
        r.Status = "Completed";
        r.Motorbike.Status = "Available";

        await db.SaveChangesAsync();

        return Ok(await Project().FirstAsync(x => x.RentalId == id));
    }

    /// <summary>
    /// Gia hạn thời gian thuê xe
    /// </summary>
    [HttpPost("{id:int}/extend")]
    [Authorize(Roles = "Admin,Staff")]
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
    /// Hủy phiếu thuê
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<RentalDTO>> Cancel(int id)
    {
        var r = await db.Rentals.Include(x => x.Motorbike).FirstOrDefaultAsync(x => x.RentalId == id);
        if (r == null)
            return NotFound(new { message = "Không tìm thấy phiếu thuê" });

        if (r.Status != "Active")
            return Conflict(new { message = "Chỉ có thể hủy phiếu thuê đang ở trạng thái Đang thuê" });

        r.Status = "Cancelled";
        r.TotalPrice = 0;
        r.ActualPayment = 0;
        r.Motorbike.Status = "Available";

        await db.SaveChangesAsync();

        return Ok(await Project().FirstAsync(x => x.RentalId == id));
    }
}
