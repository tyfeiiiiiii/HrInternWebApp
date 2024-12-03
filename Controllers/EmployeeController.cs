using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

public class EmployeeController : Controller
{
    private readonly EmployeeService _employeeService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(EmployeeService employeeService, IMemoryCache memoryCache, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _cache = memoryCache;
        _logger = logger;
    }

    #region View Employees
    public async Task<IActionResult> ViewEmp()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return View(employees);
    }
    #endregion

    #region Search Employee
    public async Task<IActionResult> SearchEmployee(string empId)
    {
        IList<ViewEmp> filteredEmployees;

        if (string.IsNullOrEmpty(empId))
        {
            filteredEmployees = await _employeeService.GetAllEmployeesAsync();
        }
        else
        {
            filteredEmployees = await _employeeService.GetFilteredEmployeesAsync(empId);
        }

        return Json(filteredEmployees); 
    }
    #endregion

    #region Edit Employee (GET)
    public async Task<IActionResult> EditEmp(int id)
    {
        var model = await _employeeService.GetEmployeeByIdAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction("ViewEmp");
        }

        return View(model);
    }
    #endregion

    #region Edit Employee (POST)
    [HttpPost]
    public async Task<IActionResult> EditEmp(ViewEmp model, IFormFile? profilePic)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(model); 
        }

        try
        {
            var existingEmployee = await _employeeService.GetEmployeeByIdAsync(model.empId);
            if (existingEmployee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction("ViewEmp");
            }

            if (profilePic != null && profilePic.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await profilePic.CopyToAsync(memoryStream);
                    model.profilePic = memoryStream.ToArray(); 
                }
            }
            else
            {
                model.profilePic = existingEmployee.profilePic;
            }

            await _employeeService.UpdateEmployeeAsync(model);

            TempData["SuccessMessage"] = "Employee updated successfully!";
            return RedirectToAction("ViewEmp");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating employee: {ex.Message}";
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
