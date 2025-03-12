using HrInternWebApp.Data;
using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LeaveController : Controller
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(AppDbContext context, IMemoryCache cache, ILogger<LeaveController> logger)
    {
        _context = context;
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
            leaveTypes = _context.LeaveTypes
                .Where(l => l.Gender == gender || l.Gender == "All")
                .Select(l => l.Name)
                .ToList();

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
            leave.EmployeeId = employeeId;
            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Leave application submitted for Employee ID: {EmployeeId}.", employeeId);
            return RedirectToAction(nameof(ViewLeave));
        }

        var gender = HttpContext.Session.GetString("Gender");
        ViewData["Gender"] = gender;
        ViewData["LeaveTypes"] = _context.LeaveTypes
            .Where(l => l.Gender == gender || l.Gender == "All")
            .Select(l => l.Name)
            .ToList();

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
            leaves = await _context.Leaves
                .Select(l => new ViewLeave { Id = l.Id, EmployeeId = l.EmployeeId, Status = l.Status })
                .ToListAsync();
            if (!string.IsNullOrEmpty(empId) && int.TryParse(empId, out int employeeIdFilter))
            {
                leaves = leaves.Where(l => l.EmployeeId == employeeIdFilter).ToList();
            }
        }
        else if (int.TryParse(employeeIdString, out int employeeId))
        {
            leaves = await _context.Leaves
                .Where(l => l.EmployeeId == employeeId)
                .Select(l => new ViewLeave { Id = l.Id, EmployeeId = l.EmployeeId, Status = l.Status })
                .ToListAsync();

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
            filteredLeaves = await _context.Leaves
                .Select(l => new ViewLeave { Id = l.Id, EmployeeId = l.EmployeeId, Status = l.Status })
                .ToListAsync();
        }
        else
        {
            filteredLeaves = await _context.Leaves
                .Where(l => l.EmployeeId.ToString().Contains(empId))
                .Select(l => new ViewLeave { Id = l.Id, EmployeeId = l.EmployeeId, Status = l.Status })
                .ToListAsync();
        }

        return Json(filteredLeaves);
    }
    #endregion

    #region Update Leave Status
    public async Task<IActionResult> UpdateLeaveStatusAsync(int leaveId, string status)
    {
        var approver = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(approver))
        {
            return RedirectToAction("Login", "Authentication");
        }

        var leave = await _context.Leaves.FindAsync(leaveId);
        if (leave != null)
        {
            leave.Status = status;
            leave.ApprovedBy = approver;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ViewLeave));
    }
    #endregion

    #region Delete Leave
    public async Task<IActionResult> DeleteLeave(int leaveId)
    {
        try
        {
            var leave = await _context.Leaves.FindAsync(leaveId);
            if (leave != null)
            {
                _context.Leaves.Remove(leave);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Leave not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Failed to delete leave: " + ex.Message;
        }
        return RedirectToAction(nameof(ViewLeave));
    }
    #endregion
}
