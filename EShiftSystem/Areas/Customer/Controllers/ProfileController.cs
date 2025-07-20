using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShiftSystem.Data;
using EShiftSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Areas.Customer.Controllers
{
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Customer/Profile
        public async Task<IActionResult> Index()
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
                TempData["ErrorMessage"] = "Customer profile not found.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var viewModel = new ProfileViewModel
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Address = customer.Address ?? "",
                Phone = customer.Phone ?? "",
                Email = currentUser.Email ?? ""
            };

            ViewBag.ActiveTab = TempData["ActiveTab"] ?? "profile"; // Use TempData if available, otherwise default to profile tab
            return View(viewModel);
        }

        // POST: Customer/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            // Check if user wants to change password
            bool isPasswordChange = !string.IsNullOrEmpty(model.CurrentPassword) || !string.IsNullOrEmpty(model.NewPassword);
            
            if (isPasswordChange)
            {
                // Validate password fields
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to change password.");
                }
                if (string.IsNullOrEmpty(model.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "New password is required.");
                }
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "New password and confirmation password do not match.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Set active tab based on validation errors
                ViewBag.ActiveTab = isPasswordChange ? "password" : "profile";
                return View(model);
            }

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
                TempData["ErrorMessage"] = "Customer profile not found.";
                return View(model);
            }

            try
            {
                // Update Customer data
                customer.FullName = model.FullName;
                customer.Address = model.Address;
                customer.Phone = model.Phone;

                // Save Customer changes
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                var successMessage = "Profile updated successfully!";

                // Handle password change if requested
                if (isPasswordChange && !string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
                {
                    var changePasswordResult = await _userManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);
                    if (changePasswordResult.Succeeded)
                    {
                        successMessage = "Profile and password updated successfully!";
                    }
                    else
                    {
                        // Add password change errors to ModelState
                        foreach (var error in changePasswordResult.Errors)
                        {
                            ModelState.AddModelError("CurrentPassword", error.Description);
                        }
                        ViewBag.ActiveTab = "password";
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = successMessage;
                // Set active tab based on the operation performed
                TempData["ActiveTab"] = isPasswordChange ? "password" : "profile";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                ViewBag.ActiveTab = isPasswordChange ? "password" : "profile";
                return View(model);
            }
        }
    }

    // Profile ViewModel
    public class ProfileViewModel
    {
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Address")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // Password change fields
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string? NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
} 