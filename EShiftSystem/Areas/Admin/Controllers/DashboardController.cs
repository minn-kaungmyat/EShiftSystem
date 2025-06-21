using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShiftSystem.Data;
using EShiftSystem.Models.Enums;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get current date for comparisons
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Job statistics
            var totalJobs = await _context.Jobs.CountAsync();
            var pendingJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Pending);
            var approvedJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Approved);
            var inProgressJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.InProgress);
            var completedJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Completed);
            var todayJobs = await _context.Jobs.CountAsync(j => j.CreatedAt.Date == today);
            var weekJobs = await _context.Jobs.CountAsync(j => j.CreatedAt.Date >= startOfWeek);
            var monthJobs = await _context.Jobs.CountAsync(j => j.CreatedAt.Date >= startOfMonth);

            // Customer statistics
            var totalCustomers = await _context.Customers.CountAsync();
            var activeCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            var inactiveCustomers = totalCustomers - activeCustomers;

            // Transport Unit statistics
            var totalTransportUnits = await _context.TransportUnits.CountAsync();
            var availableTransportUnits = await _context.TransportUnits.CountAsync(t => t.Status == TransportUnitStatus.Available);
            var assignedTransportUnits = await _context.TransportUnits.CountAsync(t => t.Status == TransportUnitStatus.Assigned);

            // Resource statistics
            var totalDrivers = await _context.Drivers.CountAsync();
            var totalLorries = await _context.Lorries.CountAsync();
            var totalContainers = await _context.Containers.CountAsync();
            var totalAssistants = await _context.Assistants.CountAsync();

            // Recent jobs (last 5)
            var recentJobs = await _context.Jobs
                .Include(j => j.Customer)
                .ThenInclude(c => c.ApplicationUser)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Priority distribution
            var urgentJobs = await _context.Jobs.CountAsync(j => j.Priority == JobPriority.Urgent && j.Status != JobStatus.Completed && j.Status != JobStatus.Cancelled);
            var highPriorityJobs = await _context.Jobs.CountAsync(j => j.Priority == JobPriority.High && j.Status != JobStatus.Completed && j.Status != JobStatus.Cancelled);
            var normalPriorityJobs = await _context.Jobs.CountAsync(j => j.Priority == JobPriority.Normal && j.Status != JobStatus.Completed && j.Status != JobStatus.Cancelled);

            // Create view model
            var viewModel = new
            {
                // Job Stats
                TotalJobs = totalJobs,
                PendingJobs = pendingJobs,
                ApprovedJobs = approvedJobs,
                InProgressJobs = inProgressJobs,
                CompletedJobs = completedJobs,
                TodayJobs = todayJobs,
                WeekJobs = weekJobs,
                MonthJobs = monthJobs,

                // Customer Stats
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                InactiveCustomers = inactiveCustomers,

                // Transport Unit Stats
                TotalTransportUnits = totalTransportUnits,
                AvailableTransportUnits = availableTransportUnits,
                AssignedTransportUnits = assignedTransportUnits,

                // Resource Stats
                TotalDrivers = totalDrivers,
                TotalLorries = totalLorries,
                TotalContainers = totalContainers,
                TotalAssistants = totalAssistants,

                // Priority Stats
                UrgentJobs = urgentJobs,
                HighPriorityJobs = highPriorityJobs,
                NormalPriorityJobs = normalPriorityJobs,

                // Recent Activity
                RecentJobs = recentJobs
            };

            return View(viewModel);
        }
    }
}