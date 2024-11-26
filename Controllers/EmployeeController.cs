using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

public class EmployeeController : Controller
{
    private readonly EmployeeService _employeeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    #region Constructor
    public EmployeeController(EmployeeService employeeService, IHttpContextAccessor httpContextAccessor)
    {
        _employeeService = employeeService;
        _httpContextAccessor = httpContextAccessor;
    }
    #endregion

    #region View Employees
    //public async Task<IActionResult> ViewEmp(string empId)
    //{
    //    if (!string.IsNullOrWhiteSpace(empId))
    //    {
    //        //if (int.TryParse(empId, out int parsedEmpId))//convert empId to integer
    //        //{
    //        //    var employee = await _employeeService.GetEmployeeByIdAsync(parsedEmpId);
    //        //    if (employee != null)
    //        //    {
    //        //        return View(new List<Employee> { employee });
    //        //    }
    //        //    TempData["ErrorMessage"] = "Employee not found.";
    //        //}
    //        //else
    //        //{
    //        //    TempData["ErrorMessage"] = "Invalid Employee ID.";
    //        //}
    //    }
    //    // List all employees if no specific employee ID is searched 
    //    var employees = await _employeeService.GetAllEmployeesAsync();
    //    return View(employees);
    //}
    public async Task<IActionResult> ViewEmp(string empId)
    {
        if (!string.IsNullOrWhiteSpace(empId))
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var filteredEmployees = employees.Where(e => e.empId.ToString().Contains(empId)).ToList();
            return Json(filteredEmployees); // Return filtered employees as JSON
        }

        // If no empId is provided, return all employees
        var allEmployees = await _employeeService.GetAllEmployeesAsync();
        return View(allEmployees);
    }

    #endregion

    #region Edit Employee 
    public async Task<IActionResult> EditEmp(int id)//for get (load initial form with data for editing)
    {
        // Check if the user is an admin
        var currentUserRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
        if (currentUserRole != "Admin")
        {
            return RedirectToAction(nameof(ViewEmp));
        }

        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction(nameof(ViewEmp));
        }

        var model = new EditEmp
        {
            empId = employee.empId,
            username = employee.username,
            Role = employee.Role,
            Department = employee.Department,
            email = employee.email,
            profilePic = employee.profilePic
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditEmp(EditEmp model, IFormFile ProfilePic)
    {
        var currentUserRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
        if (currentUserRole != "Admin")
        {
            TempData["ErrorMessage"] = "You do not have permission to edit employee information.";
            return RedirectToAction(nameof(ViewEmp));
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(model); 
        }

        var employee = await _employeeService.GetEmployeeByIdAsync(model.empId);
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction(nameof(ViewEmp));
        }

        employee.username = model.username;
        employee.email = model.email;
        employee.Role = model.Role; 
        employee.Department = model.Department;

        if (ProfilePic != null && ProfilePic.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await ProfilePic.CopyToAsync(memoryStream);
            employee.profilePic = memoryStream.ToArray();
        }

        try
        {
            await _employeeService.UpdateEmployeeAsync(employee);
            TempData["SuccessMessage"] = "Employee information updated successfully.";
            return RedirectToAction(nameof(ViewEmp));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while updating the employee: " + ex.Message);
            return View(model); 
        }
    }
    #endregion


    #region Delete Employee
    public async Task<IActionResult> DeleteEmp(int id)
    {
        try
        {
            await _employeeService.DeleteEmployeeAsync(id);
            TempData["SuccessMessage"] = "Employee deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Failed to delete employee: " + ex.Message;
        }

        return RedirectToAction(nameof(ViewEmp));
    }

    #endregion
}

