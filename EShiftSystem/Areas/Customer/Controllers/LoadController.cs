using EShiftSystem.Data;
using EShiftSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Areas.Customer.Controllers
{
    public class LoadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoadController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
       

    }
}
