using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Web.Models;
using MotorRental.Web.Services;

namespace MotorRental.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApiClient api;
    public HomeController(ApiClient api) { this.api = api; }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeVm();

        var bikes = await api.GetAsync<List<MotorbikeVm>>("api/motorbikes");
        if (bikes.Success && bikes.Data != null)
        {
            vm.Available = bikes.Data.Count(b => b.Status == "Available");
            vm.Rented = bikes.Data.Count(b => b.Status == "Rented");
            vm.Maintenance = bikes.Data.Count(b => b.Status == "Maintenance");
            vm.AvailableBikes = bikes.Data.Where(b => b.Status == "Available").Take(8).ToList();
        }
        else TempData["Error"] = bikes.Message;

        var rentals = await api.GetAsync<List<RentalVm>>("api/rentals?status=Active");
        if (rentals.Success && rentals.Data != null)
        {
            vm.ActiveRentals = rentals.Data;
        }

        if (User.IsInRole("Admin"))
        {
            var s = await api.GetAsync<SummaryVm>("api/reports/summary");
            if (s.Success) vm.Summary = s.Data;
        }
        return View(vm);
    }
}
