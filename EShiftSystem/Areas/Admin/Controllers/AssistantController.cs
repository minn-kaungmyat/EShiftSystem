using EShiftSystem.Data;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AssistantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssistantController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var assistants = await _context.Assistants
                    .Include(a => a.TransportUnit)
                    .ToListAsync();
                return View(assistants);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading assistants: " + ex.Message;
                return View(new List<Assistant>());
            }
        }

        public IActionResult Create()
        {
            try
            {
                ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name");
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while preparing the create form: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assistant assistant)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(assistant);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Assistant '{assistant.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name", assistant.TransportUnitId);
                return View(assistant);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the assistant: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Assistant ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var assistant = await _context.Assistants.FindAsync(id);
                if (assistant == null)
                {
                    TempData["ErrorMessage"] = "Assistant not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name", assistant.TransportUnitId);
                return View(assistant);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the assistant: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Assistant assistant)
        {
            try
            {
                if (id != assistant.AssistantId)
                {
                    TempData["ErrorMessage"] = "Invalid assistant ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(assistant);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Assistant '{assistant.Name}' has been updated successfully.";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!AssistantExists(assistant.AssistantId))
                        {
                            TempData["ErrorMessage"] = "Assistant no longer exists.";
                            return RedirectToAction(nameof(Index));
                        }
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }

                ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name", assistant.TransportUnitId);
                return View(assistant);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the assistant: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Assistant ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var assistant = await _context.Assistants
                    .Include(a => a.TransportUnit)
                    .FirstOrDefaultAsync(m => m.AssistantId == id);
                if (assistant == null)
                {
                    TempData["ErrorMessage"] = "Assistant not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(assistant);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the assistant: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var assistant = await _context.Assistants.FindAsync(id);
                if (assistant == null)
                {
                    TempData["ErrorMessage"] = "Assistant not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Assistants.Remove(assistant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Assistant '{assistant.Name}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the assistant: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool AssistantExists(int id)
        {
            return _context.Assistants.Any(e => e.AssistantId == id);
        }
    }
}
