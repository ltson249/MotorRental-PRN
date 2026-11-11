using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApiClient api;
    public UsersController(ApiClient api) { this.api = api; }

    public async Task<IActionResult> Index()
    {
        var r = await api.GetAsync<List<UserVm>>("api/users");
        if (!r.Success) TempData["Error"] = r.Message;
        return View(r.Data ?? new List<UserVm>());
    }

    public IActionResult Create() => View(new CreateUserVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var r = await api.PostAsync<object>("api/users", vm);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(vm); }
        TempData["Success"] = "Đã tạo tài khoản";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var r = await api.PostAsync<object>($"api/users/{id}/toggle");
        if (r.Success) TempData["Success"] = "Đã cập nhật trạng thái tài khoản"; else TempData["Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}
