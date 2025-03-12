using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using HrInternWebApp.Data;
using Microsoft.Extensions.Logging;

public class EmployeeController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(AppDbContext context, ILogger<EmployeeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region View Employees
    public async Task<IActionResult> ViewEmp()
    {
        var employees = await _context.Employees.ToListAsync();
        return View(employees);
    }
    #endregion

    #region Edit Employee
    public async Task<IActionResult> EditEmp(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction("ViewEmp");
        }
        return View(employee);
    }

    [HttpPost]
    public async Task<IActionResult> EditEmp(Employee model, IFormFile? profilePic)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(model);
        }

        var existingEmployee = await _context.Employees.FindAsync(model.EmpId);
        if (existingEmployee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction("ViewEmp");
        }

        if (profilePic != null && profilePic.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await profilePic.CopyToAsync(memoryStream);
            existingEmployee.ProfilePic = memoryStream.ToArray();
        }

        _context.Update(existingEmployee);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Employee updated successfully!";
        return RedirectToAction("ViewEmp");
    }
    #endregion

    #region Delete Employee
    public async Task<IActionResult> DeleteEmp(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Employee deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete employee.";
        }
        return RedirectToAction("ViewEmp");
    }
    #endregion
}
