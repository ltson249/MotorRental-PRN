using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient api;
    public AccountController(ApiClient api) { this.api = api; }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await api.PostAsync<LoginResponse>("api/auth/login", new { vm.Username, vm.Password });
        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError("", result.Message);
            return View(vm);
        }

        var d = result.Data;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, d.Username),
            new("FullName", d.FullName),
            new(ClaimTypes.Role, d.Role),
            new("access_token", d.Token)
        };

        if (d.CustomerId.HasValue)
        {
            claims.Add(new Claim("CustomerId", d.CustomerId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = d.ExpiresAt });

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    // ==========================================
    // 1. ĐĂNG KÝ TÀI KHOẢN KHÁCH HÀNG (REGISTER)
    // ==========================================
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new RegisterVm());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await api.PostAsync<dynamic>("api/auth/register", new
        {
            vm.Username,
            vm.Password,
            vm.ConfirmPassword,
            vm.FullName,
            vm.Email,
            vm.Phone,
            vm.IdentityNumber,
            vm.DriverLicense,
            vm.Address
        });

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(vm);
        }

        TempData["SuccessMessage"] = "🎉 Chúc mừng bạn đã đăng ký tài khoản thành công! Hãy đăng nhập để bắt đầu thuê xe.";
        return RedirectToAction(nameof(Login));
    }

    // ==========================================
    // 2. QUÊN MẬT KHẨU (GỬI MÃ OTP)
    // ==========================================
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordVm());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await api.PostAsync<dynamic>("api/auth/forgot-password", new { vm.Email });
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(vm);
        }

        TempData["InfoMessage"] = "📧 Mã OTP xác thực 6 chữ số đã được gửi tới email của bạn (kiểm tra hộp thư đến hoặc spam, có hiệu lực 10 phút).";
        return RedirectToAction(nameof(ResetPassword), new { email = vm.Email });
    }

    // ==========================================
    // 3. ĐẶT LẠI MẬT KHẨU (XÁC THỰC OTP + ĐỔI MẬT KHẨU)
    // ==========================================
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email)
    {
        return View(new ResetPasswordVm { Email = email ?? "" });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await api.PostAsync<dynamic>("api/auth/reset-password", new
        {
            vm.Email,
            vm.Otp,
            vm.NewPassword,
            vm.ConfirmPassword
        });

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(vm);
        }

        TempData["SuccessMessage"] = "✅ Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
