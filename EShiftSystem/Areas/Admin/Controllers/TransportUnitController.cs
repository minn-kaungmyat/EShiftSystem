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
                
                // Get available lorries (not assigned)
                var availableLorries = _context.Lorries
                    .Where(l => !l.IsAssigned)
                    .ToList();
               
                ViewData["LorryId"] = new SelectList(availableLorries, "LorryId", "LicensePlate", 0);

                // Get available drivers (not assigned)
                var availableDrivers = _context.Drivers
                    .Where(d => !d.IsAssigned)
                    .ToList();
                
                ViewData["DriverId"] = new SelectList(availableDrivers, "DriverId", "Name", 0);

                // Get available containers (not assigned)
                var availableContainers = _context.Containers
                    .Where(c => !c.IsAssigned)
                    .ToList();
               
                ViewData["ContainerId"] = new SelectList(availableContainers, "ContainerId", "Type", 0);
                
                // Get available assistants (not assigned)
                var availableAssistants = _context.Assistants
                    .Where(a => !a.IsAssigned)
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
                // Ensure empty assistant selection doesn't cause validation error
                if (model.SelectedAssistantIds == null)
                {
                    model.SelectedAssistantIds = new List<int>();
                }
                
                // Additional server-side validation for assistant limits
                if (model.SelectedAssistantIds != null && model.SelectedAssistantIds.Count > 4)
                {
                    ModelState.AddModelError("SelectedAssistantIds", "A transport unit can have a maximum of 4 assistants.");
                }

                if (ModelState.IsValid)
                {
                    _context.TransportUnits.Add(model.TransportUnit);
                    await _context.SaveChangesAsync();

                    // Mark lorry as assigned
                    var lorry = await _context.Lorries.FindAsync(model.TransportUnit.LorryId);
                    if (lorry != null)
                    {
                        lorry.IsAssigned = true;
                    }

                    // Mark driver as assigned
                    var driver = await _context.Drivers.FindAsync(model.TransportUnit.DriverId);
                    if (driver != null)
                    {
                        driver.IsAssigned = true;
                    }

                    // Mark container as assigned
                    var container = await _context.Containers.FindAsync(model.TransportUnit.ContainerId);
                    if (container != null)
                    {
                        container.IsAssigned = true;
                    }

                    // Mark assistants as assigned (only if assistants are selected)
                    if (model.SelectedAssistantIds != null && model.SelectedAssistantIds.Any())
                    {
                        var selectedAssistants = _context.Assistants
                            .Where(a => model.SelectedAssistantIds.Contains(a.AssistantId));

                        foreach (var assistant in selectedAssistants)
                        {
                            assistant.TransportUnitId = model.TransportUnit.TransportUnitId;
                            assistant.IsAssigned = true;
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Transport unit '{model.TransportUnit.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // Reload dropdowns on error
                var availableLorries = _context.Lorries
                    .Where(l => !l.IsAssigned)
                    .ToList();
                ViewData["LorryId"] = new SelectList(availableLorries, "LorryId", "LicensePlate");

                var availableDrivers = _context.Drivers
                    .Where(d => !d.IsAssigned)
                    .ToList();
                ViewData["DriverId"] = new SelectList(availableDrivers, "DriverId", "Name");

                var availableContainers = _context.Containers
                    .Where(c => !c.IsAssigned)
                    .ToList();
                ViewData["ContainerId"] = new SelectList(availableContainers, "ContainerId", "Type");

                var availableAssistants = _context.Assistants
                    .Where(a => !a.IsAssigned)
                    .ToList();
                ViewData["AssistantIds"] = new MultiSelectList(availableAssistants, "AssistantId", "Name", model.SelectedAssistantIds);

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

                // For edit: show available resources plus the currently assigned ones
                var availableLorries = _context.Lorries
                    .Where(l => !l.IsAssigned || l.LorryId == unit.LorryId)
                    .ToList();
                ViewData["LorryId"] = new SelectList(availableLorries, "LorryId", "LicensePlate", unit.LorryId);
                
                var availableDrivers = _context.Drivers
                    .Where(d => !d.IsAssigned || d.DriverId == unit.DriverId)
                    .ToList();
                ViewData["DriverId"] = new SelectList(availableDrivers, "DriverId", "Name", unit.DriverId);
                
                var availableContainers = _context.Containers
                    .Where(c => !c.IsAssigned || c.ContainerId == unit.ContainerId)
                    .ToList();
                ViewData["ContainerId"] = new SelectList(availableContainers, "ContainerId", "Type", unit.ContainerId);
                
                var availableAssistants = _context.Assistants
                    .Where(a => !a.IsAssigned || a.TransportUnitId == id)
                    .ToList();
                ViewData["AssistantIds"] = new MultiSelectList(availableAssistants, "AssistantId", "Name", viewModel.SelectedAssistantIds);

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

                // Ensure empty assistant selection doesn't cause validation error
                if (model.SelectedAssistantIds == null)
                {
                    model.SelectedAssistantIds = new List<int>();
                }

                // Additional server-side validation for assistant limits
                if (model.SelectedAssistantIds != null && model.SelectedAssistantIds.Count > 4)
                {
                    ModelState.AddModelError("SelectedAssistantIds", "A transport unit can have a maximum of 4 assistants.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Get the current transport unit to check for changes
                        var currentUnit = await _context.TransportUnits
                            .Include(tu => tu.Assistants)
                            .FirstOrDefaultAsync(tu => tu.TransportUnitId == id);

                        if (currentUnit == null)
                        {
                            TempData["ErrorMessage"] = "Transport unit no longer exists.";
                            return RedirectToAction(nameof(Index));
                        }

                        // Handle lorry assignment changes
                        if (currentUnit.LorryId != model.TransportUnit.LorryId)
                        {
                            // Unassign old lorry
                            var oldLorry = await _context.Lorries.FindAsync(currentUnit.LorryId);
                            if (oldLorry != null)
                            {
                                oldLorry.IsAssigned = false;
                            }

                            // Assign new lorry
                            var newLorry = await _context.Lorries.FindAsync(model.TransportUnit.LorryId);
                            if (newLorry != null)
                            {
                                newLorry.IsAssigned = true;
                            }
                        }

                        // Handle driver assignment changes
                        if (currentUnit.DriverId != model.TransportUnit.DriverId)
                        {
                            // Unassign old driver
                            var oldDriver = await _context.Drivers.FindAsync(currentUnit.DriverId);
                            if (oldDriver != null)
                            {
                                oldDriver.IsAssigned = false;
                            }

                            // Assign new driver
                            var newDriver = await _context.Drivers.FindAsync(model.TransportUnit.DriverId);
                            if (newDriver != null)
                            {
                                newDriver.IsAssigned = true;
                            }
                        }

                        // Handle container assignment changes
                        if (currentUnit.ContainerId != model.TransportUnit.ContainerId)
                        {
                            // Unassign old container
                            var oldContainer = await _context.Containers.FindAsync(currentUnit.ContainerId);
                            if (oldContainer != null)
                            {
                                oldContainer.IsAssigned = false;
                            }

                            // Assign new container
                            var newContainer = await _context.Containers.FindAsync(model.TransportUnit.ContainerId);
                            if (newContainer != null)
                            {
                                newContainer.IsAssigned = true;
                            }
                        }

                        // Update the tracked entity properties instead of using _context.Update()
                        currentUnit.Name = model.TransportUnit.Name;
                        currentUnit.LorryId = model.TransportUnit.LorryId;
                        currentUnit.DriverId = model.TransportUnit.DriverId;
                        currentUnit.ContainerId = model.TransportUnit.ContainerId;
                        currentUnit.Status = model.TransportUnit.Status;
                        
                        await _context.SaveChangesAsync();

                        // Clear old assistant assignments
                        var oldAssistants = _context.Assistants.Where(a => a.TransportUnitId == id);
                        foreach (var assistant in oldAssistants)
                        {
                            assistant.TransportUnitId = null;
                            assistant.IsAssigned = false;
                        }

                        // Assign new assistants (only if assistants are selected)
                        if (model.SelectedAssistantIds != null && model.SelectedAssistantIds.Any())
                        {
                            var newAssistants = _context.Assistants.Where(a => model.SelectedAssistantIds.Contains(a.AssistantId));
                            foreach (var assistant in newAssistants)
                            {
                                assistant.TransportUnitId = model.TransportUnit.TransportUnitId;
                                assistant.IsAssigned = true;
                            }
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

                // Reload dropdowns for edit: show available resources plus currently assigned ones
                var availableLorries = _context.Lorries
                    .Where(l => !l.IsAssigned || l.LorryId == model.TransportUnit.LorryId)
                    .ToList();
                ViewData["LorryId"] = new SelectList(availableLorries, "LorryId", "LicensePlate", model.TransportUnit.LorryId);
                
                var availableDrivers = _context.Drivers
                    .Where(d => !d.IsAssigned || d.DriverId == model.TransportUnit.DriverId)
                    .ToList();
                ViewData["DriverId"] = new SelectList(availableDrivers, "DriverId", "Name", model.TransportUnit.DriverId);
                
                var availableContainers = _context.Containers
                    .Where(c => !c.IsAssigned || c.ContainerId == model.TransportUnit.ContainerId)
                    .ToList();
                ViewData["ContainerId"] = new SelectList(availableContainers, "ContainerId", "Type", model.TransportUnit.ContainerId);
                
                // For edit, show available assistants plus currently assigned ones
                var availableAssistants = _context.Assistants
                    .Where(a => !a.IsAssigned || a.TransportUnitId == id)
                    .ToList();
                ViewData["AssistantIds"] = new MultiSelectList(availableAssistants, "AssistantId", "Name", model.SelectedAssistantIds);

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
                var unit = await _context.TransportUnits
                    .Include(tu => tu.Assistants)
                    .FirstOrDefaultAsync(tu => tu.TransportUnitId == id);
                    
                if (unit == null)
                {
                    TempData["ErrorMessage"] = "Transport unit not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Unassign all resources before deleting
                
                // Unassign lorry
                var lorry = await _context.Lorries.FindAsync(unit.LorryId);
                if (lorry != null)
                {
                    lorry.IsAssigned = false;
                }

                // Unassign driver
                var driver = await _context.Drivers.FindAsync(unit.DriverId);
                if (driver != null)
                {
                    driver.IsAssigned = false;
                }

                // Unassign container
                var container = await _context.Containers.FindAsync(unit.ContainerId);
                if (container != null)
                {
                    container.IsAssigned = false;
                }

                // Unassign all assistants
                foreach (var assistant in unit.Assistants)
                {
                    assistant.TransportUnitId = null;
                    assistant.IsAssigned = false;
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
