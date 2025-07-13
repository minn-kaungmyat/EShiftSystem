using System.Diagnostics;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace EShiftSystem.Controllers
{
    // main controller handling home page and user role-based redirections
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // initializes home controller with logger dependency
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // handles home page requests and redirects users based on their roles
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (User.IsInRole("Customer"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Customer" });
                }
                else
                {
                    return View();
                }
            }
            return View();
        }

        // displays privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }
       
        // redirects users to their respective dashboard based on role
        public IActionResult Dashboard()
        {
            if (User.IsInRole("Admin"))
            {
                // Redirect to Admin area
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (User.IsInRole("Customer"))
            {
                // Redirect to Customer area
                return RedirectToAction("Index", "Dashboard", new { area = "Customer" });
            }
            else
            {
                return View();
            }
        }

        // handles application errors and returns error view with trace information
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
