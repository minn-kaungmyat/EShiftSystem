// EShiftSystem/Areas/Customer/Controllers/JobController.cs
using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Models.Enums;
using EShiftSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Required for UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Required for .FirstOrDefaultAsync()

namespace EShiftSystem.Areas.Customer.Controllers
{
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Add UserManager

        public JobController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) // Inject UserManager
        {
            _context = context;
            _userManager = userManager; // Initialize UserManager
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Jobs.ToListAsync());
        }

        // GET: /Customer/Job/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new JobCreateViewModel();
            // Start the user off with one blank load form
            // Ensure LoadViewModel also initializes its Products list if it's not nullable
            viewModel.Loads.Add(new LoadViewModel { Products = new List<ProductViewModel>() });
            // Optionally, add one blank product to the first load
            viewModel.Loads[0].Products.Add(new ProductViewModel());

            ViewBag.TransportUnits = new SelectList(_context.TransportUnits, "TransportUnitId", "Name");
            return View(viewModel);
        }

        // POST: /Customer/Job/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCreateViewModel viewModel)
        {
            // Repopulate ViewBag in case ModelState is invalid and we return the view
            ViewBag.TransportUnits = new SelectList(_context.TransportUnits, "TransportUnitId", "Name");

            if (ModelState.IsValid)
            {
                // Get the currently logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    // Handle case where user is not found (e.g., redirect to login)
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get the Customer entity associated with the current user
                var customer = await _context.Customers
                                             .FirstOrDefaultAsync(c => c.ApplicationUserId == currentUser.Id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer profile not found. Please complete your profile.";
                    // Consider redirecting to a profile creation page or displaying an error
                    return View(viewModel);
                }

                // 1. Create the main Job entity
                var newJob = new Job
                {
                    JobTitle = viewModel.JobTitle,
                    Description = viewModel.Description,
                    JobDate = viewModel.JobDate,
                    Priority = viewModel.Priority,
                    Status = JobStatus.Pending, // Set initial status as Pending
                    CustomerId = customer.CustomerId, // Link to the current customer
                    CreatedAt = DateTime.Now // Set creation date
                };

                // Initialize Loads and Products collections to prevent null reference issues
                newJob.Loads = new List<Load>();

                // 2. Loop through the LoadViewModels to create Load entities
                if (viewModel.Loads != null)
                {
                    foreach (var loadVm in viewModel.Loads)
                    {
                        var newLoad = new Load
                        {
                            StartLocation = loadVm.StartLocation,
                            Destination = loadVm.Destination,
                            TransportUnitId = loadVm.TransportUnitId,
                            Job = newJob // Link to the parent job
                        };

                        // Initialize Products collection for the load
                        newLoad.Products = new List<Product>();

                        // 3. NESTED LOOP: Loop through ProductViewModels to create Product entities
                        if (loadVm.Products != null)
                        {
                            foreach (var productVm in loadVm.Products)
                            {
                                // Only add products if they have a name (or other required fields)
                                if (!string.IsNullOrWhiteSpace(productVm.Name))
                                {
                                    var newProduct = new Product
                                    {
                                        Name = productVm.Name,
                                        Quantity = productVm.Quantity,
                                        Weight = productVm.Weight,
                                        Load = newLoad // Link this product to its parent load
                                    };
                                    newLoad.Products.Add(newProduct);
                                }
                            }
                        }
                        // Only add loads if they have a start location (or other required fields)
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

            // If ModelState is not valid, return the view with the current viewModel
            return View(viewModel);
        }
    }
}