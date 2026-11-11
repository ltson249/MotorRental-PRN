using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class CustomersController : Controller
{
    private readonly ApiClient api;
    public CustomersController(ApiClient api) { this.api = api; }

    public async Task<IActionResult> Index(string? q)
    {
        var url = "api/customers" + (string.IsNullOrWhiteSpace(q) ? "" : "?q=" + Uri.EscapeDataString(q.Trim()));
        var r = await api.GetAsync<List<CustomerVm>>(url);
        if (!r.Success) TempData["Error"] = r.Message;
        ViewBag.Q = q;
        return View(r.Data ?? new List<CustomerVm>());
    }

    public IActionResult Create() => View(new CustomerVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var r = await api.PostAsync<object>("api/customers", vm);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(vm); }
        TempData["Success"] = "Đã thêm khách hàng";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var r = await api.GetAsync<CustomerVm>($"api/customers/{id}");
        if (!r.Success || r.Data == null) { TempData["Error"] = r.Message; return RedirectToAction(nameof(Index)); }
        return View(r.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var r = await api.PutAsync<object>($"api/customers/{id}", vm);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(vm); }
        TempData["Success"] = "Đã cập nhật khách hàng";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await api.DeleteAsync<object>($"api/customers/{id}");
        if (r.Success) TempData["Success"] = "Đã xóa khách hàng"; else TempData["Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}
