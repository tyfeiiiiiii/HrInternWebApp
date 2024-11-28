//using HrInternWebApp.Models.Identity;
//using Microsoft.AspNetCore.Mvc;
//using HrInternWebApp.Entity;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

//public class EmployeeController : Controller
//{
//    private readonly EmployeeService _employeeService;
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    private readonly ILogger<EmployeeController> _logger;

//    public EmployeeController(EmployeeService employeeService, IHttpContextAccessor httpContextAccessor, ILogger<EmployeeController> logger)
//    {
//        _employeeService = employeeService;
//        _httpContextAccessor = httpContextAccessor;
//        _logger = logger;
//    }

//    #region View Employees
//    public async Task<IActionResult> ViewEmp()
//    {
//        var employees = await _employeeService.GetAllEmployeesAsync();
//        return View(employees); // Return the list of ViewEmp models to the view
//    }
//    #endregion

//    #region Search Employee
//    public async Task<IActionResult> SearchEmployee(string empId)
//    {
//        IList<ViewEmp> filteredEmployees;

//        if (string.IsNullOrEmpty(empId))
//        {
//            // Return all employees if no search query
//            filteredEmployees = await _employeeService.GetAllEmployeesAsync();
//        }
//        else
//        {
//            // Filter employees based on the provided empId
//            filteredEmployees = await _employeeService.GetFilteredEmployeesAsync(empId);
//        }

//        return Json(filteredEmployees); // Return filtered list as JSON
//    }
//    #endregion

//    #region Edit Employee
//    // EditEmp GET method
//    public async Task<IActionResult> EditEmp(int empId)
//    {
//        // Fetch the Employee entity
//        var employee = await _employeeService.GetEmployeeByIdAsync(empId);

//        if (employee == null)
//        {
//            TempData["ErrorMessage"] = "Employee not found.";
//            return RedirectToAction(nameof(ViewEmp)); // Return to employee list or other page
//        }

//        // Map the Employee entity to ViewEmp model (used for form)
//        var model = new ViewEmp
//        {
//            empId = employee.empId,
//            username = employee.username,
//            email = employee.email,
//            Role = employee.Role,
//            Department = employee.Department,
//            profilePic = employee.profilePic // map the profilePic if necessary
//        };

//        // Return to the Edit view with the ViewEmp model
//        return View(model);
//    }


//    [HttpPost]
//    [HttpPost]
//    public async Task<IActionResult> EditEmp(ViewEmp model)
//    {
//        if (!ModelState.IsValid)
//        {
//            TempData["ErrorMessage"] = "Please correct the errors in the form.";
//            return View(model); // Return with validation errors if invalid
//        }

//        // Fetch the Employee entity
//        var employee = await _employeeService.GetEmployeeByIdAsync(model.empId);

//        if (employee == null)
//        {
//            TempData["ErrorMessage"] = "Employee not found.";
//            return RedirectToAction(nameof(ViewEmp)); // Return to employee list if not found
//        }

//        // Map the properties from ViewEmp to Employee entity
//        employee.username = model.username; // Update entity properties
//        employee.email = model.email;
//        employee.Role = model.Role;
//        employee.Department = model.Department;

//        // Handle profile picture upload if required
//        if (model.profilePic != null && model.profilePic.Length > 0)
//        {
//            employee.profilePic = model.profilePic;
//        }

//        // Save the updated Employee entity
//        await _employeeService.UpdateEmployeeAsync(employee);

//        TempData["SuccessMessage"] = "Employee updated successfully!";
//        return RedirectToAction(nameof(ViewEmp)); // Redirect to the list view
//    }

//    #endregion

//    #region Delete Employee
//    public async Task<IActionResult> DeleteEmp(int id)
//    {
//        try
//        {
//            await _employeeService.DeleteEmployeeAsync(id);
//            TempData["SuccessMessage"] = "Employee deleted successfully.";
//        }
//        catch (Exception ex)
//        {
//            TempData["ErrorMessage"] = "Failed to delete employee: " + ex.Message;
//        }

//        return RedirectToAction(nameof(ViewEmp));
//    }
//    #endregion
//}
// EmployeeController.cs

using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;

public class EmployeeController : Controller
{
    private readonly EmployeeService _employeeService;

    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    #region View Employees
    public async Task<IActionResult> ViewEmp()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return View(employees); // Return the list of ViewEmp models to the view
    }
    #endregion

    #region Search Employee
    public async Task<IActionResult> SearchEmployee(string empId)
    {
        IList<ViewEmp> filteredEmployees;

        if (string.IsNullOrEmpty(empId))
        {
            // Return all employees if no search query
            filteredEmployees = await _employeeService.GetAllEmployeesAsync();
        }
        else
        {
            // Filter employees based on the provided empId
            filteredEmployees = await _employeeService.GetFilteredEmployeesAsync(empId);
        }

        return Json(filteredEmployees); // Return filtered list as JSON
    }
    #endregion

    #region Edit Employee (GET)
    public async Task<IActionResult> EditEmp(int id)
    {
        var model = await _employeeService.GetEmployeeByIdAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction("ViewEmp"); // Redirect to employee list or other page
        }

        return View(model); // Pass ViewEmp model to the view
    }
    #endregion

    #region Edit Employee (POST)
    [HttpPost]
    public async Task<IActionResult> EditEmp(ViewEmp model, IFormFile? profilePic)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(model); // Return the same model if validation fails
        }

        try
        {
            // Fetch the existing employee to preserve the old profilePic if no new one is uploaded
            var existingEmployee = await _employeeService.GetEmployeeByIdAsync(model.empId);
            if (existingEmployee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction("ViewEmp");
            }

            // If a new profile picture is uploaded, update it
            if (profilePic != null && profilePic.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await profilePic.CopyToAsync(memoryStream);
                    model.profilePic = memoryStream.ToArray(); // Set the new profile picture
                }
            }
            else
            {
                // If no new picture is uploaded, keep the existing one
                model.profilePic = existingEmployee.profilePic;
            }

            // Update the employee's information (preserve old profilePic if no new one is uploaded)
            await _employeeService.UpdateEmployeeAsync(model);

            TempData["SuccessMessage"] = "Employee updated successfully!";
            return RedirectToAction("ViewEmp"); // Redirect back to the employee list or profile page
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating employee: {ex.Message}";
            return View(model); // Return the same model if there's an error
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
