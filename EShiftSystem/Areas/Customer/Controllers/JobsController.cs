using EShiftSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShiftSystem.Areas.Customer.Controllers
{
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class JobsController : Controller
    {
        // GET: Customer/Jobs/Create
        public IActionResult Create()
        {
            return View(); // Views/Customer/Jobs/Create.cshtml
        }

        // POST: Customer/Jobs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Job model)
        {
            if (ModelState.IsValid)
            {
                // Save job logic here
                return RedirectToAction("MyJobs");
            }

            return View();
        }

        // GET: Customer/Jobs/MyJobs
        public IActionResult MyJobs()
        {
            // Fetch customer jobs from database
            return View(); // Views/Customer/Jobs/MyJobs.cshtml
        }
    }
}
