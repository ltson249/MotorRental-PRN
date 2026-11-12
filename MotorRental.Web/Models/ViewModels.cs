using System.ComponentModel.DataAnnotations;

namespace MotorRental.Web.Models;

public class LoginVm
{
    [Required(ErrorMessage = "Nhập tên đăng nhập")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public int? CustomerId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class RegisterVm
{
    [Required(ErrorMessage = "Nhập tên đăng nhập")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 ký tự trở lên")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Xác nhận mật khẩu")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Nhập họ và tên")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Nhập địa chỉ Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [Display(Name = "Địa chỉ Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 số (ví dụ: 0981123456)")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Nhập số CCCD/CMND")]
    [RegularExpression(@"^(\d{9}|\d{12})$", ErrorMessage = "Số CCCD phải gồm 9 hoặc 12 số")]
    [Display(Name = "Số CCCD / CMND")]
    public string IdentityNumber { get; set; } = "";

    [Display(Name = "Số Giấy phép lái xe (GPLX)")]
    public string? DriverLicense { get; set; }

    [Display(Name = "Địa chỉ (KTX ĐH FPT / Nhà trọ / Nơi ở)")]
    public string? Address { get; set; }
}

public class ForgotPasswordVm
{
    [Required(ErrorMessage = "Vui lòng nhập Email đã đăng ký")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [Display(Name = "Địa chỉ Email")]
    public string Email { get; set; } = "";
}

public class ResetPasswordVm
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Nhập mã OTP 6 số")]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Mã OTP không hợp lệ")]
    [Display(Name = "Mã xác thực OTP")]
    public string Otp { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu mới")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới từ 6 ký tự trở lên")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Xác nhận mật khẩu mới")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = "";
}

public class MotorbikeVm
{
    public int MotorbikeId { get; set; }

    [Required(ErrorMessage = "Biển số là bắt buộc")]
    [RegularExpression(@"^[0-9A-Za-z\-\. ]{5,15}$", ErrorMessage = "Biển số không hợp lệ (5-15 ký tự: chữ, số, - . )")]
    [Display(Name = "Biển kiểm soát")]
    public string LicensePlate { get; set; } = "";

    [Required(ErrorMessage = "Tên xe là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tối đa 100 ký tự")]
    [Display(Name = "Tên phương tiện")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Hãng xe là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tối đa 50 ký tự")]
    [Display(Name = "Thương hiệu")]
    public string Brand { get; set; } = "";

    [Range(50, 1000, ErrorMessage = "Dung tích từ 50 đến 1000 cc")]
    [Display(Name = "Dung tích (cc)")]
    public int EngineCapacity { get; set; } = 110;

    [Range(10000, 10000000, ErrorMessage = "Giá từ 10.000 đến 10.000.000")]
    [Display(Name = "Giá thuê / ngày")]
    public decimal PricePerDay { get; set; } = 100000;

    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "Available";

    [Display(Name = "Ảnh xe (URL)")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Phân loại")]
    public string Category { get; set; } = "Xe số";

    [Display(Name = "Màu sơn")]
    public string? Color { get; set; }

    [Display(Name = "Số ODO hiện tại (km)")]
    public int CurrentOdometer { get; set; } = 0;
}

public class CustomerVm
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tối đa 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "SĐT gồm 10 số, bắt đầu bằng 0")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "CCCD/CMND là bắt buộc")]
    [RegularExpression(@"^(\d{9}|\d{12})$", ErrorMessage = "CCCD/CMND gồm 9 hoặc 12 số")]
    [Display(Name = "Số CCCD / Hộ chiếu")]
    public string IdentityNumber { get; set; } = "";

    [StringLength(20, ErrorMessage = "Tối đa 20 ký tự")]
    [Display(Name = "Giấy phép lái xe")]
    public string? DriverLicense { get; set; }

    [StringLength(200, ErrorMessage = "Tối đa 200 ký tự")]
    [Display(Name = "Địa chỉ / KTX / Nơi lưu trú")]
    public string? Address { get; set; }

    public int? UserId { get; set; }
}

public class RentalVm
{
    public int RentalId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public int MotorbikeId { get; set; }
    public string LicensePlate { get; set; } = "";
    public string MotorbikeName { get; set; } = "";
    public string? MotorbikeImage { get; set; }
    public string? Category { get; set; }
    public string StaffName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "";
    public string? Note { get; set; }

    public int? OdometerOut { get; set; }
    public int? OdometerIn { get; set; }
    public decimal LateFee { get; set; }
    public decimal DamageFee { get; set; }
    public decimal OtherFee { get; set; }
    public decimal ActualPayment { get; set; }
    public string? ConditionNote { get; set; }

    // Thông tin thanh toán
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid | DepositPaid | FullyPaid
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public class CreateRentalVm : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn khách hàng")]
    [Display(Name = "Khách hàng thuê xe")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn xe")]
    [Display(Name = "Xe máy")]
    public int MotorbikeId { get; set; }

    [Required(ErrorMessage = "Chọn ngày thuê")]
    [Display(Name = "Ngày giờ giao xe")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Chọn ngày trả dự kiến")]
    [Display(Name = "Ngày giờ trả dự kiến")]
    public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);

    [Range(0, 100000000, ErrorMessage = "Tiền cọc không hợp lệ")]
    [Display(Name = "Tiền đặt cọc (VNĐ)")]
    public decimal Deposit { get; set; } = 1000000;

    [Display(Name = "Số km hiện tại lúc giao (ODO)")]
    public int? OdometerOut { get; set; }

    [StringLength(300, ErrorMessage = "Tối đa 300 ký tự")]
    [Display(Name = "Ghi chú & Phụ kiện kèm theo")]
    public string? Note { get; set; } = "Bàn giao: 02 Mũ bảo hiểm Hola, 01 Áo mưa, Giấy tờ xe công chứng.";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("Ngày trả phải sau ngày thuê", new[] { nameof(EndDate) });
    }
}

public class ReturnRentalVm
{
    public int RentalId { get; set; }
    public int? OdometerIn { get; set; }
    public decimal LateFee { get; set; } = 0;
    public decimal DamageFee { get; set; } = 0;
    public decimal OtherFee { get; set; } = 0;
    public string? ConditionNote { get; set; }
    public bool MarkPaid { get; set; } = true;
}

public class ExtendRentalVm
{
    public int RentalId { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn ngày trả mới")]
    public DateTime NewEndDate { get; set; }
}

public class UserVm
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public int? CustomerId { get; set; }
}

public class CreateUserVm
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Từ 4 đến 50 ký tự")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = "";

    [Display(Name = "Vai trò phân quyền")]
    public string Role { get; set; } = "Staff";
}

public class MonthRevenueVm
{
    public string Month { get; set; } = "";
    public decimal Revenue { get; set; }
}

public class SummaryVm
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
    public List<MonthRevenueVm> Monthly { get; set; } = new();
}

public class HomeVm
{
    public int Available { get; set; }
    public int Rented { get; set; }
    public int Maintenance { get; set; }
    public List<RentalVm> ActiveRentals { get; set; } = new();
    public List<MotorbikeVm> AvailableBikes { get; set; } = new();
    public SummaryVm? Summary { get; set; }

    // Thông tin thêm cho Customer và Technician
    public List<RentalVm> MyRentals { get; set; } = new();
    public List<MaintenanceVm> TechMaintenances { get; set; } = new();
}

public class RentalDetailVm : RentalVm
{
    public string CustomerPhone { get; set; } = "";
    public string IdentityNumber { get; set; } = "";
    public string? DriverLicense { get; set; }
    public string? CustomerAddress { get; set; }
}

public class MaintenanceVm
{
    public int MaintenanceId { get; set; }
    public int MotorbikeId { get; set; }
    public string LicensePlate { get; set; } = "";
    public string MotorbikeName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = "";
    public decimal Cost { get; set; }
    public int? OdometerAtMaint { get; set; }
    public string Status { get; set; } = "";

    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
    public DateTime? PaidDate { get; set; }
    public string? PaidByStaff { get; set; }
    public string? TechnicianBankInfo { get; set; }
}

public class CreateMaintenanceVm
{
    [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn xe")]
    [Display(Name = "Xe máy cần bảo dưỡng")]
    public int MotorbikeId { get; set; }

    [Display(Name = "Ngày đưa vào xưởng")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Nhập nội dung bảo dưỡng")]
    [Display(Name = "Nội dung bảo dưỡng / Thay thế phụ tùng")]
    public string Description { get; set; } = "";

    [Range(0, 100000000, ErrorMessage = "Chi phí không hợp lệ")]
    [Display(Name = "Chi phí bảo dưỡng (VNĐ)")]
    public decimal Cost { get; set; } = 0;

    [Display(Name = "Số km đồng hồ lúc bảo dưỡng")]
    public int? OdometerAtMaint { get; set; }

    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "InProgress";

    [Display(Name = "Thông tin tài khoản nhận tiền của xưởng")]
    public string? TechnicianBankInfo { get; set; } = "Vietcombank 1014306428 - LE TIEN SON";
}
