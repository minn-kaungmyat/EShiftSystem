using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Models.Enums;
using EShiftSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Include(j => j.Customer)
                    .Include(j => j.Loads)
                        .ThenInclude(l => l.TransportUnit)
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync();

                return View(jobs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading jobs: " + ex.Message;
                return View(new List<Job>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Job ID is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Customer)
                        .ThenInclude(c => c.ApplicationUser)
                    .Include(j => j.Loads)
                        .ThenInclude(l => l.LoadItems)
                    .Include(j => j.Loads)
                        .ThenInclude(l => l.TransportUnit)
                            .ThenInclude(tu => tu != null ? tu.Driver : null)
                    .Include(j => j.Loads)
                        .ThenInclude(l => l.TransportUnit)
                            .ThenInclude(tu => tu != null ? tu.Container : null)
                    .Include(j => j.Loads)
                        .ThenInclude(l => l.TransportUnit)
                            .ThenInclude(tu => tu != null ? tu.Assistants : null)
                    .FirstOrDefaultAsync(j => j.JobId == id);

                if (job == null)
                {
                    TempData["ErrorMessage"] = "Job not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get available transport units for assignment
                var availableTransportUnits = await _context.TransportUnits
                    .Include(tu => tu.Driver)
                    .Include(tu => tu.Container)
                    .Include(tu => tu.Assistants)
                    .ToListAsync();

                ViewData["TransportUnits"] = new SelectList(availableTransportUnits, "TransportUnitId", "Name");

                return View(job);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading job details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTransportUnit(int jobId, int loadId, int? transportUnitId)
        {
            try
            {
                var load = await _context.Loads
                    .Include(l => l.Job)
                    .Include(l => l.TransportUnit)
                    .FirstOrDefaultAsync(l => l.LoadId == loadId && l.JobId == jobId);

                if (load == null)
                {
                    TempData["ErrorMessage"] = "Load not found.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                var oldTransportUnitName = load.TransportUnit?.Name;
                var newTransportUnitName = transportUnitId.HasValue 
                    ? (await _context.TransportUnits.FindAsync(transportUnitId))?.Name 
                    : null;

                // Update transport unit assignment
                load.TransportUnitId = transportUnitId;
                
                // If transport unit is assigned, update status to InProgress
                if (transportUnitId.HasValue)
                {
                    load.Status = JobStatus.InProgress;
                    
                    // Update job status if all loads are in progress
                    var allLoadsInProgress = await _context.Loads
                        .Where(l => l.JobId == jobId)
                        .AllAsync(l => l.Status == JobStatus.InProgress);
                    
                    if (allLoadsInProgress)
                    {
                        load.Job.Status = JobStatus.InProgress;
                    }

                    TempData["SuccessMessage"] = $"Transport unit '{newTransportUnitName}' has been assigned to the load.";
                }
                else
                {
                    load.Status = JobStatus.Pending;
                    load.Job.Status = JobStatus.Pending;
                    TempData["SuccessMessage"] = oldTransportUnitName != null 
                        ? $"Transport unit '{oldTransportUnitName}' has been unassigned from the load."
                        : "Transport unit has been unassigned from the load.";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while assigning transport unit: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLoadStatus(int jobId, int loadId, JobStatus status)
        {
            try
            {
                var load = await _context.Loads
                    .Include(l => l.Job)
                    .Include(l => l.TransportUnit)
                    .FirstOrDefaultAsync(l => l.LoadId == loadId && l.JobId == jobId);

                if (load == null)
                {
                    TempData["ErrorMessage"] = "Load not found.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                var oldStatus = load.Status;
                
                // Update load status
                load.Status = status;

                // If load is completed or cancelled, unassign the transport unit
                if (status == JobStatus.Completed || status == JobStatus.Cancelled)
                {
                    // Store the old transport unit name for message
                    var oldTransportUnitName = load.TransportUnit?.Name;
                    if (oldTransportUnitName != null)
                    {
                        TempData["SuccessMessage"] = $"Transport Unit '{oldTransportUnitName}' has been unassigned as the load is {status.ToString().ToLower()}.";
                    }
                    load.TransportUnitId = null;
                }
                
                // Update job status based on load statuses
                var allLoads = await _context.Loads
                    .Where(l => l.JobId == jobId)
                    .ToListAsync();

                if (allLoads.All(l => l.Status == JobStatus.Completed))
                {
                    load.Job.Status = JobStatus.Completed;
                    
                    // Unassign transport units from all loads if not already unassigned
                    foreach (var jobLoad in allLoads.Where(l => l.TransportUnitId.HasValue))
                    {
                        jobLoad.TransportUnitId = null;
                    }
                    
                    TempData["SuccessMessage"] = "Job has been marked as completed. All transport units have been unassigned.";
                }
                else if (allLoads.Any(l => l.Status == JobStatus.Cancelled))
                {
                    load.Job.Status = JobStatus.Cancelled;
                    TempData["SuccessMessage"] = "Job status has been updated to Cancelled.";
                }
                else if (allLoads.Any(l => l.Status == JobStatus.InProgress))
                {
                    load.Job.Status = JobStatus.InProgress;
                    TempData["SuccessMessage"] = "Job status has been updated to In Progress.";
                }
                else
                {
                    load.Job.Status = JobStatus.Pending;
                    TempData["SuccessMessage"] = "Job status has been updated to Pending.";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating load status: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = jobId });
        }
    }
} 