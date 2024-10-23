using Microsoft.AspNetCore.Mvc;
using NHibernate;
using ISession = NHibernate.ISession;
using HrInternWebApp.Models.Identity;

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
            // Set username and role in session
            HttpContext.Session.SetString("Username", employee.Username);
            HttpContext.Session.SetString("Role", employee.Role);  // Store the user role in session
            HttpContext.Session.SetString("EmployeeId", employee.EmpId.ToString());  // Store EmployeeId

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(loginModel);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(AuthenticationController.Login));
    }
}
