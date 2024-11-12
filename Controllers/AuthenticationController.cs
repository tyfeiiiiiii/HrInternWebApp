using Microsoft.AspNetCore.Mvc;
using NHibernate;
using ISession = NHibernate.ISession;
using HrInternWebApp.Entity;
using HrInternWebApp.Controllers;
using HrInternWebApp.Models.Identity;

public class AuthenticationController : Controller
{
    #region Fields
    private readonly ISession _session;
    #endregion

    #region Constructor
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ISession session, ILogger<AuthenticationController> logger)
    {
        _session = session;
        _logger = logger;
    }

    #endregion

    #region Login Actions
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(LogIn loginModel, bool RememberMe)
    {
        _logger.LogInformation("Login attempt for username: {Username}", loginModel.Username);

        var employee = _session.Query<Employee>()
                               .FirstOrDefault(e => e.username == loginModel.Username && e.password == loginModel.Password);
        if (employee != null)
        {
            _logger.LogInformation("User {Username} authenticated successfully", employee.username);

            // Set username and role in session
            HttpContext.Session.SetString("Username", employee.username);
            HttpContext.Session.SetString("Role", employee.Role);
            HttpContext.Session.SetString("EmployeeId", employee.empId.ToString());

            // If "Remember Me" is clicked, create cookie 
            if (RememberMe)
            {
                _logger.LogInformation("Setting Remember Me cookies for user {Username}", employee.username);

                CookieOptions options = new CookieOptions
                {
                    HttpOnly = true, // Prevent client-side scripts from accessing the cookie
                    Secure = true,
                    Expires = DateTime.Now.AddDays(30),
                    Path = "/"  // Cookie available within entire application
                };
                Response.Cookies.Append("RememberMeName", employee.username, options);
                Response.Cookies.Append("RememberMePass", employee.password, options);
            }

            // Set ViewBag values to retrieve them when loading the form
            ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
            ViewBag.RememberMePassword = Request.Cookies["RememberMePass"];

            return RedirectToAction("Index", "Home");
        }

        _logger.LogWarning("Invalid login attempt for username: {Username}", loginModel.Username);
        ModelState.AddModelError("", "Invalid username or password.");

        // Set ViewBag values even if login fails
        ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
        ViewBag.RememberMePassword = Request.Cookies["RememberMePass"];

        return View(loginModel);
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        _logger.LogInformation("Session cleared");

        //// Remove cookies when logging out
        //Response.Cookies.Delete("RememberMeName");

        //Response.Cookies.Delete("RememberMePass");

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
