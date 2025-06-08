// EShiftSystem/Areas/Customer/Controllers/JobController.cs
using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Models.Enums;
using EShiftSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EShiftSystem.Areas.Customer.Controllers
{
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
            return View(await _context.Jobs.Where(j => j.CustomerId == customer.CustomerId).ToListAsync());
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

        // GET: /Customer/Job/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new JobCreateViewModel();
            // Start the user off with one blank load form
            // Changed initialization to use LoadItems and LoadItemViewModel
            viewModel.Loads.Add(new LoadViewModel { LoadItems = new List<LoadItemViewModel>() });
            // Optionally, add one blank item to the first load
            viewModel.Loads[0].LoadItems.Add(new LoadItemViewModel());

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
                    JobTitle = viewModel.JobTitle,
                    Description = viewModel.Description,
                    JobDate = viewModel.JobDate,
                    Priority = viewModel.Priority,
                    Status = JobStatus.Pending,
                    CustomerId = customer.CustomerId,
                    CreatedAt = DateTime.Now,
                    Loads = new List<Load>()
                };

                if (viewModel.Loads != null)
                {
                    foreach (var loadVm in viewModel.Loads)
                    {
                        var newLoad = new Load
                        {
                            StartLocation = loadVm.StartLocation,
                            Destination = loadVm.Destination,
                            Job = newJob,
                            // Changed from Products to LoadItems
                            LoadItems = new List<LoadItem>()
                        };

                        // Changed from loadVm.Products to loadVm.LoadItems
                        if (loadVm.LoadItems != null)
                        {
                            // Changed from productVm to loadItemVm
                            foreach (var loadItemVm in loadVm.LoadItems)
                            {
                                // Changed from productVm.Name to loadItemVm.ItemType
                                if (!string.IsNullOrWhiteSpace(loadItemVm.ItemType))
                                {
                                    // Create a new LoadItem, not Product
                                    var newLoadItem = new LoadItem
                                    {
                                        ItemType = loadItemVm.ItemType,
                                        Quantity = loadItemVm.Quantity,
                                        Note = loadItemVm.Note,
                                        Load = newLoad
                                    };
                                    newLoad.LoadItems.Add(newLoadItem);
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(newLoad.StartLocation))
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
                TempData["ErrorMessage"] = "Only pending jobs can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new JobCreateViewModel
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                Description = job.Description,
                JobDate = job.JobDate,
                Priority = job.Priority,
                Loads = job.Loads.Select(l => new LoadViewModel
                {
                    StartLocation = l.StartLocation,
                    Destination = l.Destination,
                    LoadItems = l.LoadItems.Select(li => new LoadItemViewModel
                    {
                        ItemType = li.ItemType,
                        Quantity = li.Quantity,
                        Note = li.Note
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
                TempData["ErrorMessage"] = "Only pending jobs can be edited.";
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
                            StartLocation = loadVm.StartLocation,
                            Destination = loadVm.Destination,
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
                                        Quantity = loadItemVm.Quantity,
                                        Note = loadItemVm.Note,
                                        Load = newLoad
                                    };
                                    newLoad.LoadItems.Add(newLoadItem);
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(newLoad.StartLocation))
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
                // Remove all loads and their items
                foreach (var load in job.Loads.ToList())
                {
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
    }
}