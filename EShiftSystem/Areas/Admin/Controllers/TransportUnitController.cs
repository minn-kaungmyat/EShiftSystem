using EShiftSystem.Data;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using EShiftSystem.ViewModels;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class TransportUnitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportUnitController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var transportUnits = _context.TransportUnits
                    .Include(t => t.Lorry)
                    .Include(t => t.Driver)
                    .Include(t => t.Container)
                    .Include(t => t.Assistants);
                return View(await transportUnits.ToListAsync());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading transport units: " + ex.Message;
                return View(new List<TransportUnit>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                var viewModel = new TransportUnitFormViewModel();
                var availableLorries = _context.Lorries
                    .Where(l => !_context.TransportUnits.Any(t => t.LorryId == l.LorryId))
                    .ToList();
               
                ViewData["LorryId"] = new SelectList(availableLorries, "LorryId", "LicensePlate", 0);

                var availableDrivers = _context.Drivers
                    .Where(d => !_context.TransportUnits.Any(t => t.DriverId == d.DriverId))
                    .ToList();
                
                ViewData["DriverId"] = new SelectList(availableDrivers, "DriverId", "Name", 0);

                var availableContainers = _context.Containers
                    .Where(c => !_context.TransportUnits.Any(t => t.ContainerId == c.ContainerId))
                    .ToList();
               
                ViewData["ContainerId"] = new SelectList(availableContainers, "ContainerId", "Type", 0);
                var availableAssistants = _context.Assistants
                    .Where(a => a.TransportUnitId == null)
                    .ToList();
                ViewData["AssistantIds"] = new MultiSelectList(availableAssistants, "AssistantId", "Name");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while preparing the create form: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransportUnitFormViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.TransportUnits.Add(model.TransportUnit);
                    await _context.SaveChangesAsync();

                    var selectedAssistants = _context.Assistants
                        .Where(a => model.SelectedAssistantIds.Contains(a.AssistantId));

                    foreach (var assistant in selectedAssistants)
                    {
                        assistant.TransportUnitId = model.TransportUnit.TransportUnitId;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Transport unit '{model.TransportUnit.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // Reload dropdowns on error
                ViewData["LorryId"] = new SelectList(_context.Lorries, "LorryId", "LicensePlate");
                ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name");
                ViewData["ContainerId"] = new SelectList(_context.Containers, "ContainerId", "Type");
                ViewData["AssistantIds"] = new MultiSelectList(_context.Assistants, "AssistantId", "Name");

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the transport unit: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Transport unit ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var unit = await _context.TransportUnits
                    .Include(t => t.Assistants)
                    .FirstOrDefaultAsync(t => t.TransportUnitId == id);
                if (unit == null)
                {
                    TempData["ErrorMessage"] = "Transport unit not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new TransportUnitFormViewModel
                {
                    TransportUnit = unit,
                    SelectedAssistantIds = unit.Assistants?.Select(a => a.AssistantId).ToList() ?? new List<int>()
                };

                ViewData["LorryId"] = new SelectList(_context.Lorries, "LorryId", "LicensePlate", unit.LorryId);
                ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name", unit.DriverId);
                ViewData["ContainerId"] = new SelectList(_context.Containers, "ContainerId", "Type", unit.ContainerId);
                ViewData["AssistantIds"] = new MultiSelectList(_context.Assistants, "AssistantId", "Name", viewModel.SelectedAssistantIds);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the transport unit: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransportUnitFormViewModel model)
        {
            try
            {
                if (id != model.TransportUnit.TransportUnitId)
                {
                    TempData["ErrorMessage"] = "Invalid transport unit ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(model.TransportUnit);
                        await _context.SaveChangesAsync();

                        // Clear old assistant assignments
                        var oldAssistants = _context.Assistants.Where(a => a.TransportUnitId == id);
                        foreach (var assistant in oldAssistants)
                        {
                            assistant.TransportUnitId = null;
                        }

                        // Assign new assistants
                        var newAssistants = _context.Assistants.Where(a => model.SelectedAssistantIds.Contains(a.AssistantId));
                        foreach (var assistant in newAssistants)
                        {
                            assistant.TransportUnitId = model.TransportUnit.TransportUnitId;
                        }

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Transport unit '{model.TransportUnit.Name}' has been updated successfully.";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TransportUnitExists(model.TransportUnit.TransportUnitId))
                        {
                            TempData["ErrorMessage"] = "Transport unit no longer exists.";
                            return RedirectToAction(nameof(Index));
                        }
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }

                // Reload dropdowns
                ViewData["LorryId"] = new SelectList(_context.Lorries, "LorryId", "LicensePlate", model.TransportUnit.LorryId);
                ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name", model.TransportUnit.DriverId);
                ViewData["ContainerId"] = new SelectList(_context.Containers, "ContainerId", "Type", model.TransportUnit.ContainerId);
                ViewData["AssistantIds"] = new MultiSelectList(_context.Assistants, "AssistantId", "Name", model.SelectedAssistantIds);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the transport unit: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Transport unit ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var unit = await _context.TransportUnits
                    .Include(t => t.Lorry)
                    .Include(t => t.Driver)
                    .Include(t => t.Container)
                    .FirstOrDefaultAsync(m => m.TransportUnitId == id);
                if (unit == null)
                {
                    TempData["ErrorMessage"] = "Transport unit not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(unit);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the transport unit: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var unit = await _context.TransportUnits.FindAsync(id);
                if (unit == null)
                {
                    TempData["ErrorMessage"] = "Transport unit not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.TransportUnits.Remove(unit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Transport unit '{unit.Name}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the transport unit: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TransportUnitExists(int id)
        {
            return _context.TransportUnits.Any(e => e.TransportUnitId == id);
        }
    }
}
