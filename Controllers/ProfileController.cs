using HrInternWebApp.Data;
using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HrInternWebApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #region Constructor
        public ProfileController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<IActionResult> ViewProfile()
        {
            var empId = _httpContextAccessor.HttpContext.Session.GetString("EmployeeId");

            if (string.IsNullOrEmpty(empId))
            {
                TempData["ErrorMessage"] = "You must be logged in to view your profile.";
                return RedirectToAction("Login", "Authentication");
            }

            if (int.TryParse(empId, out int parsedEmpId))
            {
                var employee = await _context.Employees.FindAsync(parsedEmpId);

                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToAction("Index", "Home");
                }

                return View(employee);
            }

            TempData["ErrorMessage"] = "Invalid Employee ID.";
            return RedirectToAction("Index", "Home");
        }
    }
}
