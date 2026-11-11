using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize]
public class MotorbikesController : Controller
{
    private static readonly string[] Statuses = { "Available", "Rented", "Maintenance" };
    private readonly ApiClient api;
    public MotorbikesController(ApiClient api) { this.api = api; }

    // Tim kiem / loc bang OData: $filter, $orderby
    public async Task<IActionResult> Index(string? q, string? status)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().Replace("'", "''");
            filters.Add($"(contains(Name,'{s}') or contains(LicensePlate,'{s}') or contains(Brand,'{s}'))");
        }
        if (!string.IsNullOrWhiteSpace(status) && Statuses.Contains(status))
            filters.Add($"Status eq '{status}'");

        var url = "api/motorbikes?$orderby=Name";
        if (filters.Count > 0) url += "&$filter=" + Uri.EscapeDataString(string.Join(" and ", filters));

        var r = await api.GetAsync<List<MotorbikeVm>>(url);
        if (!r.Success) TempData["Error"] = r.Message;
        ViewBag.Q = q;
        ViewBag.Status = status;
        return View(r.Data ?? new List<MotorbikeVm>());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new MotorbikeVm());

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(MotorbikeVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var r = await api.PostAsync<object>("api/motorbikes", vm);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(vm); }
        TempData["Success"] = "Đã thêm xe mới";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await api.GetAsync<MotorbikeVm>($"api/motorbikes/{id}");
        if (!r.Success || r.Data == null) { TempData["Error"] = r.Message; return RedirectToAction(nameof(Index)); }
        return View(r.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, MotorbikeVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var r = await api.PutAsync<object>($"api/motorbikes/{id}", vm);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(vm); }
        TempData["Success"] = "Đã cập nhật xe";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await api.DeleteAsync<object>($"api/motorbikes/{id}");
        if (r.Success) TempData["Success"] = "Đã xóa xe"; else TempData["Error"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}
