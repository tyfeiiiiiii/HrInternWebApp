using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using HrInternWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISession = NHibernate.ISession;

public class LeaveController : Controller
{
    private readonly LeaveService _leaveService;
    private readonly LeaveBalanceService _leaveBalanceService;
    private readonly EmailService _emailService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LeaveController> _logger;
    private readonly ISession _session;

    public LeaveController(
        LeaveService leaveService,
        LeaveBalanceService leaveBalanceService,
        EmailService emailService,
        IMemoryCache cache,
        ILogger<LeaveController> logger,
        ISession session)
    {
        _leaveService = leaveService;
        _leaveBalanceService = leaveBalanceService;
        _cache = cache;
        _logger = logger;
        _emailService = emailService;
        _session = session;
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

    [HttpPost]
    public async Task<IActionResult> ApplyLeave(ApplyLeave leave)
    {
        var employeeIdString = HttpContext.Session.GetString("EmployeeId");

        if (ModelState.IsValid)
        {
            // Validate and parse employee ID
            if (string.IsNullOrEmpty(employeeIdString) || !int.TryParse(employeeIdString, out int employeeId))
            {
                ModelState.AddModelError("", "Invalid Employee ID");
                // Avoid calling the method with an invalid employeeId (0 or null)
                return View(leave);
            }

            // Calculate leave days
            var leaveDays = (leave.endDate.Value - leave.startDate.Value).Days + 1;

            // Get leave balance
            var leaveBalance = await _leaveBalanceService.GetLeaveBalanceAsync(employeeId);

            // Determine remaining balance
            int remainingBalance = leave.leaveType switch
            {
                "Annual Leave" => leaveBalance.AnnualLeave - leaveBalance.AnnualLeaveUsed,
                "Medical Leave" => leaveBalance.MedicalLeave - leaveBalance.MedicalLeaveUsed,
                "Hospitalization" => leaveBalance.Hospitalization - leaveBalance.HospitalizationUsed,
                "Examination" => leaveBalance.Examination - leaveBalance.ExaminationUsed,
                "Marriage" => leaveBalance.Marriage - leaveBalance.MarriageUsed,
                "Paternity Leave" => leaveBalance.PaternityLeave - leaveBalance.PaternityLeaveUsed,
                "Maternity Leave" => leaveBalance.MaternityLeave - leaveBalance.MaternityLeaveUsed,
                "Childcare Leave" => leaveBalance.ChildcareLeave - leaveBalance.ChildcareLeaveUsed,
                "Unpaid Leave" => leaveBalance.UnpaidLeave - leaveBalance.UnpaidLeaveUsed,
                "Emergency Leave" => leaveBalance.EmergencyLeave - leaveBalance.EmergencyLeaveUsed,
                _ => 0
            };

            if (leaveDays > remainingBalance)
            {
                ModelState.AddModelError("", "You do not have enough leave balance. Please contact the admin manually.");
                // Reload leave data to show balance
                ReloadLeaveDataForViewInternal(employeeId);
                return View(leave);
            }

            // ✅ Try applying the leave
            bool success = await _leaveService.ApplyLeaveAsync(leave, employeeId);
            if (!success)
            {
                ModelState.AddModelError("", "Your leave request overlaps with an existing request.");
                // Reload leave data to show balance
                ReloadLeaveDataForViewInternal(employeeId);
                return View(leave);
            }

            _logger.LogInformation("Leave application submitted for Employee ID: {EmployeeId}.", employeeId);
            return RedirectToAction(nameof(ViewLeave));
        }

        // Reload leave data if the employee ID is valid
        if (int.TryParse(employeeIdString, out int validEmployeeId))
        {
            ReloadLeaveDataForViewInternal(validEmployeeId); // Reload leave data to show balance
        }

        return View(leave);
    }

    void ReloadLeaveDataForViewInternal(int employeeId)
    {
        var gender = HttpContext.Session.GetString("Gender");
        ViewData["Gender"] = gender;
        var leaveTypes = _leaveService.GetLeaveTypesByGender(gender);
        ViewData["LeaveTypes"] = leaveTypes;

        // Reload the leave balance to show on the view
        var leaveBalance = _leaveBalanceService.GetLeaveBalanceAsync(employeeId).Result;
        ViewData["LeaveBalance"] = leaveBalance;
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

    //#region Update Leave Status

    //// Only for admins
    //public async Task<IActionResult> UpdateLeaveStatusAsync(int leaveId, string status)
    //{
    //    var approver = HttpContext.Session.GetString("Username");

    //    if (string.IsNullOrEmpty(approver))
    //    {
    //        return RedirectToAction("Login", "Authentication");
    //    }

    //    await _leaveService.UpdateLeaveStatusAsync(leaveId, status, approver);

    //    return RedirectToAction(nameof(ViewLeave));
    //}

    //#endregion
    #region Update Leave Status

    // Only for admins
    public async Task<IActionResult> UpdateLeaveStatusAsync(int leaveId, string status)
    {
        var approver = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(approver))
        {
            return RedirectToAction("Login", "Authentication");
        }

        try
        {
            // Fetch the leave request from the database using NHibernate
            var leave = await _session.GetAsync<Leave>(leaveId);

            if (leave != null)
            {
                // Update leave status and approver
                leave.status = status;
                leave.approver = approver;

                using (var transaction = _session.BeginTransaction())
                {
                    try
                    {
                        // Commit the transaction and update the leave in the database
                        await _session.UpdateAsync(leave);
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Leave status updated for leave ID {leaveId}");

                        // Ensure Employee object is loaded for email
                        NHibernateUtil.Initialize(leave.employee);

                        // Send email to the employee if the email exists
                        if (leave.employee != null && !string.IsNullOrEmpty(leave.employee.email))
                        {
                            // Prepare the email content
                            string subject = "Leave Request Status Update";
                            string body = $"<p>Dear {leave.employee.username},<br>Your leave request has been <b>{status}</b> by {approver}.</p>";

                            // Send the email notification to the employee
                            _emailService.SendEmail(leave.employee.email, subject, body);

                            _logger.LogInformation($"Email sent to {leave.employee.email}");
                        }

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Failed to update leave status");
                        throw new Exception("Failed to update leave status", ex);
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Leave with ID {leaveId} not found.");
                throw new KeyNotFoundException($"Leave with ID {leaveId} not found.");
            }

            return RedirectToAction(nameof(ViewLeave));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating leave status.");
            throw;
        }
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