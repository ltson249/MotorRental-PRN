namespace MotorRental.Api.Models;

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = "Staff"; // "Admin" | "Staff" | "Customer" | "Technician"
    public bool IsActive { get; set; } = true;
    public string? Email { get; set; }
    public string? ResetOtp { get; set; }
    public DateTime? ResetOtpExpires { get; set; }

    // Liên kết nếu User là Khách hàng (Customer Profile)
    public int? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    // Liên kết nghiệp vụ
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public virtual ICollection<Maintenance> HandledMaintenances { get; set; } = new List<Maintenance>();
}

public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string IdentityNumber { get; set; } = null!;
    public string? DriverLicense { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Liên kết tài khoản đăng nhập (nếu khách hàng có tài khoản trong hệ thống)
    public int? UserId { get; set; }
    public virtual AppUser? User { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

public class Motorbike
{
    public int MotorbikeId { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public int EngineCapacity { get; set; }
    public decimal PricePerDay { get; set; }
    public string Status { get; set; } = "Available";   // Available | Rented | Maintenance
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = "Xe số";     // Xe số | Xe tay ga | Xe côn tay | Xe điện
    public string? Color { get; set; }
    public int CurrentOdometer { get; set; } = 0;       // Số km hiện tại trên đồng hồ xe

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
}

public class Rental
{
    public int RentalId { get; set; }
    public int CustomerId { get; set; }
    public int MotorbikeId { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Active";      // Active | Completed | Cancelled
    public string? Note { get; set; }

    // Quản lý nghiệp vụ trả xe & phụ phí thực tế
    public int? OdometerOut { get; set; }               // Số km lúc bàn giao xe
    public int? OdometerIn { get; set; }                // Số km lúc nhận lại xe
    public decimal LateFee { get; set; } = 0;           // Phí trễ hạn
    public decimal DamageFee { get; set; } = 0;         // Phí hư hỏng / trầy xước
    public decimal OtherFee { get; set; } = 0;          // Phụ thu xăng, rửa xe,...
    public decimal ActualPayment { get; set; } = 0;     // Số tiền khách thực tế phải trả sau khi cấn trừ cọc
    public string? ConditionNote { get; set; }          // Ghi chú hiện trạng khi bàn giao / trả xe

    // Quản lý trạng thái thanh toán từ Khách hàng
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid | DepositPaid | FullyPaid
    public string? PaymentMethod { get; set; } = "VietQR"; // VietQR | Tiền mặt | Thẻ
    public DateTime? PaymentDate { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Motorbike Motorbike { get; set; } = null!;
    public virtual AppUser Staff { get; set; } = null!;
}

public class Maintenance
{
    public int MaintenanceId { get; set; }
    public int MotorbikeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = null!;
    public decimal Cost { get; set; }
    public int? OdometerAtMaint { get; set; }
    public string Status { get; set; } = "Completed";   // InProgress | Completed

    // Kỹ thuật viên / Đối tác xưởng sửa xe đảm nhận
    public int? TechnicianId { get; set; }
    public virtual AppUser? Technician { get; set; }

    // Quản lý thanh toán tiền bảo dưỡng: Cửa hàng thanh toán cho Bên sửa xe
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid | Paid
    public DateTime? PaidDate { get; set; }
    public string? PaidByStaff { get; set; }
    public string? TechnicianBankInfo { get; set; } = "Vietcombank 1014306428 - LE TIEN SON";

    public virtual Motorbike Motorbike { get; set; } = null!;
}
