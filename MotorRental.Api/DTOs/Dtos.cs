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
    public int? CustomerId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class RegisterDTO
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
    public string Password { get; set; } = null!;

    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "Họ và tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Số CCCD/CMND không được để trống")]
    [RegularExpression(@"^(\d{9}|\d{12})$", ErrorMessage = "Số CCCD phải gồm 9 hoặc 12 chữ số")]
    public string IdentityNumber { get; set; } = null!;

    public string? DriverLicense { get; set; }
    public string? Address { get; set; }
}

public class ForgotPasswordDTO
{
    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = null!;
}

public class VerifyOtpDTO
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mã OTP không được để trống")]
    public string Otp { get; set; } = null!;
}

public class ResetPasswordDTO
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mã OTP không được để trống")]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Mã OTP không hợp lệ")]
    public string Otp { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6 ký tự trở lên")]
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = null!;
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

    // Các thuộc tính mở rộng
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Color { get; set; }
    public int CurrentOdometer { get; set; }
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
    public int CurrentOdometer { get; set; } = 0;
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
    public int? UserId { get; set; }
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

    public int? UserId { get; set; }
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
    public string? MotorbikeImage { get; set; }
    public string? Category { get; set; }
    public string StaffName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }

    // Quản lý nghiệp vụ trả xe & phụ phí thực tế
    public int? OdometerOut { get; set; }
    public int? OdometerIn { get; set; }
    public decimal LateFee { get; set; }
    public decimal DamageFee { get; set; }
    public decimal OtherFee { get; set; }
    public decimal ActualPayment { get; set; }
    public string? ConditionNote { get; set; }

    // Thanh toán từ Khách hàng
    public string PaymentStatus { get; set; } = "Unpaid";
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
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

    public int? OdometerOut { get; set; }

    [StringLength(300, ErrorMessage = "Ghi chú tối đa 300 ký tự")]
    public string? Note { get; set; }

    public string PaymentStatus { get; set; } = "Unpaid";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("Ngày trả phải sau ngày thuê", new[] { nameof(EndDate) });
        if ((EndDate - StartDate).TotalDays > 90)
            yield return new ValidationResult("Thời gian thuê tối đa 90 ngày", new[] { nameof(EndDate) });
    }
}

public class ReturnRentalDTO
{
    public int? OdometerIn { get; set; }
    public decimal LateFee { get; set; } = 0;
    public decimal DamageFee { get; set; } = 0;
    public decimal OtherFee { get; set; } = 0;
    public string? ConditionNote { get; set; }
    public bool MarkPaid { get; set; } = true;
}

public class PayRentalDTO
{
    public string PaymentMethod { get; set; } = "VietQR";
    public string PaymentStatus { get; set; } = "FullyPaid"; // DepositPaid | FullyPaid
}

public class ExtendRentalDTO
{
    [Required(ErrorMessage = "Ngày kết thúc mới là bắt buộc")]
    public DateTime NewEndDate { get; set; }
}

public class RentalDetailDTO : RentalDTO
{
    public string CustomerPhone { get; set; } = "";
    public string IdentityNumber { get; set; } = "";
    public string? DriverLicense { get; set; }
    public string? CustomerAddress { get; set; }
}

// ==========================================
// 5. MAINTENANCE DTOs
// ==========================================
public class MaintenanceDTO
{
    public int MaintenanceId { get; set; }
    public int MotorbikeId { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string MotorbikeName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = null!;
    public decimal Cost { get; set; }
    public int? OdometerAtMaint { get; set; }
    public string Status { get; set; } = null!;
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }

    // Thanh toán chi phí cho xưởng sửa xe
    public string PaymentStatus { get; set; } = "Unpaid";
    public DateTime? PaidDate { get; set; }
    public string? PaidByStaff { get; set; }
    public string? TechnicianBankInfo { get; set; }
}

public class MaintenanceSaveDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn xe bảo dưỡng")]
    public int MotorbikeId { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; }

    [Required(ErrorMessage = "Nội dung bảo dưỡng là bắt buộc")]
    [StringLength(300, ErrorMessage = "Nội dung tối đa 300 ký tự")]
    public string Description { get; set; } = null!;

    [Range(0, 100000000, ErrorMessage = "Chi phí bảo dưỡng không hợp lệ")]
    public decimal Cost { get; set; }

    public int? OdometerAtMaint { get; set; }

    [RegularExpression("^(InProgress|Completed)$", ErrorMessage = "Trạng thái chỉ InProgress hoặc Completed")]
    public string Status { get; set; } = "Completed";

    public int? TechnicianId { get; set; }
    public string? TechnicianBankInfo { get; set; } = "Vietcombank 1014306428 - LE TIEN SON";
}

public class PayMaintenanceDTO
{
    public string? PaidByStaff { get; set; }
}

// ==========================================
// 6. USER DTOs
// ==========================================
public class UserDTO
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!; // "Admin" | "Staff" | "Customer" | "Technician"
    public bool IsActive { get; set; }
    public int? CustomerId { get; set; }
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

    [RegularExpression("^(Admin|Staff|Customer|Technician)$", ErrorMessage = "Vai trò phải là Admin, Staff, Customer hoặc Technician")]
    public string Role { get; set; } = "Staff";

    public int? CustomerId { get; set; }
}

// ==========================================
// 7. REPORT DTOs
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
    public decimal MaintenanceCost { get; set; }
    public decimal NetProfit => TotalRevenue - MaintenanceCost;
    public double FleetUtilizationRate => TotalMotorbikes > 0 ? Math.Round((double)Rented / TotalMotorbikes * 100, 1) : 0;
    public List<MonthRevenueDTO> Monthly { get; set; } = new();
}
