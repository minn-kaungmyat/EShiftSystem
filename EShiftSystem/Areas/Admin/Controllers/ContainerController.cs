using EShiftSystem.Data;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ContainerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContainerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Containers.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Container container)
        {
            if (ModelState.IsValid)
            {
                _context.Add(container);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(container);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var container = await _context.Containers.FindAsync(id);
            if (container == null) return NotFound();

            return View(container);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Container container)
        {
            if (id != container.ContainerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(container);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContainerExists(container.ContainerId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(container);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var container = await _context.Containers
                .FirstOrDefaultAsync(m => m.ContainerId == id);
            if (container == null) return NotFound();

            return View(container);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var container = await _context.Containers.FindAsync(id);
            if (container != null)
            {
                _context.Containers.Remove(container);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ContainerExists(int id)
        {
            return _context.Containers.Any(e => e.ContainerId == id);
        }
    }
}
