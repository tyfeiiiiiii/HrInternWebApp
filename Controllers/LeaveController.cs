using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    public IActionResult ApplyLeave(ApplyLeave leave)
    {
        var employeeIdString = HttpContext.Session.GetString("EmployeeId");

        if (ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(employeeIdString) || !int.TryParse(employeeIdString, out int employeeId)) // Convert string to integer
            {
                ModelState.AddModelError("", "Invalid Employee ID");
                return View(leave);
            }

            leaveServices.ApplyLeave(leave, employeeId);
            return RedirectToAction("ViewLeave");
        }
        return View(leave);  // Return form with validation errors if model is invalid
    }
    #endregion

    public async Task<IActionResult> ViewLeave()
    {
        string role = HttpContext.Session.GetString("Role");
        string employeeIdString = HttpContext.Session.GetString("EmployeeId");

        IList<EditLeave> leaves;

        if (role == "Admin")
        {
            // Admin can see all leave applications
            leaves = await leaveServices.GetAllLeaves();
        }
        else if (int.TryParse(employeeIdString, out int employeeId))
        {
            // Normal users can only see their own leave applications
            leaves = await leaveServices.GetLeavesByEmployee(employeeId);
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to find Employee ID.";
            return RedirectToAction("Login", "Authentication");
        }

        return View("ViewLeave", leaves);  // Return filtered leave data
    }



    #region Update Leave Status
    // Update leave status, only for admins
    //[HttpPost]
    //[Authorize(Roles = "Admin")]
    public IActionResult UpdateLeaveStatus(int leaveId, string status)
    {
        var approver = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(approver))
        {
            return RedirectToAction("Login", "Authentication");
        }

        leaveServices.UpdateLeaveStatus(leaveId, status, approver);
        return RedirectToAction("ViewLeave");
    }
    #endregion
}
