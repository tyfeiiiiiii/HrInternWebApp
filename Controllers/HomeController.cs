using HrInternWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace HrInternWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
