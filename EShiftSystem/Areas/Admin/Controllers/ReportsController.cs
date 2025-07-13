using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EShiftSystem.Areas.Admin.Controllers
{
    public class JobStatusReportItem
    {
        public JobStatus Status { get; set; }
        public int Count { get; set; }
    }

    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Job Status Report
        public async Task<IActionResult> JobStatusReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Jobs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(j => j.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(j => j.CreatedAt <= endDate.Value.AddDays(1));

            var jobs = await query.ToListAsync();

            var statusReport = jobs
                .GroupBy(j => j.Status)
                .Select(g => new JobStatusReportItem
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(statusReport);
        }

        // Transport Unit Report
        public async Task<IActionResult> TransportUnitReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.TransportUnits
                .Include(tu => tu.Driver)
                .Include(tu => tu.Container)
                .Include(tu => tu.Loads)
                .AsQueryable();

            var report = await query
                .Select(tu => new
                {
                    TransportUnit = tu,
                    TotalJobs = tu.Loads.Count,
                    CompletedJobs = tu.Loads.Count(l => l.Status == JobStatus.Completed),
                    InProgressJobs = tu.Loads.Count(l => l.Status == JobStatus.InProgress)
                })
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(report);
        }

        // Customer Activity Report
        public async Task<IActionResult> CustomerActivityReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Customers
                .Include(c => c.Jobs)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.Jobs.Any(j => j.CreatedAt >= startDate.Value));
            if (endDate.HasValue)
                query = query.Where(c => c.Jobs.Any(j => j.CreatedAt <= endDate.Value.AddDays(1)));

            var customers = await query.ToListAsync();

            var report = customers.Select(c => new
            {
                Customer = c,
                TotalJobs = c.Jobs.Count(j => (!startDate.HasValue || j.CreatedAt >= startDate.Value) &&
                                             (!endDate.HasValue || j.CreatedAt <= endDate.Value.AddDays(1))),
                CompletedJobs = c.Jobs.Count(j => j.Status == JobStatus.Completed &&
                                                 (!startDate.HasValue || j.CreatedAt >= startDate.Value) &&
                                                 (!endDate.HasValue || j.CreatedAt <= endDate.Value.AddDays(1))),
                CancelledJobs = c.Jobs.Count(j => j.Status == JobStatus.Cancelled &&
                                                 (!startDate.HasValue || j.CreatedAt >= startDate.Value) &&
                                                 (!endDate.HasValue || j.CreatedAt <= endDate.Value.AddDays(1)))
            })
            .OrderByDescending(r => r.TotalJobs)
            .ToList();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(report);
        }

        // Export Job Status Report as CSV
        public async Task<IActionResult> ExportJobStatusReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Jobs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(j => j.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(j => j.CreatedAt <= endDate.Value.AddDays(1));

            var jobs = await query.ToListAsync();

            var statusReport = jobs
                .GroupBy(j => j.Status)
                .Select(g => new JobStatusReportItem
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Status)
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Status,Count");
            
            foreach (var item in statusReport)
            {
                csv.AppendLine($"{item.Status},{item.Count}");
            }

            var fileName = $"JobStatusReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        // Export Transport Unit Report as CSV
        public async Task<IActionResult> ExportTransportUnitReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.TransportUnits
                .Include(tu => tu.Driver)
                .Include(tu => tu.Container)
                .Include(tu => tu.Loads)
                .AsQueryable();

            var report = await query
                .Select(tu => new
                {
                    TransportUnit = tu,
                    TotalJobs = tu.Loads.Count,
                    CompletedJobs = tu.Loads.Count(l => l.Status == JobStatus.Completed),
                    InProgressJobs = tu.Loads.Count(l => l.Status == JobStatus.InProgress)
                })
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Transport Unit,Status,Driver,Container,Total Jobs,Completed Jobs,In Progress Jobs");
            
            foreach (var item in report)
            {
                var transportUnit = item.TransportUnit.Name ?? "N/A";
                var status = item.TransportUnit.Status.ToString();
                var driver = item.TransportUnit.Driver?.Name ?? "N/A";
                var container = $"{item.TransportUnit.Container?.Size} - {item.TransportUnit.Container?.Type}" ?? "N/A";
                
                csv.AppendLine($"\"{transportUnit}\",\"{status}\",\"{driver}\",\"{container}\",{item.TotalJobs},{item.CompletedJobs},{item.InProgressJobs}");
            }

            var fileName = $"TransportUnitReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        // Export Customer Activity Report as CSV
        public async Task<IActionResult> ExportCustomerActivityReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Customers
                .Include(c => c.Jobs)
                .Include(c => c.ApplicationUser)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.Jobs.Any(j => j.CreatedAt >= startDate.Value));
            if (endDate.HasValue)
                query = query.Where(c => c.Jobs.Any(j => j.CreatedAt <= endDate.Value.AddDays(1)));

            var customers = await query.ToListAsync();

            var report = customers.Select(c => new
            {
                Customer = c,
                TotalJobs = c.Jobs.Count(j => (!startDate.HasValue || j.CreatedAt >= startDate.Value) &&
                                             (!endDate.HasValue || j.CreatedAt <= endDate.Value.AddDays(1))),
                CompletedJobs = c.Jobs.Count(j => j.Status == JobStatus.Completed &&
                                                 (!startDate.HasValue || j.CreatedAt >= startDate.Value) &&
                                                 (!endDate.HasValue || j.CreatedAt <= endDate.Value.AddDays(1))),
                CancelledJobs = c.Jobs.Count(j => j.Status == JobStatus.Cancelled &&
                                                 (!startDate.HasValue || j.CreatedAt >= startDate.Value) &&
                                                 (!endDate.HasValue || j.CreatedAt <= endDate.Value.AddDays(1)))
            })
            .OrderByDescending(r => r.TotalJobs)
            .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Customer Name,Email,Total Jobs,Completed Jobs,Cancelled Jobs");
            
            foreach (var item in report)
            {
                var customerName = item.Customer.FullName ?? "N/A";
                var email = item.Customer.ApplicationUser?.Email ?? "N/A";
                
                csv.AppendLine($"\"{customerName}\",\"{email}\",{item.TotalJobs},{item.CompletedJobs},{item.CancelledJobs}");
            }

            var fileName = $"CustomerActivityReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }
    }
} 