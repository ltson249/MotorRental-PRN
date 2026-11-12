using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Data;
using MotorRental.Api.DTOs;
using MotorRental.Api.Models;
using MotorRental.Api.Services;

namespace MotorRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MotorRentalContext db;
    private readonly JwtService jwt;
    private readonly IEmailService emailService;

    public AuthController(MotorRentalContext db, JwtService jwt, IEmailService emailService)
    {
        this.db = db;
        this.jwt = jwt;
        this.emailService = emailService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO dto)
    {
        var user = await db.Users.Include(u => u.Customer).FirstOrDefaultAsync(u => u.Username == dto.Username);

        bool isPasswordValid = user != null && (user.PasswordHash == dto.Password || PasswordHasher.Verify(dto.Password, user.PasswordHash));

        if (user == null || !user.IsActive || !isPasswordValid)
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        var customerId = user.CustomerId ?? user.Customer?.CustomerId;
        if (!customerId.HasValue && user.Role == "Customer")
        {
            var matchedCustomer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == user.UserId);
            customerId = matchedCustomer?.CustomerId;
        }

        user.CustomerId = customerId;
        var (token, expires) = jwt.CreateToken(user);

        return Ok(new LoginResponseDTO
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            CustomerId = customerId,
            ExpiresAt = expires
        });
    }

    // ==========================================
    // 1. ĐĂNG KÝ KHÁCH HÀNG (CUSTOMER REGISTRATION)
    // ==========================================
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        var username = dto.Username.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        var phone = dto.Phone.Trim();
        var cccd = dto.IdentityNumber.Trim();

        // Kiểm tra trùng tên đăng nhập
        if (await db.Users.AnyAsync(u => u.Username == username))
            return BadRequest(new { message = "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác." });

        // Kiểm tra trùng email
        if (await db.Users.AnyAsync(u => u.Email == email) || await db.Customers.AnyAsync(c => c.Email == email))
            return BadRequest(new { message = "Địa chỉ email này đã được đăng ký trong hệ thống." });

        // Kiểm tra trùng số điện thoại đã có tài khoản
        var existingPhoneCustomer = await db.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
        if (existingPhoneCustomer != null && existingPhoneCustomer.UserId != null)
            return BadRequest(new { message = "Số điện thoại này đã được liên kết với một tài khoản khác." });

        // Kiểm tra trùng số CCCD đã có tài khoản
        var existingCccdCustomer = await db.Customers.FirstOrDefaultAsync(c => c.IdentityNumber == cccd);
        if (existingCccdCustomer != null && existingCccdCustomer.UserId != null)
            return BadRequest(new { message = "Số CCCD này đã được liên kết với một tài khoản khác." });

        Customer customerToLink;

        // Nếu thông tin khách hàng đã có trong hồ sơ showroom (chưa có tài khoản) thì liên kết
        if (existingPhoneCustomer != null && existingPhoneCustomer.UserId == null)
        {
            customerToLink = existingPhoneCustomer;
            customerToLink.Email = email;
            if (!string.IsNullOrWhiteSpace(dto.DriverLicense)) customerToLink.DriverLicense = dto.DriverLicense.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Address)) customerToLink.Address = dto.Address.Trim();
        }
        else if (existingCccdCustomer != null && existingCccdCustomer.UserId == null)
        {
            customerToLink = existingCccdCustomer;
            customerToLink.Email = email;
            if (!string.IsNullOrWhiteSpace(dto.DriverLicense)) customerToLink.DriverLicense = dto.DriverLicense.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Address)) customerToLink.Address = dto.Address.Trim();
        }
        else
        {
            // Tạo mới khách hàng
            customerToLink = new Customer
            {
                FullName = dto.FullName.Trim(),
                Phone = phone,
                IdentityNumber = cccd,
                DriverLicense = string.IsNullOrWhiteSpace(dto.DriverLicense) ? null : dto.DriverLicense.Trim(),
                Address = string.IsNullOrWhiteSpace(dto.Address) ? "Hòa Lạc, Thạch Thất, Hà Nội" : dto.Address.Trim(),
                Email = email,
                CreatedAt = DateTime.Now
            };
            db.Customers.Add(customerToLink);
            await db.SaveChangesAsync();
        }

        // Tạo tài khoản AppUser role Customer
        var newUser = new AppUser
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FullName = dto.FullName.Trim(),
            Role = "Customer",
            Email = email,
            IsActive = true,
            CustomerId = customerToLink.CustomerId
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();

        // Gắn ngược UserId vào Customer
        customerToLink.UserId = newUser.UserId;
        await db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đăng ký tài khoản thành công! Chào mừng bạn đến với MotorRental FPT Hòa Lạc.",
            username = newUser.Username,
            customerId = customerToLink.CustomerId
        });
    }

    // ==========================================
    // 2. QUÊN MẬT KHẨU & GỬI MÃ OTP QUA EMAIL
    // ==========================================
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        // Tìm AppUser theo email
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            // Tìm qua Customer
            var cust = await db.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (cust?.UserId != null)
            {
                user = await db.Users.FindAsync(cust.UserId);
            }
        }

        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy tài khoản nào được đăng ký với email này." });
        }

        if (!user.IsActive)
        {
            return BadRequest(new { message = "Tài khoản này hiện đang bị khóa. Vui lòng liên hệ quản trị viên." });
        }

        // Sinh mã OTP ngẫu nhiên 6 chữ số
        var otp = Random.Shared.Next(100000, 999999).ToString();
        user.ResetOtp = otp;
        user.ResetOtpExpires = DateTime.UtcNow.AddMinutes(10);
        await db.SaveChangesAsync();

        // Gửi email
        var sendResult = await emailService.SendOtpEmailAsync(email, user.FullName, otp);

        // Tạo chuỗi mask email an toàn: vd vu***@fpt.edu.vn
        var atIndex = email.IndexOf('@');
        var maskedEmail = atIndex > 2 
            ? string.Concat(email.AsSpan(0, 2), "***", email.AsSpan(atIndex))
            : email;

        return Ok(new
        {
            success = true,
            message = sendResult.Message,
            email = email,
            maskedEmail = maskedEmail,
            devOtp = otp // Cung cấp để chấm đồ án hoặc test khi chưa cấu hình SMTP
        });
    }

    // ==========================================
    // 3. XÁC THỰC MÃ OTP
    // ==========================================
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDTO dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var otp = dto.Otp.Trim();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            var cust = await db.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (cust?.UserId != null) user = await db.Users.FindAsync(cust.UserId);
        }

        if (user == null || string.IsNullOrWhiteSpace(user.ResetOtp) || user.ResetOtp != otp)
        {
            return BadRequest(new { message = "Mã OTP không chính xác. Vui lòng kiểm tra lại." });
        }

        if (!user.ResetOtpExpires.HasValue || user.ResetOtpExpires.Value < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Mã OTP đã hết hạn (chỉ có hiệu lực trong 10 phút). Hãy yêu cầu mã mới." });
        }

        return Ok(new
        {
            success = true,
            message = "Mã OTP hợp lệ! Bạn có thể đặt mật khẩu mới."
        });
    }

    // ==========================================
    // 4. ĐẶT LẠI MẬT KHẨU MỚI (RESET PASSWORD)
    // ==========================================
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var otp = dto.Otp.Trim();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            var cust = await db.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (cust?.UserId != null) user = await db.Users.FindAsync(cust.UserId);
        }

        if (user == null || string.IsNullOrWhiteSpace(user.ResetOtp) || user.ResetOtp != otp)
        {
            return BadRequest(new { message = "Mã OTP không chính xác. Vui lòng thử lại." });
        }

        if (!user.ResetOtpExpires.HasValue || user.ResetOtpExpires.Value < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Mã OTP đã hết hạn. Vui lòng yêu cầu lại mã mới." });
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return BadRequest(new { message = "Mật khẩu xác nhận không khớp." });
        }

        // Cập nhật mật khẩu mới và xóa OTP
        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        user.ResetOtp = null;
        user.ResetOtpExpires = null;
        await db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới ngay bây giờ."
        });
    }
}
