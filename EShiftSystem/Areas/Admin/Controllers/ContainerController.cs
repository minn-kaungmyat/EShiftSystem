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
            try
            {
                return View(await _context.Containers.ToListAsync());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading containers: " + ex.Message;
                return View(new List<Container>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Container container)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(container);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Container '{container.Type}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(container);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the container: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Container ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var container = await _context.Containers.FindAsync(id);
                if (container == null)
                {
                    TempData["ErrorMessage"] = "Container not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(container);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the container: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Container container)
        {
            try
            {
                if (id != container.ContainerId)
                {
                    TempData["ErrorMessage"] = "Invalid container ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(container);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Container '{container.Type}' has been updated successfully.";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ContainerExists(container.ContainerId))
                        {
                            TempData["ErrorMessage"] = "Container no longer exists.";
                            return RedirectToAction(nameof(Index));
                        }
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(container);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the container: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Container ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var container = await _context.Containers
                    .FirstOrDefaultAsync(m => m.ContainerId == id);
                if (container == null)
                {
                    TempData["ErrorMessage"] = "Container not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(container);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the container: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var container = await _context.Containers.FindAsync(id);
                if (container == null)
                {
                    TempData["ErrorMessage"] = "Container not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Containers.Remove(container);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Container '{container.Type}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the container: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ContainerExists(int id)
        {
            return _context.Containers.Any(e => e.ContainerId == id);
        }
    }
}
