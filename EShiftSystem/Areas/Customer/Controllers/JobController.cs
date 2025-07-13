// EShiftSystem/Areas/Customer/Controllers/JobController.cs
using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Models.Enums;
using EShiftSystem.Services;
using EShiftSystem.ViewModels;
using EShiftSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EShiftSystem.Areas.Customer.Controllers
{
    // customer job controller for creating, editing and managing personal jobs
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJobNumberGenerator _jobNumberGenerator;
        private const int PageSize = 10;

        // initializes controller with database context, user manager and job number generator
        public JobController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IJobNumberGenerator jobNumberGenerator)
        {
            _context = context;
            _userManager = userManager;
            _jobNumberGenerator = jobNumberGenerator;
        }

        // displays list of customer's jobs ordered by creation date
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            
            var jobsQuery = _context.Jobs
                .Where(j => j.CustomerId == customer.CustomerId)
                .OrderByDescending(j => j.CreatedAt);

            var pageIndex = pageNumber ?? 1;
            var jobs = await PaginatedList<Job>.CreateAsync(jobsQuery, pageIndex, PageSize);

            return View(jobs);
        }

        // GET: /Customer/Job/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            
            var job = await _context.Jobs
                .Include(j => j.Loads)
                    .ThenInclude(l => l.LoadItems)
                .FirstOrDefaultAsync(j => j.JobId == id && j.CustomerId == customer.CustomerId);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // displays job creation form with initial empty load and item
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new JobCreateViewModel
            {
                JobTitle = "",
                StartLocation = "",
                Destination = ""
            };
            // Start the user off with one blank load form
            viewModel.Loads.Add(new LoadViewModel { LoadItems = new List<LoadItemViewModel>() });
            // Optionally, add one blank item to the first load
            viewModel.Loads[0].LoadItems.Add(new LoadItemViewModel { ItemType = "" });

            return View(viewModel);
        }

        // POST: /Customer/Job/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var customer = await _context.Customers
                                             .FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer profile not found. Please complete your profile.";
                    return View(viewModel);
                }

                var newJob = new Job
                {
                    JobNumber = await _jobNumberGenerator.GenerateJobNumberAsync(),
                    JobTitle = viewModel.JobTitle,
                    Description = viewModel.Description,
                    JobDate = viewModel.JobDate,
                    Priority = viewModel.Priority,
                    StartLocation = viewModel.StartLocation,
                    Destination = viewModel.Destination,
                    StartLatitude = viewModel.StartLatitude,
                    StartLongitude = viewModel.StartLongitude,
                    DestinationLatitude = viewModel.DestinationLatitude,
                    DestinationLongitude = viewModel.DestinationLongitude,
                    Status = JobStatus.Pending,
                    CustomerId = customer.CustomerId,
                    Customer = customer,
                    CreatedAt = DateTime.Now,
                    Loads = new List<Load>()
                };

                if (viewModel.Loads != null)
                {
                    foreach (var loadVm in viewModel.Loads)
                    {
                        var newLoad = new Load
                        {
                            Job = newJob,
                            LoadItems = new List<LoadItem>()
                        };

                        if (loadVm.LoadItems != null)
                        {
                            foreach (var loadItemVm in loadVm.LoadItems)
                            {
                                if (!string.IsNullOrWhiteSpace(loadItemVm.ItemType))
                                {
                                    var newLoadItem = new LoadItem
                                    {
                                        ItemType = loadItemVm.ItemType,
                                        Description = loadItemVm.Description,
                                        Quantity = loadItemVm.Quantity,
                                        WeightKg = loadItemVm.WeightKg,
                                        SpecialInstructions = loadItemVm.SpecialInstructions,
                                        Load = newLoad
                                    };
                                    newLoad.LoadItems.Add(newLoadItem);
                                }
                            }
                        }

                        // Add load if it has items
                        if (newLoad.LoadItems.Any())
                        {
                            newJob.Loads.Add(newLoad);
                        }
                    }
                }

                _context.Jobs.Add(newJob);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Job successfully created!";
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        // GET: /Customer/Job/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            
            var job = await _context.Jobs
                .Include(j => j.Loads)
                    .ThenInclude(l => l.LoadItems)
                .FirstOrDefaultAsync(j => j.JobId == id && j.CustomerId == customer.CustomerId);

            if (job == null)
            {
                return NotFound();
            }

            if (job.Status != JobStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending jobs can be edited. Cancelled jobs cannot be modified.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new JobCreateViewModel
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                Description = job.Description,
                JobDate = job.JobDate,
                Priority = job.Priority,
                StartLocation = job.StartLocation,
                Destination = job.Destination,
                StartLatitude = job.StartLatitude,
                StartLongitude = job.StartLongitude,
                DestinationLatitude = job.DestinationLatitude,
                DestinationLongitude = job.DestinationLongitude,
                Loads = job.Loads.Select(l => new LoadViewModel
                {
                    LoadItems = l.LoadItems.Select(li => new LoadItemViewModel
                    {
                        ItemType = li.ItemType,
                        Description = li.Description,
                        Quantity = li.Quantity,
                        WeightKg = li.WeightKg,
                        SpecialInstructions = li.SpecialInstructions,

                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Customer/Job/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobCreateViewModel viewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            
            var job = await _context.Jobs
                .Include(j => j.Loads)
                    .ThenInclude(l => l.LoadItems)
                .FirstOrDefaultAsync(j => j.JobId == id && j.CustomerId == customer.CustomerId);

            if (job == null)
            {
                return NotFound();
            }

            if (job.Status != JobStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending jobs can be edited. Cancelled jobs cannot be modified.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update job properties
                    job.JobTitle = viewModel.JobTitle;
                    job.Description = viewModel.Description;
                    job.JobDate = viewModel.JobDate;
                    job.Priority = viewModel.Priority;
                    job.StartLocation = viewModel.StartLocation;
                    job.Destination = viewModel.Destination;
                    job.StartLatitude = viewModel.StartLatitude;
                    job.StartLongitude = viewModel.StartLongitude;
                    job.DestinationLatitude = viewModel.DestinationLatitude;
                    job.DestinationLongitude = viewModel.DestinationLongitude;
                    job.UpdatedAt = DateTime.Now;

                    // Remove existing loads and items
                    foreach (var load in job.Loads.ToList())
                    {
                        foreach (var item in load.LoadItems.ToList())
                        {
                            _context.LoadItems.Remove(item);
                        }
                        _context.Loads.Remove(load);
                    }

                    // Add new loads and items
                    foreach (var loadVm in viewModel.Loads)
                    {
                        var newLoad = new Load
                        {
                            Job = job,
                            LoadItems = new List<LoadItem>()
                        };

                        if (loadVm.LoadItems != null)
                        {
                            foreach (var loadItemVm in loadVm.LoadItems)
                            {
                                if (!string.IsNullOrWhiteSpace(loadItemVm.ItemType))
                                {
                                    var newLoadItem = new LoadItem
                                    {
                                        ItemType = loadItemVm.ItemType,
                                        Description = loadItemVm.Description,
                                        Quantity = loadItemVm.Quantity,
                                        WeightKg = loadItemVm.WeightKg,
                                        SpecialInstructions = loadItemVm.SpecialInstructions,
                                        Load = newLoad
                                    };
                                    newLoad.LoadItems.Add(newLoadItem);
                                }
                            }
                        }

                        // Add load if it has items
                        if (newLoad.LoadItems.Any())
                        {
                            job.Loads.Add(newLoad);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Job updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(viewModel);
        }

        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.JobId == id);
        }

        // GET: /Customer/Job/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            
            var job = await _context.Jobs
                .Include(j => j.Loads)
                    .ThenInclude(l => l.LoadItems)
                .FirstOrDefaultAsync(j => j.JobId == id && j.CustomerId == customer.CustomerId);

            if (job == null)
            {
                return NotFound();
            }

            if (job.Status != JobStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending jobs can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            return View(job);
        }

        // POST: /Customer/Job/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            
            var job = await _context.Jobs
                .Include(j => j.Loads)
                    .ThenInclude(l => l.LoadItems)
                .Include(j => j.Loads)
                    .ThenInclude(l => l.TransportUnit)
                .FirstOrDefaultAsync(j => j.JobId == id && j.CustomerId == customer.CustomerId);

            if (job == null)
            {
                TempData["ErrorMessage"] = "Job not found.";
                return RedirectToAction(nameof(Index));
            }

            if (job.Status != JobStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending jobs can be deleted.";
                return RedirectToAction(nameof(Details), new { id = job.JobId });
            }

            try
            {
                // Remove all loads and their items, and release transport units
                foreach (var load in job.Loads.ToList())
                {
                    // Release transport unit if assigned
                    if (load.TransportUnit != null)
                    {
                        load.TransportUnit.Status = TransportUnitStatus.Available;
                    }
                    
                    foreach (var item in load.LoadItems.ToList())
                    {
                        _context.LoadItems.Remove(item);
                    }
                    _context.Loads.Remove(load);
                }

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the job.";
                return RedirectToAction(nameof(Details), new { id = job.JobId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkLoadCompleted(int jobId, int loadId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);

                var load = await _context.Loads
                    .Include(l => l.Job)
                    .Include(l => l.TransportUnit)
                    .FirstOrDefaultAsync(l => l.LoadId == loadId && l.JobId == jobId && l.Job.CustomerId == customer.CustomerId);

                if (load == null)
                {
                    TempData["ErrorMessage"] = "Load not found or you don't have permission to update it.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                if (load.Status != JobStatus.Delivered)
                {
                    TempData["ErrorMessage"] = "Only delivered loads can be marked as completed.";
                    return RedirectToAction(nameof(Details), new { id = jobId });
                }

                // Mark load as completed and unassign transport unit
                load.Status = JobStatus.Completed;
                var transportUnitName = load.TransportUnit?.Name;
                
                // Set transport unit status back to Available
                if (load.TransportUnit != null)
                {
                    load.TransportUnit.Status = TransportUnitStatus.Available;
                }
                
                load.TransportUnitId = null;

                // Check if all loads in the job are completed
                var allLoads = await _context.Loads
                    .Where(l => l.JobId == jobId)
                    .ToListAsync();

                if (allLoads.All(l => l.Status == JobStatus.Completed))
                {
                    load.Job.Status = JobStatus.Completed;
                    load.Job.UpdatedAt = DateTime.Now;
                    
                    TempData["SuccessMessage"] = $"Load marked as completed! All loads completed - job is now complete. Transport unit '{transportUnitName}' has been unassigned.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Load marked as completed! Transport unit '{transportUnitName}' has been unassigned.";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating load status: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = jobId });
        }

        // POST: /Customer/Job/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            
            var job = await _context.Jobs
                .Include(j => j.Loads)
                    .ThenInclude(l => l.LoadItems)
                .Include(j => j.Loads)
                    .ThenInclude(l => l.TransportUnit)
                .FirstOrDefaultAsync(j => j.JobId == id && j.CustomerId == customer.CustomerId);

            if (job == null)
            {
                TempData["ErrorMessage"] = "Job not found.";
                return RedirectToAction(nameof(Index));
            }

            if (job.Status != JobStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending jobs can be cancelled.";
                return RedirectToAction(nameof(Details), new { id = job.JobId });
            }

            try
            {
                // Update job status to cancelled instead of deleting
                job.Status = JobStatus.Cancelled;
                job.UpdatedAt = DateTime.Now;
                
                // Update all loads to cancelled status
                foreach (var load in job.Loads)
                {
                    load.Status = JobStatus.Cancelled;
                    
                    // Release transport unit if assigned (though this shouldn't happen for pending jobs)
                    if (load.TransportUnit != null)
                    {
                        load.TransportUnit.Status = TransportUnitStatus.Available;
                        load.TransportUnitId = null;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job cancelled successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling the job.";
                return RedirectToAction(nameof(Details), new { id = job.JobId });
            }
        }
    }
}