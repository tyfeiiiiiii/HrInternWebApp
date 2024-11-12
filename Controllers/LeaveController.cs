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
    public async Task<IActionResult> ViewLeave()
    {
        string role = HttpContext.Session.GetString("Role");
        string employeeIdString = HttpContext.Session.GetString("EmployeeId");

        IList<ViewLeave> leaves;

        if (role == "Admin")
        {
            // Admin can see all leave applications
            leaves = await leaveServices.GetAllLeavesAsync(); // Synchronous call
        }
        else if (int.TryParse(employeeIdString, out int employeeId))
        {
            // Normal users can only see their own leave applications
            leaves = await leaveServices.GetLeavesByEmployeeAsync(employeeId); // Synchronous call
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to find Employee ID.";
            return RedirectToAction(nameof(AuthenticationController.Login));
        }

        return View("ViewLeave", leaves);  // Return filtered leave data
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
