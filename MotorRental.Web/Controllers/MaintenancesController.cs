using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize(Roles = "Admin,Staff,Technician")]
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
                TempData["Success"] = "Đã lập hồ sơ đưa xe vào xưởng bảo dưỡng!";
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

    /// <summary>
    /// Kỹ thuật viên / Xưởng xác nhận đã sửa xong xe
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Finish(int id)
    {
        var r = await api.PostAsync<object>($"api/maintenances/{id}/finish");
        if (r.Success)
            TempData["Success"] = $"Đã hoàn tất bảo dưỡng #{id}, xe đã chuyển về showroom ở trạng thái Sẵn sàng phục vụ!";
        else
            TempData["Error"] = r.Message;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Cửa hàng / Nhân viên xác nhận thanh toán tiền cho bên sửa chữa bảo dưỡng
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Pay(int id)
    {
        var staffName = User.FindFirst("FullName")?.Value ?? "Nhân viên Showroom";
        var r = await api.PostAsync<object>($"api/maintenances/{id}/pay", new { PaidByStaff = staffName });
        if (r.Success)
            TempData["Success"] = $"Đã thanh toán chi phí bảo dưỡng cho xưởng sửa xe thành công!";
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
