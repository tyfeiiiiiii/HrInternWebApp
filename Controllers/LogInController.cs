using HrInternWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace HrInternWebApp.Controllers
{
    public class LogInController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //used to submit data, Login is triggered when user submit login form
        public IActionResult Login(LogIn model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == "admin" && model.Password == "123")//hardcode the info
                {
                    return RedirectToAction("Index", "Home");
                }

                //If credential are incorrect, add error to ModelState
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            return View(model);
        }
    }
}
