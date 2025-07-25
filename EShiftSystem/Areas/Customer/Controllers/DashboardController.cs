﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Models.Enums;

namespace EShiftSystem.Areas.Customer.Controllers
{
    // customer dashboard controller showing personal job statistics and activity
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // initializes dashboard with database context and user manager
        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // displays customer's personal dashboard with job statistics and recent activity
        public async Task<IActionResult> Index()
        {
            // Get current customer
            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer profile not found.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // calculate customer's job statistics for dashboard display
            var customerJobs = _context.Jobs.Where(j => j.CustomerId == customer.CustomerId);
            
            var totalJobs = await customerJobs.CountAsync();
            var activeJobs = await customerJobs.CountAsync(j => j.Status != JobStatus.Completed && j.Status != JobStatus.Cancelled);
            var completedJobs = await customerJobs.CountAsync(j => j.Status == JobStatus.Completed);
            
            // Jobs requiring customer action (delivered but not completed)
            var jobsAwaitingConfirmation = await customerJobs
                .CountAsync(j => j.Status == JobStatus.Delivered);

            // Recent jobs (last 5)
            var recentJobs = await customerJobs
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Priority jobs (only urgent active jobs)
            var urgentActiveJobs = await customerJobs
                .CountAsync(j => j.Priority == JobPriority.Urgent && 
                           j.Status != JobStatus.Completed && 
                           j.Status != JobStatus.Cancelled);

            // Create simplified view model
            var viewModel = new
            {
                CustomerName = customer.FullName,
                
                // Essential Statistics Only
                TotalJobs = totalJobs,
                ActiveJobs = activeJobs,
                CompletedJobs = completedJobs,
                JobsAwaitingConfirmation = jobsAwaitingConfirmation,
                UrgentActiveJobs = urgentActiveJobs,
                
                // Recent Activity
                RecentJobs = recentJobs
            };

            return View(viewModel);
        }
    }
}
