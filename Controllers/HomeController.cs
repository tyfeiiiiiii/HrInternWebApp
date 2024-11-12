using HrInternWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace HrInternWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var Username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(Username))
            { 
                return RedirectToAction(nameof(AuthenticationController.Login));
            }
            ViewBag.Username = Username;
            return View();
        }
    }
}
