using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize]
public class RentalsController : Controller
{
    private static readonly string[] Statuses = { "Active", "Completed", "Cancelled" };
    private readonly ApiClient api;

    public RentalsController(ApiClient api)
    {
        this.api = api;
    }

    /// <summary>
    /// Hiển thị danh sách phiếu thuê xe (Admin, Staff)
    /// </summary>
    [Authorize(Roles = "Admin,Staff")]
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
    /// Trang dành riêng cho Khách hàng (Role: Customer / Sinh viên FPT) theo dõi phiếu thuê của mình
    /// </summary>
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyRentals()
    {
        var r = await api.GetAsync<List<RentalVm>>("api/rentals/my-rentals");
        if (!r.Success)
            TempData["Error"] = r.Message;

        return View(r.Data ?? new List<RentalVm>());
    }

    /// <summary>
    /// Giao diện tạo phiếu thuê mới (Staff, Admin lập hợp đồng; Customer đặt xe trực tuyến)
    /// </summary>
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> Create(int? bikeId)
    {
        var vm = new CreateRentalVm();
        if (bikeId.HasValue && bikeId.Value > 0)
        {
            vm.MotorbikeId = bikeId.Value;
        }

        if (User.IsInRole("Customer"))
        {
            var myCustRes = await api.GetAsync<CustomerVm>("api/customers/me");
            if (myCustRes.Success && myCustRes.Data != null)
            {
                vm.CustomerId = myCustRes.Data.CustomerId;
                ViewBag.MyCustomer = myCustRes.Data;
            }
            else
            {
                var custClaim = User.FindFirst("CustomerId")?.Value;
                if (int.TryParse(custClaim, out var cId)) vm.CustomerId = cId;
            }
        }

        await LoadLookups();
        return View(vm);
    }

    /// <summary>
    /// Xử lý tạo phiếu thuê mới
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> Create(CreateRentalVm vm)
    {
        if (User.IsInRole("Customer"))
        {
            var custClaim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(custClaim, out var cId) && cId > 0)
            {
                vm.CustomerId = cId;
            }
            else
            {
                var myCustRes = await api.GetAsync<CustomerVm>("api/customers/me");
                if (myCustRes.Success && myCustRes.Data != null)
                {
                    vm.CustomerId = myCustRes.Data.CustomerId;
                }
            }

            ModelState.Remove(nameof(vm.CustomerId));
        }

        if (ModelState.IsValid)
        {
            var r = await api.PostAsync<RentalVm>("api/rentals", vm);
            if (r.Success)
            {
                if (User.IsInRole("Customer"))
                {
                    TempData["Success"] = $"🎉 Đặt thuê xe thành công (Mã phiếu #{r.Data?.RentalId})! Vui lòng quét mã VietQR để thanh toán tiền cọc giữ xe.";
                    return RedirectToAction(nameof(MyRentals));
                }

                TempData["Success"] = $"Đã lập phiếu thuê xe #{r.Data?.RentalId} thành công!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", r.Message);
        }

        if (User.IsInRole("Customer"))
        {
            var myCustRes = await api.GetAsync<CustomerVm>("api/customers/me");
            if (myCustRes.Success) ViewBag.MyCustomer = myCustRes.Data;
        }

        await LoadLookups();
        return View(vm);
    }

    /// <summary>
    /// Khách hàng hoặc Nhân viên xác nhận thanh toán (VietQR Vietcombank / Tiền mặt)
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id, string paymentMethod = "VietQR", string paymentStatus = "FullyPaid")
    {
        var payload = new
        {
            PaymentMethod = paymentMethod,
            PaymentStatus = paymentStatus
        };

        var r = await api.PostAsync<RentalVm>($"api/rentals/{id}/pay", payload);
        if (r.Success)
        {
            TempData["Success"] = $"Xác nhận thanh toán phiếu thuê #{id} thành công!";
        }
        else
        {
            TempData["Error"] = r.Message;
        }

        if (User.IsInRole("Customer"))
            return RedirectToAction(nameof(MyRentals));

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Tiếp nhận trả xe & quyết toán phụ phí từ Modal
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Return(
        int id,
        decimal lateFee = 0,
        decimal damageFee = 0,
        decimal otherFee = 0,
        int? odometerIn = null,
        string? conditionNote = null,
        bool markPaid = true)
    {
        var payload = new
        {
            LateFee = lateFee,
            DamageFee = damageFee,
            OtherFee = otherFee,
            OdometerIn = odometerIn,
            ConditionNote = conditionNote,
            MarkPaid = markPaid
        };

        var r = await api.PostAsync<RentalVm>($"api/rentals/{id}/return", payload);
        if (r.Success)
        {
            TempData["Success"] = $"Đã nhận trả xe #{id} thành công! Hóa đơn quyết toán: {r.Data?.TotalPrice:N0} ₫ (Đã thanh toán đủ)";
        }
        else
        {
            TempData["Error"] = r.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Gia hạn hợp đồng thuê
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Extend(int id, DateTime newEndDate)
    {
        var payload = new { NewEndDate = newEndDate };
        var r = await api.PostAsync<RentalVm>($"api/rentals/{id}/extend", payload);
        if (r.Success)
            TempData["Success"] = $"Đã gia hạn hợp đồng #{id} đến {newEndDate:dd/MM/yyyy HH:mm}!";
        else
            TempData["Error"] = r.Message;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Hủy phiếu thuê
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
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
    /// Xem và in hợp đồng / hóa đơn bàn giao xe (hỗ trợ cả Customer xem)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Print(int id)
    {
        var detailRes = await api.GetAsync<RentalDetailVm>($"api/rentals/{id}/detail");
        if (detailRes.Success && detailRes.Data != null)
        {
            return View(detailRes.Data);
        }

        var r = await api.GetAsync<RentalVm>($"api/rentals/{id}");
        if (!r.Success || r.Data == null)
        {
            TempData["Error"] = "Không tìm thấy phiếu thuê!";
            return RedirectToAction(User.IsInRole("Customer") ? nameof(MyRentals) : nameof(Index));
        }

        var custRes = await api.GetAsync<CustomerVm>($"api/customers/{r.Data.CustomerId}");
        var c = custRes.Data;

        var detailVm = new RentalDetailVm
        {
            RentalId = r.Data.RentalId,
            CustomerId = r.Data.CustomerId,
            CustomerName = r.Data.CustomerName,
            MotorbikeId = r.Data.MotorbikeId,
            LicensePlate = r.Data.LicensePlate,
            MotorbikeName = r.Data.MotorbikeName,
            MotorbikeImage = r.Data.MotorbikeImage,
            Category = r.Data.Category,
            StaffName = r.Data.StaffName,
            StartDate = r.Data.StartDate,
            EndDate = r.Data.EndDate,
            ReturnDate = r.Data.ReturnDate,
            PricePerDay = r.Data.PricePerDay,
            Deposit = r.Data.Deposit,
            TotalPrice = r.Data.TotalPrice,
            Status = r.Data.Status,
            Note = r.Data.Note,
            OdometerOut = r.Data.OdometerOut,
            OdometerIn = r.Data.OdometerIn,
            LateFee = r.Data.LateFee,
            DamageFee = r.Data.DamageFee,
            OtherFee = r.Data.OtherFee,
            ActualPayment = r.Data.ActualPayment,
            ConditionNote = r.Data.ConditionNote,
            PaymentStatus = r.Data.PaymentStatus,
            PaymentMethod = r.Data.PaymentMethod,
            PaymentDate = r.Data.PaymentDate,
            CustomerPhone = c?.Phone ?? "Chưa có",
            IdentityNumber = c?.IdentityNumber ?? "Chưa có",
            DriverLicense = c?.DriverLicense,
            CustomerAddress = c?.Address
        };

        return View(detailVm);
    }

    private async Task LoadLookups()
    {
        if (!User.IsInRole("Customer"))
        {
            var customers = await api.GetAsync<List<CustomerVm>>("api/customers");
            ViewBag.Customers = customers.Data ?? new List<CustomerVm>();
            if (!customers.Success) TempData["Error"] = customers.Message;
        }
        else
        {
            ViewBag.Customers = new List<CustomerVm>();
        }

        var bikes = await api.GetAsync<List<MotorbikeVm>>(
            "api/motorbikes?$orderby=Name&$filter=" + Uri.EscapeDataString("Status eq 'Available'"));

        ViewBag.Bikes = bikes.Data ?? new List<MotorbikeVm>();
        if (!bikes.Success) TempData["Error"] = bikes.Message;
    }
}
