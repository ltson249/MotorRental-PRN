using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Data;
using MotorRental.Api.DTOs;

namespace MotorRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly MotorRentalContext db;
    public ReportsController(MotorRentalContext db) { this.db = db; }

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryDTO>> Summary()
    {
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var sixStart = monthStart.AddMonths(-5);

        var byStatus = await db.Motorbikes.GroupBy(m => m.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
        int CountOf(string s) => byStatus.FirstOrDefault(b => b.Status == s)?.Count ?? 0;

        var completed = db.Rentals.Where(r => r.Status == "Completed");
        var totalRevenue = await completed.SumAsync(r => (decimal?)r.TotalPrice) ?? 0;
        var monthRevenue = await completed.Where(r => r.ReturnDate >= monthStart).SumAsync(r => (decimal?)r.TotalPrice) ?? 0;

        var maintenanceCost = await db.Maintenances.SumAsync(m => (decimal?)m.Cost) ?? 0;

        var rows = await completed.Where(r => r.ReturnDate >= sixStart)
            .Select(r => new { r.ReturnDate, r.TotalPrice }).ToListAsync();

        var monthly = Enumerable.Range(0, 6).Select(i =>
        {
            var m = sixStart.AddMonths(i);
            return new MonthRevenueDTO
            {
                Month = m.ToString("MM/yyyy"),
                Revenue = rows.Where(x => x.ReturnDate!.Value.Year == m.Year && x.ReturnDate.Value.Month == m.Month)
                              .Sum(x => x.TotalPrice)
            };
        }).ToList();

        return Ok(new SummaryDTO
        {
            TotalMotorbikes = byStatus.Sum(b => b.Count),
            Available = CountOf("Available"),
            Rented = CountOf("Rented"),
            Maintenance = CountOf("Maintenance"),
            ActiveRentals = await db.Rentals.CountAsync(r => r.Status == "Active"),
            TotalCustomers = await db.Customers.CountAsync(),
            RevenueThisMonth = monthRevenue,
            TotalRevenue = totalRevenue,
            MaintenanceCost = maintenanceCost,
            Monthly = monthly
        });
    }
}
