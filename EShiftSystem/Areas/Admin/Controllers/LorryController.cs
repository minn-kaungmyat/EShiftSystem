using EShiftSystem.Data;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class LorryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LorryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lorry
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.Lorries.ToListAsync());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading lorries: " + ex.Message;
                return View(new List<Lorry>());
            }
        }

        // GET: Lorry/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Lorry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lorry lorry)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(lorry);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Lorry '{lorry.LicensePlate}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(lorry);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the lorry: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Lorry/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Lorry ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var lorry = await _context.Lorries.FindAsync(id);
                if (lorry == null)
                {
                    TempData["ErrorMessage"] = "Lorry not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(lorry);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the lorry: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Lorry/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lorry lorry)
        {
            try
            {
                if (id != lorry.LorryId)
                {
                    TempData["ErrorMessage"] = "Invalid lorry ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(lorry);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Lorry '{lorry.LicensePlate}' has been updated successfully.";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!LorryExists(lorry.LorryId))
                        {
                            TempData["ErrorMessage"] = "Lorry no longer exists.";
                            return RedirectToAction(nameof(Index));
                        }
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(lorry);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the lorry: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Lorry/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Lorry ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var lorry = await _context.Lorries
                    .FirstOrDefaultAsync(m => m.LorryId == id);
                if (lorry == null)
                {
                    TempData["ErrorMessage"] = "Lorry not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(lorry);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the lorry: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Lorry/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var lorry = await _context.Lorries.FindAsync(id);
                if (lorry == null)
                {
                    TempData["ErrorMessage"] = "Lorry not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Lorries.Remove(lorry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Lorry '{lorry.LicensePlate}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the lorry: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool LorryExists(int id)
        {
            return _context.Lorries.Any(e => e.LorryId == id);
        }
    }
}
