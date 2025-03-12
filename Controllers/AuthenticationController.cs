using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using HrInternWebApp.Data;
using Microsoft.Extensions.Logging;

public class AuthenticationController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(AppDbContext context, ILogger<AuthenticationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Login()
    {
        ViewBag.RememberMeName = Request.Cookies["RememberMeName"];
        ViewBag.RememberMeId = Request.Cookies["RememberMeId"];
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LogIn loginModel, bool rememberMe)
    {
        if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
        {
            TempData["ErrorMessage"] = "All fields are required";
            return View(loginModel);
        }

        var employee = await _context.Employees.FirstOrDefaultAsync(e =>
            e.Username == loginModel.Username && e.EmpId == loginModel.EmpId && e.Password == loginModel.Password);

        if (employee != null)
        {
            _logger.LogInformation("User {Username} authenticated successfully", employee.Username);
            HttpContext.Session.SetString("Username", employee.Username);
            HttpContext.Session.SetString("Role", employee.Role);
            HttpContext.Session.SetString("EmployeeId", employee.EmpId.ToString());
            HttpContext.Session.SetString("Gender", employee.Gender);

            if (rememberMe)
            {
                var options = new CookieOptions { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(30), Path = "/" };
                Response.Cookies.Append("RememberMeName", employee.Username, options);
                Response.Cookies.Append("RememberMeId", employee.EmpId.ToString(), options);
            }
            return RedirectToAction("Index", "Home");
        }

        TempData["ErrorMessage"] = "Invalid username, id, or password";
        return View(loginModel);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult SignUp() => View();

    [HttpPost]
    public async Task<IActionResult> SignUp(Employee model, IFormFile profilePic)
    {
        try
        {
            if (profilePic != null && profilePic.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await profilePic.CopyToAsync(memoryStream);
                model.ProfilePic = memoryStream.ToArray();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                return RedirectToAction(nameof(SignUp));
            }

            _context.Employees.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account successfully registered. Please log in.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred. Please try again later.";
            return RedirectToAction(nameof(SignUp));
        }
    }
}
