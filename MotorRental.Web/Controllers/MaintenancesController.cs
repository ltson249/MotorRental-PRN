using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class MaintenancesController : Controller
{
    private readonly ApiClient api;
    public MaintenancesController(ApiClient api) { this.api = api; }

    public async Task<IActionResult> Index(string? status)
    {
        var url = "api/maintenances";
        if (!string.IsNullOrWhiteSpace(status))
            url += $"?status={Uri.EscapeDataString(status)}";

        var r = await api.GetAsync<List<MaintenanceVm>>(url);
        if (!r.Success) TempData["Error"] = r.Message;

        var bikes = await api.GetAsync<List<MotorbikeVm>>("api/motorbikes?$orderby=Name");
        ViewBag.Bikes = bikes.Data ?? new List<MotorbikeVm>();
        ViewBag.Status = status;

        return View(r.Data ?? new List<MaintenanceVm>());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMaintenanceVm vm)
    {
        if (ModelState.IsValid)
        {
            var r = await api.PostAsync<MaintenanceVm>("api/maintenances", vm);
            if (r.Success)
            {
                TempData["Success"] = "Đã lập hồ sơ bảo dưỡng / sửa chữa xe!";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = r.Message;
        }
        else
        {
            TempData["Error"] = "Dữ liệu nhập không hợp lệ!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Finish(int id)
    {
        var r = await api.PostAsync<object>($"api/maintenances/{id}/finish");
        if (r.Success)
            TempData["Success"] = $"Đã hoàn tất bảo dưỡng #{id}, xe đã chuyển sang trạng thái Sẵn sàng!";
        else
            TempData["Error"] = r.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await api.DeleteAsync<object>($"api/maintenances/{id}");
        if (r.Success) TempData["Success"] = "Đã xóa hồ sơ bảo dưỡng";
        else TempData["Error"] = r.Message;

        return RedirectToAction(nameof(Index));
    }
}
