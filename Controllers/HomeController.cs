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
                return RedirectToAction("Login", "LogIn");
            }


            ViewBag.Username = Username;
            return View();
        }
    }
}
