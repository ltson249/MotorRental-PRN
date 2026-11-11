using System.ComponentModel.DataAnnotations;

namespace MotorRental.Api.DTOs;

// ==========================================
// 1. AUTH DTOs
// ==========================================
public class LoginDTO
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    public string Password { get; set; } = null!;
}

public class LoginResponseDTO
{
    public string Token { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

// ==========================================
// 2. MOTORBIKE DTOs
// ==========================================
public class MotorbikeDTO
{
    public int MotorbikeId { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public int EngineCapacity { get; set; }
    public decimal PricePerDay { get; set; }
    public string Status { get; set; } = null!;

    // Các thuộc tính mở rộng (không bắt buộc)
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Color { get; set; }
}

public class MotorbikeSaveDTO
{
    [Required(ErrorMessage = "Biển số là bắt buộc")]
    [RegularExpression(@"^[0-9A-Za-z\-\. ]{5,15}$", ErrorMessage = "Biển số không hợp lệ (5-15 ký tự: chữ, số, - . )")]
    public string LicensePlate { get; set; } = null!;

    [Required(ErrorMessage = "Tên xe là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên xe tối đa 100 ký tự")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Hãng xe là bắt buộc")]
    [StringLength(50, ErrorMessage = "Hãng xe tối đa 50 ký tự")]
    public string Brand { get; set; } = null!;

    [Range(50, 1000, ErrorMessage = "Dung tích xi lanh từ 50 đến 1000 cc")]
    public int EngineCapacity { get; set; }

    [Range(10000, 10000000, ErrorMessage = "Giá thuê/ngày từ 10.000 đến 10.000.000")]
    public decimal PricePerDay { get; set; }

    [RegularExpression("^(Available|Maintenance)$", ErrorMessage = "Trạng thái chỉ được là Available hoặc Maintenance")]
    public string Status { get; set; } = "Available";

    public string? ImageUrl { get; set; }
    public string? Category { get; set; } = "Xe số";
    public string? Color { get; set; }
}

// ==========================================
// 3. CUSTOMER DTOs
// ==========================================
public class CustomerDTO
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string IdentityNumber { get; set; } = null!;
    public string? DriverLicense { get; set; }
    public string? Address { get; set; }
}

public class CustomerSaveDTO
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số và bắt đầu bằng 0")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "CCCD/CMND là bắt buộc")]
    [RegularExpression(@"^(\d{9}|\d{12})$", ErrorMessage = "CCCD/CMND gồm 9 hoặc 12 số")]
    public string IdentityNumber { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Số GPLX tối đa 20 ký tự")]
    public string? DriverLicense { get; set; }

    [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
    public string? Address { get; set; }
}

// ==========================================
// 4. RENTAL DTOs
// ==========================================
public class RentalDTO
{
    public int RentalId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int MotorbikeId { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string MotorbikeName { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
}

public class CreateRentalDTO : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn khách hàng")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn xe")]
    public int MotorbikeId { get; set; }

    [Required(ErrorMessage = "Ngày thuê là bắt buộc")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Ngày trả dự kiến là bắt buộc")]
    public DateTime EndDate { get; set; }

    [Range(0, 100000000, ErrorMessage = "Tiền cọc không hợp lệ")]
    public decimal Deposit { get; set; }

    [StringLength(300, ErrorMessage = "Ghi chú tối đa 300 ký tự")]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("Ngày trả phải sau ngày thuê", new[] { nameof(EndDate) });
        if ((EndDate - StartDate).TotalDays > 90)
            yield return new ValidationResult("Thời gian thuê tối đa 90 ngày", new[] { nameof(EndDate) });
    }
}

// DTO tiếp nhận dữ liệu khi nhận xe trả lại & quyết toán phụ phí
public class ReturnRentalDTO
{
    public int? OdometerIn { get; set; }            // Số km lúc trả xe
    public decimal LateFee { get; set; } = 0;        // Phí trễ hạn
    public decimal DamageFee { get; set; } = 0;      // Phí hỏng hóc / trầy xước / nón bảo hiểm
    public decimal OtherFee { get; set; } = 0;       // Phụ thu khác (xăng, rửa xe)
    public string? ConditionNote { get; set; }      // Ghi chú hiện trạng xe khi nhận lại
}

// DTO chi tiết phục vụ in hợp đồng / biên nhận
public class RentalDetailDTO : RentalDTO
{
    public string CustomerPhone { get; set; } = "";
    public string IdentityNumber { get; set; } = "";
    public string? DriverLicense { get; set; }
    public string? CustomerAddress { get; set; }
}

// ==========================================
// 5. USER DTOs
// ==========================================
public class UserDTO
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateUserDTO
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập từ 4 đến 50 ký tự")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 ký tự")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [RegularExpression("^(Admin|Staff)$", ErrorMessage = "Vai trò chỉ được là Admin hoặc Staff")]
    public string Role { get; set; } = "Staff";
}

// ==========================================
// 6. REPORT DTOs
// ==========================================
public class MonthRevenueDTO
{
    public string Month { get; set; } = null!;
    public decimal Revenue { get; set; }
}

public class SummaryDTO
{
    public int TotalMotorbikes { get; set; }
    public int Available { get; set; }
    public int Rented { get; set; }
    public int Maintenance { get; set; }
    public int ActiveRentals { get; set; }
    public int TotalCustomers { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<MonthRevenueDTO> Monthly { get; set; } = new();
}