using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

public class LeaveController : Controller
{
    private readonly LeaveService leaveServices;

    public LeaveController(LeaveService leaveService)
    {
        leaveServices = leaveService;
    }
    #region Apply Leave
    //public IActionResult GetLeaveTypes()
    //{
    //    var gender = HttpContext.Session.GetString("Gender");
    //    var leaveTypes = leaveServices.GetLeaveTypesByGender(gender);
    //    return Json(leaveTypes);
    //}

    public IActionResult ApplyLeave()
    {
        var gender = HttpContext.Session.GetString("Gender");
        ViewData["Gender"] = gender;

        var leaveTypes = leaveServices.GetLeaveTypesByGender(gender);
        ViewData["LeaveTypes"] = leaveTypes;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ApplyLeave(ApplyLeave leave)
    {
        var employeeIdString = HttpContext.Session.GetString("EmployeeId");

        if (ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(employeeIdString) || !int.TryParse(employeeIdString, out int employeeId))
            {
                ModelState.AddModelError("", "Invalid Employee ID");
                return View(leave);
            }
            await leaveServices.ApplyLeaveAsync(leave, employeeId);
            return RedirectToAction(nameof(ViewLeave));
        }
        var gender = HttpContext.Session.GetString("Gender");
        ViewData["Gender"] = gender;

        var leaveTypes = leaveServices.GetLeaveTypesByGender(gender);
        ViewData["LeaveTypes"] = leaveTypes;
        return View(leave);
    }
    #endregion

    #region ViewLeave
    public async Task<IActionResult> ViewLeave(string empId)
    {
        string role = HttpContext.Session.GetString("Role");
        string employeeIdString = HttpContext.Session.GetString("EmployeeId");

        IList<ViewLeave> leaves;

        if (role == "Admin")
        {
            leaves = await leaveServices.GetAllLeavesAsync();
            if (!string.IsNullOrEmpty(empId) && int.TryParse(empId, out int employeeIdFilter))
            {
                leaves = leaves.Where(l => l.empId == employeeIdFilter).ToList();
            }
        }
        else if (int.TryParse(employeeIdString, out int employeeId))
        {
            leaves = await leaveServices.GetLeavesByEmployeeAsync(employeeId);

            if (!leaves.Any())
            {
                TempData["ErrorMessage"] = "No records found for your Employee ID.";
            }
        }
        else
        {
            leaves = new List<ViewLeave>();
        }

        return View(leaves);
    }
    #endregion

    #region SearchLeave By EmpId
    public async Task<IActionResult> SearchLeave(string empId)
    {
        // If no empId is provided, return all leaves
        if (string.IsNullOrEmpty(empId))
        {
            var allLeaves = await leaveServices.GetAllLeavesAsync();
            return Json(allLeaves);
        }

        // Filter the leaves based on the provided empId
        var allFilteredLeaves = await leaveServices.GetAllLeavesAsync();
        var filteredLeaves = allFilteredLeaves.Where(l => l.empId.ToString().Contains(empId)).ToList();

        return Json(filteredLeaves); // Return filtered leave list as JSON
    }


    #endregion

    #region Update Leave Status
    // Update leave status, only for admins
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
