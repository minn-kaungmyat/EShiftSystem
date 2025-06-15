using EShiftSystem.Data;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DriverController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.Drivers.ToListAsync());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading drivers: " + ex.Message;
                return View(new List<Driver>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Driver driver)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(driver);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Driver '{driver.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(driver);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the driver: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Driver ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var driver = await _context.Drivers.FindAsync(id);
                if (driver == null)
                {
                    TempData["ErrorMessage"] = "Driver not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(driver);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the driver: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Driver driver)
        {
            try
            {
                if (id != driver.DriverId)
                {
                    TempData["ErrorMessage"] = "Invalid driver ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(driver);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Driver '{driver.Name}' has been updated successfully.";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!DriverExists(driver.DriverId))
                        {
                            TempData["ErrorMessage"] = "Driver no longer exists.";
                            return RedirectToAction(nameof(Index));
                        }
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(driver);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the driver: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Driver ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var driver = await _context.Drivers.FirstOrDefaultAsync(m => m.DriverId == id);
                if (driver == null)
                {
                    TempData["ErrorMessage"] = "Driver not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(driver);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the driver: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var driver = await _context.Drivers.FindAsync(id);
                if (driver == null)
                {
                    TempData["ErrorMessage"] = "Driver not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Drivers.Remove(driver);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Driver '{driver.Name}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the driver: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.DriverId == id);
        }
    }
}
