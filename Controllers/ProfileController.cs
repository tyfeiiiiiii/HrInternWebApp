using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HrInternWebApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly EmployeeService _employeeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #region Constructor
        public ProfileController(EmployeeService employeeService, IHttpContextAccessor httpContextAccessor)
        {
            _employeeService = employeeService;
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
                var employee = await _employeeService.GetEmployeeByIdAsync(parsedEmpId);

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
