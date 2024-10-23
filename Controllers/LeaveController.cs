using HrInternWebApp.Services;
using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

public class LeaveController : Controller
{
    private readonly LeaveService leaveServices;

    public LeaveController(LeaveService leaveService)
    {
        leaveServices = leaveService;
    }

    // View all leave applications
    public IActionResult ViewLeave()
    {
        var leaves = leaveServices.GetAllLeaves();
        return View("ViewLeave", leaves);  // Make sure you have a ViewLeave.cshtml file
    }

    // GET: Show apply leave form
    public IActionResult ApplyLeave()
    {
        return View();  // Assuming you have ApplyLeave.cshtml to render the form
    }

    // POST: Apply for leave
    [HttpPost]
    public IActionResult ApplyLeave(Leave leave)
    {
        if (ModelState.IsValid)
        {
            var employeeIdString = HttpContext.Session.GetString("EmployeeId");

            if (string.IsNullOrEmpty(employeeIdString) || !int.TryParse(employeeIdString, out int employeeId))
            {
                ModelState.AddModelError("", "Unable to find EmployeeId.");
                return View(leave);
            }

            leaveServices.ApplyLeave(leave, employeeId);
            return RedirectToAction("ViewLeave");
        }
        return View(leave);  // Return form with validation errors if model is invalid
    }

    // Update leave status, only for admins
    [HttpPost]
    [Authorize(Roles = "Admin")]  // Require Admin role to update leave status
    public IActionResult UpdateLeaveStatus(int leaveId, string status)
    {
        var approver = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(approver))
        {
            return RedirectToAction("Login", "Authentication");  // Redirect to login if not authenticated
        }

        leaveServices.UpdateLeaveStatus(leaveId, status, approver);
        return RedirectToAction("ViewLeave");
    }
}
