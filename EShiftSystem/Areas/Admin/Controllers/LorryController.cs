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
            return View(await _context.Lorries.ToListAsync());
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
            if (ModelState.IsValid)
            {
                _context.Add(lorry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lorry);
        }

        // GET: Lorry/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lorry = await _context.Lorries.FindAsync(id);
            if (lorry == null) return NotFound();

            return View(lorry);
        }

        // POST: Lorry/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lorry lorry)
        {
            if (id != lorry.LorryId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lorry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LorryExists(lorry.LorryId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(lorry);
        }

        // GET: Lorry/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lorry = await _context.Lorries
                .FirstOrDefaultAsync(m => m.LorryId == id);
            if (lorry == null) return NotFound();

            return View(lorry);
        }

        // POST: Lorry/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lorry = await _context.Lorries.FindAsync(id);
            if (lorry != null)
            {
                _context.Lorries.Remove(lorry);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LorryExists(int id)
        {
            return _context.Lorries.Any(e => e.LorryId == id);
        }
    }
}
