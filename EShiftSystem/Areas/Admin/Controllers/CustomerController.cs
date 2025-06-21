using EShiftSystem.Data;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CustomerModel = EShiftSystem.Models.Customer;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Customer
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _context.Customers
                    .Include(c => c.ApplicationUser)
                    .Include(c => c.Jobs)
                    .ToListAsync(); // Show all customers (active and inactive)
                return View(customers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading customers: " + ex.Message;
                return View(new List<CustomerModel>());
            }
        }

        // GET: Admin/Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Customer ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var customer = await _context.Customers
                    .Include(c => c.ApplicationUser)
                    .Include(c => c.Jobs!)
                        .ThenInclude(j => j.Loads)
                    .FirstOrDefaultAsync(m => m.CustomerId == id);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the customer: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }



        // POST: Admin/Customer/Reactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Reactivate customer
                customer.IsActive = true;
                _context.Update(customer);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Customer '{customer.FullName}' has been reactivated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while reactivating the customer: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Note: Customer creation is handled through the registration process
        // Admins can only edit existing customer information

        // GET: Admin/Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Customer ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var customer = await _context.Customers
                    .Include(c => c.ApplicationUser)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                // For edit, we only show the current user (can't change the associated user)
                ViewData["ApplicationUserDisplay"] = $"{customer.ApplicationUser.UserName} ({customer.ApplicationUser.Email})";
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the customer: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerModel customer)
        {
            try
            {
                if (id != customer.CustomerId)
                {
                    TempData["ErrorMessage"] = "Invalid customer ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(customer);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Customer '{customer.FullName}' has been updated successfully.";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CustomerExists(customer.CustomerId))
                        {
                            TempData["ErrorMessage"] = "Customer no longer exists.";
                            return RedirectToAction(nameof(Index));
                        }
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }

                // Reload display info for the form
                var applicationUser = await _userManager.FindByIdAsync(customer.ApplicationUserId);
                ViewData["ApplicationUserDisplay"] = $"{applicationUser?.UserName} ({applicationUser?.Email})";
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the customer: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Customer/Deactivate/5 (Soft delete via modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete - set IsActive to false
                customer.IsActive = false;
                _context.Update(customer);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Customer '{customer.FullName}' has been deactivated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deactivating the customer: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
} 