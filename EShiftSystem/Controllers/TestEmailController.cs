using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using EShiftSystem.Services;

namespace EShiftSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TestEmailController : Controller
    {
        private readonly IEmailSender _emailSender;

        public TestEmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

      
           
    }
} 