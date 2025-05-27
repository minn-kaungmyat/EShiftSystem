using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShiftSystem.Areas.Customer.Controllers
{
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Views/Customer/Dashboard/Index.cshtml
        }
    }
}
