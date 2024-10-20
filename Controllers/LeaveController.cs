using Microsoft.AspNetCore.Mvc;

namespace HrInternWebApp.Controllers
{
    public class LeaveController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
