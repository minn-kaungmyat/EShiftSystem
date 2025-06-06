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
            var transportUnits = _context.TransportUnits
                .Include(t => t.Lorry)
                .Include(t => t.Driver)
                .Include(t => t.Container)
                .Include(t => t.Assistants);
            return View(await transportUnits.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransportUnitFormViewModel model)
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
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns on error
            ViewData["LorryId"] = new SelectList(_context.Lorries, "LorryId", "LicensePlate");
            ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name");
            ViewData["ContainerId"] = new SelectList(_context.Containers, "ContainerId", "Type");
            ViewData["AssistantIds"] = new MultiSelectList(_context.Assistants, "AssistantId", "Name");

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.TransportUnits
                .Include(t => t.Assistants)
                .FirstOrDefaultAsync(t => t.TransportUnitId == id);
            if (unit == null) return NotFound();

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransportUnitFormViewModel model)
        {
            if (id != model.TransportUnit.TransportUnitId) return NotFound();

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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransportUnitExists(model.TransportUnit.TransportUnitId)) return NotFound();
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


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.TransportUnits
                .Include(t => t.Lorry)
                .Include(t => t.Driver)
                .Include(t => t.Container)
                .FirstOrDefaultAsync(m => m.TransportUnitId == id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _context.TransportUnits.FindAsync(id);
            if (unit != null)
            {
                _context.TransportUnits.Remove(unit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TransportUnitExists(int id)
        {
            return _context.TransportUnits.Any(e => e.TransportUnitId == id);
        }
    }
}
