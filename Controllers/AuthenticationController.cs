using Microsoft.AspNetCore.Mvc;
using HrInternWebApp.Models;
using NHibernate;
using ISession = NHibernate.ISession;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

public class AuthenticationController : Controller
{
    private readonly ISession _session;

    public AuthenticationController(ISession session)
    {
        _session = session;
    }

    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(LogIn loginModel)
    {
        var employee = _session.Query<Employee>()
                               .FirstOrDefault(e => e.Username == loginModel.Username && e.Password == loginModel.Password);
        if (employee != null)
        {
            // Set username 
            HttpContext.Session.SetString("Username", employee.Username);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(loginModel);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Authentication");
    }
}
