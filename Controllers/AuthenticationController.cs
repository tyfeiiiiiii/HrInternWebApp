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
                Response.Cookies.Append("RememberMeId", employee.empId.ToString(), options);
            }

            // Set ViewBag values to retrieve value when loading the form
            ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
            ViewBag.RememberMeId = Request.Cookies["RememberMeId"];

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid username or password.");

        // Set ViewBag values even if login fails
        ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
        ViewBag.RememberMeId = Request.Cookies["RememberMeId"];

        return View(loginModel);
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        //// Remove cookies when logging out
        //Response.Cookies.Delete("RememberMeName");
        //Response.Cookies.Delete("RememberMeId");

        return RedirectToAction(nameof(AuthenticationController.Login));
    }

    #endregion

    #region Signup Actions
    public IActionResult SignUp() => View();

    [HttpPost]
    public async Task<IActionResult> Signup(Employee model, IFormFile ProfilePic)
    {
        if (ProfilePic != null && ProfilePic.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await ProfilePic.CopyToAsync(memoryStream);
            model.profilePic = memoryStream.ToArray(); // Convert to byte array
        }

        // Validate model and perform additional checks as necessary
        if (ModelState.IsValid)
        {
            using var transaction = _session.BeginTransaction();
            _session.Save(model);
            await transaction.CommitAsync();
            return RedirectToAction("Login");
        }

        return View(model); // Re-display the form if there are errors
    }
    #endregion
}
