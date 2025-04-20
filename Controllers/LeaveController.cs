using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using HrInternWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LeaveController : Controller
{
    private readonly LeaveService _leaveService;
    private readonly LeaveBalanceService _leaveBalanceService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(
        LeaveService leaveService,
        LeaveBalanceService leaveBalanceService,
        IMemoryCache cache,
        ILogger<LeaveController> logger)
    {
        _leaveService = leaveService;
        _leaveBalanceService = leaveBalanceService;
        _cache = cache;
        _logger = logger;
    }

    #region Apply Leave

    public IActionResult ApplyLeave()
    {
        var gender = HttpContext.Session.GetString("Gender");
        ViewData["Gender"] = gender;

        string cacheKey = $"LeaveTypes_{gender}";
        if (!_cache.TryGetValue(cacheKey, out List<string> leaveTypes))
        {
            leaveTypes = _leaveService.GetLeaveTypesByGender(gender);

            _cache.Set(cacheKey, leaveTypes, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        }
        ViewData["LeaveTypes"] = leaveTypes;

        // Fetch leave balance for the logged-in employee
        var employeeIdString = HttpContext.Session.GetString("EmployeeId");
        if (int.TryParse(employeeIdString, out int employeeId))
        {
            var leaveBalance = _leaveBalanceService.GetLeaveBalanceAsync(employeeId).Result;
            ViewData["LeaveBalance"] = leaveBalance;
        }

        return View();
    }

    //[HttpPost]
    //public async Task<IActionResult> ApplyLeave(ApplyLeave leave)
    //{
    //    var employeeIdString = HttpContext.Session.GetString("EmployeeId");

    //    if (ModelState.IsValid)
    //    {
    //        if (string.IsNullOrEmpty(employeeIdString) || !int.TryParse(employeeIdString, out int employeeId))
    //        {
    //            ModelState.AddModelError("", "Invalid Employee ID");
    //            return View(leave);
    //        }

    //        // Calculate leave days
    //        var leaveDays = (leave.endDate.Value - leave.startDate.Value).Days + 1;

    //        // Apply leave
    //        await _leaveService.ApplyLeaveAsync(leave, employeeId);

    //        // Update leave balance
    //        await _leaveBalanceService.UpdateLeaveBalanceAsync(employeeId, leave.leaveType, leaveDays);

    //        _logger.LogInformation("Leave application submitted for Employee ID: {EmployeeId}.", employeeId);
    //        return RedirectToAction(nameof(ViewLeave));
    //    }

    //    var gender = HttpContext.Session.GetString("Gender");
    //    ViewData["Gender"] = gender;

    //    var leaveTypes = _leaveService.GetLeaveTypesByGender(gender);
    //    ViewData["LeaveTypes"] = leaveTypes;

    //    return View(leave);
    //}
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

            // Apply leave (this will also update the leave balance)
            await _leaveService.ApplyLeaveAsync(leave, employeeId);

            _logger.LogInformation("Leave application submitted for Employee ID: {EmployeeId}.", employeeId);
            return RedirectToAction(nameof(ViewLeave));
        }

        var gender = HttpContext.Session.GetString("Gender");
        ViewData["Gender"] = gender;

        var leaveTypes = _leaveService.GetLeaveTypesByGender(gender);
        ViewData["LeaveTypes"] = leaveTypes;

        return View(leave);
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
            leaves = await _leaveService.GetAllLeavesAsync();
            if (!string.IsNullOrEmpty(empId) && int.TryParse(empId, out int employeeIdFilter))
            {
                leaves = leaves.Where(l => l.empId == employeeIdFilter).ToList();
            }
        }
        else if (int.TryParse(employeeIdString, out int employeeId))
        {
            leaves = await _leaveService.GetLeavesByEmployeeAsync(employeeId);

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

    #region Search Leave by EmpId

    public async Task<IActionResult> SearchLeave(string empId)
    {
        IList<ViewLeave> filteredLeaves;

        if (string.IsNullOrEmpty(empId))
        {
            // If no empId is provided, return all leaves
            filteredLeaves = await _leaveService.GetAllLeavesAsync();
        }
        else
        {
            // If empId is provided, filter the leaves
            filteredLeaves = await _leaveService.GetFilteredLeavesAsync(empId);
        }

        return Json(filteredLeaves);
    }

    #endregion

    #region Update Leave Status

    // Only for admins
    public async Task<IActionResult> UpdateLeaveStatusAsync(int leaveId, string status)
    {
        var approver = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(approver))
        {
            return RedirectToAction("Login", "Authentication");
        }

        await _leaveService.UpdateLeaveStatusAsync(leaveId, status, approver);

        return RedirectToAction(nameof(ViewLeave));
    }

    #endregion

    #region Delete Leave

    public async Task<IActionResult> DeleteLeave(int leaveId)
    {
        try
        {
            await _leaveService.DeleteLeaveAsync(leaveId);
            TempData["SuccessMessage"] = "Leave deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Failed to delete leave: " + ex.Message;
        }
        return RedirectToAction(nameof(ViewLeave));
    }

    #endregion

    #region Initialize Leave Balance

    // Initialize leave balance for a new employee
    public async Task<IActionResult> InitializeLeaveBalance(int empId, string gender)
    {
        await _leaveBalanceService.InitializeLeaveBalanceAsync(empId, gender);
        return Ok("Leave balance initialized.");
    }

    #endregion
}