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
    //public async Task<IActionResult> ViewEmp()
    //{
    //    var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
    //    var currentUserId = _httpContextAccessor.HttpContext.Session.GetInt32("empId");

    //    var employees = await _employeeService.GetAllEmployeesAsync();

    //    if (role != "Admin" && currentUserId.HasValue)
    //    {
    //        // Filter employees to show only the current user's information for non-admins
    //        employees = employees.Where(e => e.empId == currentUserId.Value).ToList();
    //    }

    //    return View(employees);
    //}
    public async Task<IActionResult> ViewEmp(string empId)
    {
        if (!string.IsNullOrWhiteSpace(empId))
        {
            // Convert empId to int if necessary
            if (int.TryParse(empId, out int parsedEmpId))
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(parsedEmpId);
                if (employee != null)
                {
                    return View(new List<Employee> { employee });
                }
                TempData["ErrorMessage"] = "Employee not found.";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Employee ID.";
            }
        }

        // Fetch all employees if no specific employee ID is searched for
        var employees = await _employeeService.GetAllEmployeesAsync();
        return View(employees);
    }


    #endregion

    #region Edit Employee
    public async Task<IActionResult> EditEmp(int id)
    {
        // Check if the user is an admin
        var currentUserRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
        if (currentUserRole != "Admin")
        {
            TempData["ErrorMessage"] = "You do not have permission to edit employee information.";
            return RedirectToAction(nameof(ViewEmp));
        }

        // Fetch the employee record from the database
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction(nameof(ViewEmp));
        }

        // Populate the view model
        var model = new EditEmp
        {
            empId = employee.empId,
            username = employee.username,
            Role = employee.Role,
            Department = employee.Department,
            email = employee.email,
            profilePic = employee.profilePic
        };

        // Pass the model to the view for editing
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditEmp(EditEmp model, IFormFile ProfilePic)
    {
        // Check if the user is an admin
        var currentUserRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
        if (currentUserRole != "Admin")
        {
            TempData["ErrorMessage"] = "You do not have permission to edit employee information.";
            return RedirectToAction(nameof(ViewEmp));
        }

        // Validate model state
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(model); // Return view with validation errors
        }

        // Fetch the employee record from the database
        var employee = await _employeeService.GetEmployeeByIdAsync(model.empId);
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction(nameof(ViewEmp));
        }

        // Update employee fields
        employee.username = model.username;
        employee.email = model.email;
        employee.Role = model.Role; // Ensure this field is correctly populated
        employee.Department = model.Department;

        // Handle profile picture upload if provided
        if (ProfilePic != null && ProfilePic.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await ProfilePic.CopyToAsync(memoryStream);
            employee.profilePic = memoryStream.ToArray();
        }

        try
        {
            // Save changes to the database
            await _employeeService.UpdateEmployeeAsync(employee);
            TempData["SuccessMessage"] = "Employee information updated successfully.";
            return RedirectToAction(nameof(ViewEmp));
        }
        catch (Exception ex)
        {
            // Log and handle the exception as needed
            ModelState.AddModelError(string.Empty, "An error occurred while updating the employee: " + ex.Message);
            return View(model); // Return to view with model to show error
        }
    }
    #endregion


    #region Delete Employee
    public async Task<IActionResult> DeleteEmp(int id)
    {
        // Ensure only admins can delete employees
        if (_httpContextAccessor.HttpContext.Session.GetString("Role") != "Admin")
        {
            return Unauthorized();
        }

        try
        {
            // Call the service to delete the employee and associated information
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

