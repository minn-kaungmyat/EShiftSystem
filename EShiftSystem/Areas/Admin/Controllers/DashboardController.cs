using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShiftSystem.Data;
using EShiftSystem.Models.Enums;

namespace EShiftSystem.Areas.Admin.Controllers
{
    // admin dashboard controller providing system overview and statistics
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        // initializes dashboard controller with database context
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // displays comprehensive dashboard with system statistics and recent activity
        public async Task<IActionResult> Index()
        {
            // Get current date for comparisons
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // collect job statistics by status and timeframe
            var totalJobs = await _context.Jobs.CountAsync();
            var pendingJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Pending);
            var approvedJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Approved);
            var inProgressJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.InProgress);
            var completedJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Completed);
            var deliveredJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Delivered);
            var cancelledJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Cancelled);
            var todayJobs = await _context.Jobs.CountAsync(j => j.CreatedAt.Date == today);
            var weekJobs = await _context.Jobs.CountAsync(j => j.CreatedAt.Date >= startOfWeek);
            var monthJobs = await _context.Jobs.CountAsync(j => j.CreatedAt.Date >= startOfMonth);

            // collect customer account statistics
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

            // Monthly trends data (last 6 months)
            var monthlyJobsData = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var startOfMonthPeriod = new DateTime(month.Year, month.Month, 1);
                var endOfMonthPeriod = startOfMonthPeriod.AddMonths(1).AddDays(-1);
                
                var jobsCount = await _context.Jobs.CountAsync(j => 
                    j.CreatedAt.Date >= startOfMonthPeriod && j.CreatedAt.Date <= endOfMonthPeriod);
                
                monthlyJobsData.Add(new {
                    Month = month.ToString("MMM yyyy"),
                    Count = jobsCount
                });
            }

            // Create view model
            var viewModel = new
            {
                // Job Stats
                TotalJobs = totalJobs,
                PendingJobs = pendingJobs,
                ApprovedJobs = approvedJobs,
                InProgressJobs = inProgressJobs,
                DeliveredJobs = deliveredJobs,
                CompletedJobs = completedJobs,
                CancelledJobs = cancelledJobs,
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

                // Chart Data
                MonthlyJobsData = monthlyJobsData,

                // Recent Activity
                RecentJobs = recentJobs
            };

            return View(viewModel);
        }
    }
}