using System.Diagnostics;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace EShiftSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

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
                    // Default fallback if user has no role
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
            }
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }


        public IActionResult Privacy()
        {
            return View();
        }
       
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
                return RedirectToAction("Index", "Home");
            }
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
