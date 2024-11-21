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

    #region Login 
    public IActionResult Login() {
        ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
        ViewBag.RememberMeId = Request.Cookies["RememberMeId"];
        return View();
    } 

    [HttpPost]
    public IActionResult Login(LogIn loginModel, bool RememberMe)
    {
        if(string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
        {
            ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
            ViewBag.RememberMeId = Request.Cookies["RememberMeId"];
            TempData["ErrorMessage"] = "All fields are required";
            return View(loginModel);
        }
        var employee = _session.Query<Employee>().FirstOrDefault(e => e.username == loginModel.Username && e.empId == loginModel.empId && e.password == loginModel.Password);
        if (employee != null)
        {
            _logger.LogInformation("User {Username} authenticated successfully", employee.username);

            HttpContext.Session.SetString("Username", employee.username);
            HttpContext.Session.SetString("Role", employee.Role);
            HttpContext.Session.SetString("EmployeeId", employee.empId.ToString());
            HttpContext.Session.SetString("Gender", employee.Gender);

            // If "Remember Me" is clicked, create cookie 
            if (RememberMe)
            {
                _logger.LogInformation("Setting Remember Me cookies for user {Username}", employee.username);

                CookieOptions options = new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true,
                    Expires = DateTime.Now.AddDays(30),
                    Path = "/" 
                };
                Response.Cookies.Append("RememberMeName", employee.username, options);
                Response.Cookies.Append("RememberMeId", employee.empId.ToString(), options);
            }

            return RedirectToAction("Index", "Home");
        }
        ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
        ViewBag.RememberMeId = Request.Cookies["RememberMeId"];
        TempData["ErrorMessage"] = "Invalid username, id or password";
        return View(loginModel);
    }
    #endregion

    #region log out
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        //// Remove cookies when logging out
        //Response.Cookies.Delete("RememberMeName");
        //Response.Cookies.Delete("RememberMeId");

        return RedirectToAction(nameof(AuthenticationController.Login));
    }

    #endregion

    #region Signup 

    public IActionResult SignUp() => View();

    [HttpPost]
    public async Task<IActionResult> Signup(Employee model, IFormFile ProfilePic)
    {
        try
        {
            // Check if a profile picture is uploaded
            if (ProfilePic != null && ProfilePic.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await ProfilePic.CopyToAsync(memoryStream);
                model.profilePic = memoryStream.ToArray(); 
            }
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all the required fields correctly.";
                return RedirectToAction(nameof(SignUp));
            }
            using var transaction = _session.BeginTransaction();
            _session.Save(model);
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Your account has been successfully registered. Please log in.";
            return RedirectToAction(nameof(Login)); // Redirect to login page
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
            return RedirectToAction(nameof(SignUp));
        }
    }

    #endregion
}
