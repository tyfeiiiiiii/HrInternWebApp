using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

public class LeaveController : Controller
{
    private readonly LeaveService leaveServices;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(LeaveService leaveService, IMemoryCache cache, ILogger<LeaveController> logger)
    {
        leaveServices = leaveService;
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
            leaveTypes = leaveServices.GetLeaveTypesByGender(gender);

            _cache.Set(cacheKey, leaveTypes, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        }
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
            _logger.LogInformation("Leave application submitted for Employee ID: {EmployeeId}.", employeeId);
            return RedirectToAction(nameof(ViewLeave));
        }

        var gender = HttpContext.Session.GetString("Gender");
        ViewData["Gender"] = gender;

        var leaveTypes = leaveServices.GetLeaveTypesByGender(gender);
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

    #region Search Leave by EmpId

    //public async Task<IActionResult> SearchLeave(string empId)
    //{
    //    IList<ViewLeave> filteredLeaves;

    //    if (string.IsNullOrEmpty(empId))
    //    {
    //        // If no empId is provided, return all leaves
    //        filteredLeaves = await leaveServices.GetAllLeavesAsync();
    //    }
    //    else
    //    {
    //        // Filter the leaves based on the provided empId
    //        filteredLeaves = await leaveServices.GetAllLeavesAsync();
    //        filteredLeaves = filteredLeaves.Where(l => l.empId.ToString().Contains(empId)).ToList();
    //    }

    //    return Json(filteredLeaves);
    //}
    public async Task<IActionResult> SearchLeave(string empId)
    {
        IList<ViewLeave> filteredLeaves;

        if (string.IsNullOrEmpty(empId))
        {
            // If no empId is provided, return all leaves
            filteredLeaves = await leaveServices.GetAllLeavesAsync();
        }
        else
        {
            // If empId is provided, filter the leaves
            filteredLeaves = await leaveServices.GetFilteredLeavesAsync(empId);
        }

        return Json(filteredLeaves);
    }



    #endregion

    #region Update Leave Status

    // only for admins
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

    #region Delete Leave
    public async Task<IActionResult> DeleteLeave (int leaveId)
    {
        try
        {
            await leaveServices.DeleteLeaveAsync(leaveId);
            TempData["SuccessMessage"] = "Leave deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Failed tod delete leave" + ex.Message;
        }
        return RedirectToAction(nameof(ViewLeave));
    }

    #endregion
}
