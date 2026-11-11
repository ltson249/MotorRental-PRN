using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class RentalsController : Controller
{
    private static readonly string[] Statuses = { "Active", "Completed", "Cancelled" };
    private readonly ApiClient api;

    public RentalsController(ApiClient api)
    {
        this.api = api;
    }

    /// <summary>
    /// Hiển thị danh sách phiếu thuê (có hỗ trợ lọc theo trạng thái, từ khóa, xe quá hạn)
    /// </summary>
    public async Task<IActionResult> Index(string? status, string? q, bool? overdueOnly)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(status) && Statuses.Contains(status))
            queryParams.Add($"status={Uri.EscapeDataString(status)}");

        if (!string.IsNullOrWhiteSpace(q))
            queryParams.Add($"q={Uri.EscapeDataString(q.Trim())}");

        if (overdueOnly == true)
            queryParams.Add("overdueOnly=true");

        var url = "api/rentals" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        var r = await api.GetAsync<List<RentalVm>>(url);

        if (!r.Success)
            TempData["Error"] = r.Message;

        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.OverdueOnly = overdueOnly;

        return View(r.Data ?? new List<RentalVm>());
    }

    /// <summary>
    /// Giao diện tạo phiếu thuê mới
    /// </summary>
    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View(new CreateRentalVm());
    }

    /// <summary>
    /// Xử lý tạo phiếu thuê mới
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRentalVm vm)
    {
        if (ModelState.IsValid)
        {
            var r = await api.PostAsync<RentalVm>("api/rentals", vm);
            if (r.Success)
            {
                TempData["Success"] = "Đã lập phiếu thuê xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", r.Message);
        }
        await LoadLookups();
        return View(vm);
    }

    /// <summary>
    /// Tiếp nhận trả xe & quyết toán phụ phí từ Modal
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(
        int id,
        decimal lateFee = 0,
        decimal damageFee = 0,
        decimal otherFee = 0,
        int? odometerIn = null,
        string? conditionNote = null)
    {
        var payload = new
        {
            LateFee = lateFee,
            DamageFee = damageFee,
            OtherFee = otherFee,
            OdometerIn = odometerIn,
            ConditionNote = conditionNote
        };

        var r = await api.PostAsync<RentalVm>($"api/rentals/{id}/return", payload);
        if (r.Success)
        {
            TempData["Success"] = $"Đã nhận trả xe #{id} thành công! Tổng hóa đơn: {r.Data?.TotalPrice:N0} ₫";
        }
        else
        {
            TempData["Error"] = r.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Hủy phiếu thuê
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var r = await api.PostAsync<RentalVm>($"api/rentals/{id}/cancel");
        if (r.Success)
            TempData["Success"] = $"Đã hủy phiếu thuê #{id} thành công!";
        else
            TempData["Error"] = r.Message;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Xem và in hợp đồng / hóa đơn bàn giao xe (Chuẩn model RentalDetailVm)
    /// GET: /Rentals/Print/5
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Print(int id)
    {
        var r = await api.GetAsync<RentalVm>($"api/rentals/{id}");
        if (!r.Success || r.Data == null)
        {
            TempData["Error"] = "Không tìm thấy phiếu thuê!";
            return RedirectToAction(nameof(Index));
        }

        // Lấy thêm thông tin chi tiết của khách hàng để hiển thị trên hợp đồng
        var custRes = await api.GetAsync<CustomerVm>($"api/customers/{r.Data.CustomerId}");
        var c = custRes.Data;

        // Map sang RentalDetailVm khớp với khai báo @model RentalDetailVm trong Print.cshtml
        var detailVm = new RentalDetailVm
        {
            RentalId = r.Data.RentalId,
            CustomerId = r.Data.CustomerId,
            CustomerName = r.Data.CustomerName,
            MotorbikeId = r.Data.MotorbikeId,
            LicensePlate = r.Data.LicensePlate,
            MotorbikeName = r.Data.MotorbikeName,
            StaffName = r.Data.StaffName,
            StartDate = r.Data.StartDate,
            EndDate = r.Data.EndDate,
            ReturnDate = r.Data.ReturnDate,
            PricePerDay = r.Data.PricePerDay,
            Deposit = r.Data.Deposit,
            TotalPrice = r.Data.TotalPrice,
            Status = r.Data.Status,
            Note = r.Data.Note,
            CustomerPhone = c?.Phone ?? "Chưa có",
            IdentityNumber = c?.IdentityNumber ?? "Chưa có",
            DriverLicense = c?.DriverLicense,
            CustomerAddress = c?.Address
        };

        return View(detailVm);
    }

    /// <summary>
    /// Nạp dữ liệu danh sách khách hàng và xe sẵn sàng cho form tạo phiếu
    /// </summary>
    private async Task LoadLookups()
    {
        var customers = await api.GetAsync<List<CustomerVm>>("api/customers");
        var bikes = await api.GetAsync<List<MotorbikeVm>>(
            "api/motorbikes?$orderby=Name&$filter=" + Uri.EscapeDataString("Status eq 'Available'"));

        ViewBag.Customers = customers.Data ?? new List<CustomerVm>();
        ViewBag.Bikes = bikes.Data ?? new List<MotorbikeVm>();

        if (!customers.Success) TempData["Error"] = customers.Message;
        else if (!bikes.Success) TempData["Error"] = bikes.Message;
    }
}