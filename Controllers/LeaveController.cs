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

    // View all leave applications based on the user's role
    public IActionResult ViewLeave()
    {
        string role = HttpContext.Session.GetString("Role");
        string employeeIdString = HttpContext.Session.GetString("EmployeeId");

        IList<ViewLeave> leaves;

        if (role == "Admin")
        {
            // Admin can see all leave applications
            leaves = leaveServices.GetAllLeaves();
        }
        else if (int.TryParse(employeeIdString, out int employeeId))
        {
            // Normal users can only see their own leave applications
            leaves = leaveServices.GetLeavesByEmployee(employeeId);
        }
        else
        {
            // Handle case where EmployeeId is not found or invalid
            TempData["ErrorMessage"] = "Unable to find Employee ID.";
            return RedirectToAction("Login", "Authentication");
        }

        return View("ViewLeave", leaves);  // Return filtered leave data
    }


    // GET: Show apply leave form
    public IActionResult ApplyLeave()
    {
        return View();  // Assuming you have ApplyLeave.cshtml to render the form
    }

    // POST: Apply for leave
    [HttpPost]
    public IActionResult ApplyLeave(ApplyLeave leave)
    {

        var employeeIdString = HttpContext.Session.GetString("EmployeeId");

        if (ModelState.IsValid)
        {

            if (string.IsNullOrEmpty(employeeIdString) || !int.TryParse(employeeIdString, out int employeeId))//convert string to integer
            {
                ModelState.AddModelError("", "Invalid Employee ID");
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
