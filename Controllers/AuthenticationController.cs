using Microsoft.AspNetCore.Mvc;
using NHibernate;
using ISession = NHibernate.ISession;
using HrInternWebApp.Entity;

public class AuthenticationController : Controller
{
    #region Fields
    private readonly ISession _session;
    #endregion

    #region Constructor
    public AuthenticationController(ISession session)
    {
        _session = session;
    }
    #endregion

    #region Login Actions
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(LogIn loginModel)
    {
        var employee = _session.Query<Employee>()
                               .FirstOrDefault(e => e.username == loginModel.Username && e.password == loginModel.Password);
        if (employee != null)
        {
            // Set username and role in session
            HttpContext.Session.SetString("Username", employee.username);
            HttpContext.Session.SetString("Role", employee.Role);
            HttpContext.Session.SetString("EmployeeId", employee.empId.ToString());

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid username or password.");
        return View(loginModel);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(AuthenticationController.Login));
    }
    #endregion

    #region Signup Actions
    public IActionResult SignUp() => View();

    [HttpPost]
    public IActionResult Signup(Employee employee)
    {
        if (ModelState.IsValid)
        {
            using (ITransaction transaction = _session.BeginTransaction())
            {
                _session.Save(employee);
                transaction.Commit();
            }
            return RedirectToAction(nameof(AuthenticationController.Login));
        }
        return View(employee);
    }
    #endregion
}
