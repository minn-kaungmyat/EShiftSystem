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
            var assistants = await _context.Assistants
                .Include(a => a.TransportUnit)
                .ToListAsync();
            return View(assistants);
        }

        public IActionResult Create()
        {
            ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assistant assistant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(assistant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            //ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name", assistant.TransportUnitId);
            return View(assistant);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var assistant = await _context.Assistants.FindAsync(id);
            if (assistant == null) return NotFound();

            //ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name", assistant.TransportUnitId);
            return View(assistant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Assistant assistant)
        {
            if (id != assistant.AssistantId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assistant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssistantExists(assistant.AssistantId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            //ViewData["TransportUnitId"] = new SelectList(_context.TransportUnits, "TransportUnitId", "Name", assistant.TransportUnitId);
            return View(assistant);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var assistant = await _context.Assistants
                .Include(a => a.TransportUnit)
                .FirstOrDefaultAsync(m => m.AssistantId == id);
            if (assistant == null) return NotFound();

            return View(assistant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assistant = await _context.Assistants.FindAsync(id);
            if (assistant != null)
            {
                _context.Assistants.Remove(assistant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AssistantExists(int id)
        {
            return _context.Assistants.Any(e => e.AssistantId == id);
        }
    }
}
