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

        // Cache leave types by gender
        string cacheKey = $"LeaveTypes_{gender}";
        if (!_cache.TryGetValue(cacheKey, out List<string> leaveTypes))
        {
            // Cache miss, fetch from service
            _logger.LogInformation("Cache miss for LeaveTypes with gender: {Gender}. Fetching from service.", gender);
            leaveTypes = leaveServices.GetLeaveTypesByGender(gender);

            // Store in cache with a 30-minute expiration
            _cache.Set(cacheKey, leaveTypes, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            _logger.LogInformation("LeaveTypes cached with key: {CacheKey} for 30 minutes.", cacheKey);
        }
        else
        {
            _logger.LogInformation("Cache hit for LeaveTypes with gender: {Gender}.", gender);
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

        // Cache key for leave records
        string cacheKey = $"Leaves_{role}_{empId ?? employeeIdString}";

        if (!_cache.TryGetValue(cacheKey, out leaves))
        {
            _logger.LogInformation("Cache miss for leaves with role: {Role} and empId: {EmpId}. Fetching from service.", role, empId ?? employeeIdString);

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

            _cache.Set(cacheKey, leaves, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            _logger.LogInformation("Leaves data cached with key: {CacheKey} for 30 minutes.", cacheKey);
        }
        else
        {
            _logger.LogInformation("Cache hit for leaves with role: {Role} and empId: {EmpId}.", role, empId ?? employeeIdString);
        }

        return View(leaves);
    }

    #endregion

    #region Search Leave by EmpId

    public async Task<IActionResult> SearchLeave(string empId)
    {
        IList<ViewLeave> filteredLeaves;

        // Cache key for searching leaves by empId
        string cacheKey = string.IsNullOrEmpty(empId) ? "AllLeaves" : $"FilteredLeaves_{empId}";

        if (!_cache.TryGetValue(cacheKey, out filteredLeaves))
        {
            _logger.LogInformation("Cache miss for search leaves with empId: {EmpId}. Fetching from service.", empId);

            // Cache miss, fetch from service
            if (string.IsNullOrEmpty(empId))
            {
                filteredLeaves = await leaveServices.GetAllLeavesAsync();
            }
            else
            {
                filteredLeaves = await leaveServices.GetAllLeavesAsync();
                filteredLeaves = filteredLeaves.Where(l => l.empId.ToString().Contains(empId)).ToList();
            }

            // Cache the result for 15 minutes
            _cache.Set(cacheKey, filteredLeaves, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });
            _logger.LogInformation("Filtered leaves cached with key: {CacheKey} for 15 minutes.", cacheKey);
        }
        else
        {
            _logger.LogInformation("Cache hit for search leaves with empId: {EmpId}.", empId);
        }

        return Json(filteredLeaves);
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

        // Invalidate cache for leave records when updating status
        string role = HttpContext.Session.GetString("Role");
        string empId = HttpContext.Session.GetString("EmployeeId");
        string cacheKey = $"Leaves_{role}_{empId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cache for leave records with key: {CacheKey} removed due to status update.", cacheKey);

        await leaveServices.UpdateLeaveStatusAsync(leaveId, status, approver);
        _logger.LogInformation("Leave status updated for Leave ID: {LeaveId} to {Status}.", leaveId, status);
        return RedirectToAction(nameof(LeaveController.ViewLeave));
    }

    #endregion
}
