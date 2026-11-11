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
            StaffName = r.Staff.FullName,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            ReturnDate = r.ReturnDate,
            PricePerDay = r.PricePerDay,
            Deposit = r.Deposit,
            TotalPrice = r.TotalPrice,
            Status = r.Status,
            Note = r.Note
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
    public async Task<ActionResult<IEnumerable<RentalDTO>>> Get(
        [FromQuery] string? status,
        [FromQuery] string? q,
        [FromQuery] bool? overdueOnly,
        [FromQuery] int? customerId,
        [FromQuery] int? motorbikeId)
    {
        var query = db.Rentals.AsQueryable();

        // 1. Lọc theo trạng thái (Active, Completed, Cancelled)
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        // 2. Lọc các phiếu đang thuê nhưng đã QUÁ HẠN trả (EndDate < hiện tại)
        if (overdueOnly == true)
        {
            var now = DateTime.Now;
            query = query.Where(r => r.Status == "Active" && r.EndDate < now);
        }

        // 3. Lọc theo khách hàng hoặc xe cụ thể
        if (customerId.HasValue) query = query.Where(r => r.CustomerId == customerId.Value);
        if (motorbikeId.HasValue) query = query.Where(r => r.MotorbikeId == motorbikeId.Value);

        // 4. Tìm kiếm từ khóa (tên khách, SĐT, biển số xe)
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim().ToLower();
            query = query.Where(r =>
                r.Customer.FullName.ToLower().Contains(q) ||
                r.Customer.Phone.Contains(q) ||
                r.Motorbike.LicensePlate.ToLower().Contains(q) ||
                r.Motorbike.Name.ToLower().Contains(q));
        }

        var result = await query
            .OrderByDescending(r => r.RentalId)
            .Select(r => new RentalDTO
            {
                RentalId = r.RentalId,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName,
                MotorbikeId = r.MotorbikeId,
                LicensePlate = r.Motorbike.LicensePlate,
                MotorbikeName = r.Motorbike.Name,
                StaffName = r.Staff.FullName,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                ReturnDate = r.ReturnDate,
                PricePerDay = r.PricePerDay,
                Deposit = r.Deposit,
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                Note = r.Note
            })
            .ToListAsync();

        return Ok(result);
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
    /// Tạo mới phiếu thuê xe
    /// POST /api/rentals
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RentalDTO>> Create(CreateRentalDTO dto)
    {
        var customer = await db.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });

        var bike = await db.Motorbikes.FindAsync(dto.MotorbikeId);
        if (bike == null)
            return NotFound(new { message = "Không tìm thấy xe yêu cầu" });

        if (bike.Status != "Available")
            return Conflict(new { message = $"Xe {bike.Name} ({bike.LicensePlate}) hiện không sẵn sàng để cho thuê" });

        // Tạm tính giá tiền
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
            Note = dto.Note
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
    /// Hỗ trợ cả trường hợp gọi không có body (tương thích cũ) hoặc có body (phụ phí, số km)
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
            extraFees = Math.Max(dto.LateFee, 0) + Math.Max(dto.DamageFee, 0) + Math.Max(dto.OtherFee, 0);

            // Nối thêm thông tin kiểm tra hiện trạng vào Note nếu có
            if (!string.IsNullOrWhiteSpace(dto.ConditionNote))
            {
                r.Note = string.IsNullOrWhiteSpace(r.Note)
                    ? $"[Trả xe]: {dto.ConditionNote}"
                    : $"{r.Note} | [Trả xe]: {dto.ConditionNote}";
            }
        }

        // 3. Tổng chi phí cuối cùng của phiếu thuê
        r.TotalPrice = rentalFee + extraFees;
        r.Status = "Completed";

        // 4. Giải phóng xe để tiếp tục phục vụ khách khác
        r.Motorbike.Status = "Available";

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
        r.Motorbike.Status = "Available"; // Trả trạng thái xe về sẵn sàng

        await db.SaveChangesAsync();

        return Ok(await Project().FirstAsync(x => x.RentalId == id));
    }
}