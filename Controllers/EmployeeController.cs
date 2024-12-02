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
        return View(employees); // Return the list of ViewEmp models to the view
    }
    #endregion

    #region Search Employee
    //public async Task<IActionResult> SearchEmployee(string empId)
    //{
    //    IList<ViewEmp> filteredEmployees;

    //    if (string.IsNullOrEmpty(empId))
    //    {
    //        // Return all employees if no search query
    //        filteredEmployees = await _employeeService.GetAllEmployeesAsync();
    //    }
    //    else
    //    {
    //        // Filter employees based on the provided empId
    //        filteredEmployees = await _employeeService.GetFilteredEmployeesAsync(empId);
    //    }

    //    return Json(filteredEmployees); // Return filtered list as JSON
    //}
    public async Task<IActionResult> SearchEmployee(string empId)
    {
        IList<ViewEmp> filteredEmployees;

        // If no empId is provided, use cached data
        if (string.IsNullOrEmpty(empId))
        {
            filteredEmployees = await _employeeService.GetAllEmployeesAsync();
        }
        else
        {
            // If an empId is provided, search the cache or database
            string cacheKey = $"Employee_{empId}";
            if (!_cache.TryGetValue(cacheKey, out filteredEmployees))
            {
                // Cache miss: fetch from the service
                _logger.LogInformation($"Cache miss for empId: {empId}");

                filteredEmployees = await _employeeService.GetFilteredEmployeesAsync(empId);

                // Cache the result for future use
                _cache.Set(cacheKey, filteredEmployees, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) // Cache for 15 minutes
                });

                _logger.LogInformation($"Data for empId: {empId} fetched from database and cached.");
            }
            else
            {
                // Cache hit
                _logger.LogInformation($"Cache hit for empId: {empId}");
            }
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
