using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

public class LeaveController : Controller
{
    private readonly LeaveService leaveServices;

    #region Constructor
    public LeaveController(LeaveService leaveService)
    {
        leaveServices = leaveService;
    }
    #endregion

    #region Apply Leave
    // GET: Show apply leave form
    public IActionResult ApplyLeave()
    {
        return View();  // Assuming you have ApplyLeave.cshtml to render the form
    }

    // POST: Apply for leave
    [HttpPost]
    public async Task<IActionResult> ApplyLeave(ApplyLeave leave)
    {
        var employeeIdString = HttpContext.Session.GetString("EmployeeId");

        if (ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(employeeIdString) || !int.TryParse(employeeIdString, out int employeeId)) // Convert string to integer
            {
                ModelState.AddModelError("", "Invalid Employee ID");
                return View(leave);
            }

            await leaveServices.ApplyLeaveAsync(leave, employeeId);
            return RedirectToAction(nameof(LeaveController.ViewLeave));
        }
        return View(leave);  // Return form with validation errors if model is invalid
    }
    #endregion

    #region View Leave
    public async Task<IActionResult> ViewLeave(string empId)
    {
        string role = HttpContext.Session.GetString("Role");
        string employeeIdString = HttpContext.Session.GetString("EmployeeId");

        IList<ViewLeave> leaves;

        if (role == "Admin")
        {
            leaves = await leaveServices.GetAllLeavesAsync();

            // If search by key in empId
            if (!string.IsNullOrEmpty(empId) && int.TryParse(empId, out int employeeIdFilter))
            {
                leaves = leaves.Where(l => l.empId == employeeIdFilter).ToList();
            }
        }
        else if (int.TryParse(employeeIdString, out int employeeId))
        {
            // Normal users can only see own leave applications
            if (!string.IsNullOrEmpty(empId) && int.TryParse(empId, out int employeeIdFilter) && employeeIdFilter != employeeId)
            {
                // If a normal user search for an employee ID that is not their own, show an error message
                TempData["ErrorMessage"] = "You do not have the privilege to view other users' leave applications.";
                return View("ViewLeave", new List<ViewLeave>()); 
            }

            // Fetch leaves for the logged-in user
            leaves = await leaveServices.GetLeavesByEmployeeAsync(employeeId);

            if (!leaves.Any())
            {
                TempData["ErrorMessage"] = "No records found for your Employee ID.";
                return View("ViewLeave", new List<ViewLeave>()); // Return an empty list
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to find Employee ID.";
            return RedirectToAction(nameof(AuthenticationController.Login));
        }

        return View("ViewLeave", leaves);  
    }

    #endregion

    #region Update Leave Status
    // Update leave status, only for admins
    //[HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateLeaveStatusAsync(int leaveId, string status)
    {
        var approver = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(approver))
        {
            return RedirectToAction("Login", "Authentication");
        }

        await leaveServices.UpdateLeaveStatusAsync(leaveId, status, approver);
        return RedirectToAction(nameof(LeaveController.ViewLeave));
    }
    #endregion
}
