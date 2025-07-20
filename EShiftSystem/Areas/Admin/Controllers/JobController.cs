using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Models.Enums;
using EShiftSystem.ViewModels;
using EShiftSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Areas.Admin.Controllers
{
    // admin job controller for managing job approval, transport assignment and status updates
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        // initializes job controller with database context
        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        // displays list of all jobs with customer and transport information
        public async Task<IActionResult> Index(int? pageNumber)
        {
            try
            {
                var jobsQuery = _context.Jobs
                    .Include(j => j.Customer)
                    .Include(j => j.Loads)
                        .ThenInclude(l => l.TransportUnit)
                    .OrderByDescending(j => j.CreatedAt);

                var pageIndex = pageNumber ?? 1;
                var jobs = await PaginatedList<Job>.CreateAsync(jobsQuery, pageIndex, PageSize);

                return View(jobs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading jobs: " + ex.Message;
                return View(new PaginatedList<Job>(new List<Job>(), 0, 1, PageSize));
            }
        }

        // displays detailed job information with transport assignment options
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

                // Get available transport units for assignment (only Available status)
                var availableTransportUnits = await _context.TransportUnits
                    .Include(tu => tu.Driver)
                    .Include(tu => tu.Container)
                    .Include(tu => tu.Assistants)
                    .Where(tu => tu.Status == TransportUnitStatus.Available)
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

                // Check if job is approved (not pending) before allowing transport unit assignment
                if (load.Job.Status == JobStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Transport units can only be assigned after the job is approved. Please approve the job first.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                // Only allow transport unit assignment to loads that are in Pending status
                if (load.Status != JobStatus.Pending)
                {
                    TempData["ErrorMessage"] = $"Transport units can only be assigned to pending loads. Current load status: {load.Status}";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                var oldTransportUnit = load.TransportUnit;
                var oldTransportUnitName = oldTransportUnit?.Name;
                var newTransportUnit = transportUnitId.HasValue 
                    ? await _context.TransportUnits.FindAsync(transportUnitId)
                    : null;
                var newTransportUnitName = newTransportUnit?.Name;

                // Check if the new transport unit is available (if assigning)
                if (transportUnitId.HasValue && newTransportUnit?.Status != TransportUnitStatus.Available)
                {
                    TempData["ErrorMessage"] = "Selected transport unit is not available for assignment.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                // Update previous transport unit status to Available (if unassigning)
                if (oldTransportUnit != null)
                {
                    oldTransportUnit.Status = TransportUnitStatus.Available;
                }

                // Update transport unit assignment
                load.TransportUnitId = transportUnitId;

                // Update new transport unit status to Assigned (if assigning)
                if (newTransportUnit != null)
                {
                    newTransportUnit.Status = TransportUnitStatus.Assigned;
                }
                
                // If transport unit is assigned, update status to InProgress
                if (transportUnitId.HasValue)
                {
                    load.Status = JobStatus.InProgress;
                    
                    // Update job status to InProgress if not already in progress or further
                    if (load.Job.Status == JobStatus.Approved)
                    {
                        load.Job.Status = JobStatus.InProgress;
                    }

                    TempData["SuccessMessage"] = $"Transport unit '{newTransportUnitName}' has been assigned to the load.";
                }
                else
                {
                    load.Status = JobStatus.Pending; // Reset to pending when unassigning
                    
                    // Check current status of all loads to determine job status
                    var allLoads = await _context.Loads
                        .Where(l => l.JobId == jobId)
                        .ToListAsync();
                    
                    // Update the current load in the list
                    var currentLoadInList = allLoads.FirstOrDefault(l => l.LoadId == loadId);
                    if (currentLoadInList != null)
                    {
                        currentLoadInList.Status = JobStatus.Pending;
                    }
                    
                    // Determine job status based on all loads
                    if (allLoads.All(l => l.Status == JobStatus.Pending))
                    {
                        load.Job.Status = JobStatus.Approved;
                    }
                    else if (allLoads.Any(l => l.Status == JobStatus.InProgress))
                    {
                        load.Job.Status = JobStatus.InProgress;
                    }
                    else if (allLoads.All(l => l.Status == JobStatus.Delivered))
                    {
                        load.Job.Status = JobStatus.Delivered;
                    }
                    else if (allLoads.All(l => l.Status == JobStatus.Completed))
                    {
                        load.Job.Status = JobStatus.Completed;
                    }
                    
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

                // Admin can only update InProgress loads to Delivered
                if (load.Status != JobStatus.InProgress)
                {
                    TempData["ErrorMessage"] = "Only loads in progress can be marked as delivered.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                if (status != JobStatus.Delivered)
                {
                    TempData["ErrorMessage"] = "Admins can only mark loads as delivered.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                // Update load status to Delivered
                load.Status = JobStatus.Delivered;

                // Check the status of all loads to determine job status
                var allLoads = await _context.Loads
                    .Where(l => l.JobId == jobId)
                    .ToListAsync();

                // Determine job status based on all loads
                if (allLoads.All(l => l.Status == JobStatus.Completed))
                {
                    // All loads are completed by customer
                    load.Job.Status = JobStatus.Completed;
                    load.Job.UpdatedAt = DateTime.Now;
                    TempData["SuccessMessage"] = "Load marked as delivered! All loads are completed - job status updated to Completed.";
                }
                else if (allLoads.All(l => l.Status == JobStatus.Delivered || l.Status == JobStatus.Completed))
                {
                    // All loads are delivered (waiting for customer confirmation) or completed
                    load.Job.Status = JobStatus.Delivered;
                    load.Job.UpdatedAt = DateTime.Now;
                    TempData["SuccessMessage"] = "Load marked as delivered! All loads delivered - job status updated to Delivered.";
                }
                else
                {
                    // Some loads are still in progress
                    TempData["SuccessMessage"] = "Load marked as delivered successfully.";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating load status: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveJob(int jobId)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(jobId);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Job not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (job.Status != JobStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending jobs can be approved.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                job.Status = JobStatus.Approved;
                job.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Job has been approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while approving the job: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelJob(int jobId, string cancellationReason = "")
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Loads)
                        .ThenInclude(l => l.TransportUnit)
                    .FirstOrDefaultAsync(j => j.JobId == jobId);
                    
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Job not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (job.Status == JobStatus.Completed || job.Status == JobStatus.Cancelled)
                {
                    TempData["ErrorMessage"] = "Completed or already cancelled jobs cannot be cancelled.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                job.Status = JobStatus.Cancelled;
                job.UpdatedAt = DateTime.Now;
                
                // Update all loads to cancelled and release transport units
                foreach (var load in job.Loads)
                {
                    load.Status = JobStatus.Cancelled;
                    
                    // Release transport unit if assigned
                    if (load.TransportUnit != null)
                    {
                        load.TransportUnit.Status = TransportUnitStatus.Available;
                        load.TransportUnitId = null;
                    }
                }
                
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = string.IsNullOrEmpty(cancellationReason) 
                    ? "Job has been cancelled." 
                    : $"Job has been cancelled. Reason: {cancellationReason}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling the job: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = jobId });
        }
    }
} 