using ISession = NHibernate.ISession;
using HrInternWebApp.Models.Identity;
using NHibernate;
using NHibernate.Transform;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using HrInternWebApp.Entity;

public class LeaveService
{
    #region Fields
    private readonly ISession _session;
    private readonly ILogger<LeaveService> _logger;
    #endregion

    #region Constructor
    public LeaveService(ISession session, ILogger<LeaveService> logger)
    {
        _session = session;
        _logger = logger;
    }
    #endregion

    #region Apply Leave
    public async Task ApplyLeaveAsync(ApplyLeave leaveRequest, int employeeId)
    {
        try
        {
            Employee employee = await _session.GetAsync<Employee>(employeeId);
            if (employee == null)
            {
                _logger.LogWarning($"No employee found with ID {employeeId}");
                throw new InvalidOperationException($"Employee with ID {employeeId} does not exist.");
            }

            // Create a Leave entity using data from ApplyLeave
            Leave leaveEntity = new Leave
            {
                leaveType = leaveRequest.leaveType,
                startDate = leaveRequest.startDate,
                endDate = leaveRequest.endDate,
                reason = leaveRequest.reason,
                status = "Pending",
                approver = "Pending",
                employee = employee 
            };

            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    await _session.SaveAsync(leaveEntity);
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Leave record successfully saved for employee ID {employeeId}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to apply leave");
                    throw new Exception("Failed to apply leave", ex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying for leave.");
            throw;
        }
    }

    #endregion

    #region Fetch Leaves

    // Fetch leave applications by employee ID (for users) using QueryOver with join logic
    public async Task<IList<ViewLeave>> GetLeavesByEmployeeAsync(int employeeId)
    {
        try
        {
            Employee employeeAlias = null;
            Leave leaveAlias = null;
            ViewLeave viewLeave = null;

            // Use synchronous QueryOver for joins
            var leaves = await _session.QueryOver(() => leaveAlias)
                .JoinAlias(() => leaveAlias.employee, () => employeeAlias)
                .Where(() => employeeAlias.empId == employeeId)
                .SelectList(list => list
                    .Select(() => leaveAlias.leaveId).WithAlias(() => viewLeave.leaveId)
                    .Select(() => leaveAlias.leaveType).WithAlias(() => viewLeave.leaveType)
                    .Select(() => leaveAlias.startDate).WithAlias(() => viewLeave.startDate)
                    .Select(() => leaveAlias.endDate).WithAlias(() => viewLeave.endDate)
                    .Select(() => leaveAlias.reason).WithAlias(() => viewLeave.reason)
                    .Select(() => leaveAlias.status).WithAlias(() => viewLeave.status)
                    .Select(() => leaveAlias.approver).WithAlias(() => viewLeave.approver)
                    .Select(() => employeeAlias.empId).WithAlias(() => viewLeave.empId)
                    .Select(() => employeeAlias.username).WithAlias(() => viewLeave.username)
                )
                .TransformUsing(Transformers.AliasToBean<ViewLeave>())
                .ListAsync<ViewLeave>();

            _logger.LogInformation($"Retrieved {leaves.Count} leave(s) for employee ID: {employeeId}");
            return leaves;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaves for employee ID: {EmployeeId}", employeeId);
            throw;
        }
    }

    // Fetch all leave applications (for admins) using synchronous QueryOver (if needed)
    public async Task<IList<ViewLeave>> GetAllLeavesAsync()
    {
        try
        {
            Employee employeeAlias = null;
            Leave leaveAlias = null;
            ViewLeave viewLeave = null;

            var leaves = await _session.QueryOver(() => leaveAlias)
                .JoinAlias(() => leaveAlias.employee, () => employeeAlias)
                .SelectList(list => list
                    .Select(() => leaveAlias.leaveId).WithAlias(() => viewLeave.leaveId)
                    .Select(() => leaveAlias.leaveType).WithAlias(() => viewLeave.leaveType)
                    .Select(() => leaveAlias.startDate).WithAlias(() => viewLeave.startDate)
                    .Select(() => leaveAlias.endDate).WithAlias(() => viewLeave.endDate)
                    .Select(() => leaveAlias.reason).WithAlias(() => viewLeave.reason)
                    .Select(() => leaveAlias.status).WithAlias(() => viewLeave.status)
                    .Select(() => leaveAlias.approver).WithAlias(() => viewLeave.approver)
                    .Select(() => employeeAlias.empId).WithAlias(() => viewLeave.empId)
                    .Select(() => employeeAlias.username).WithAlias(() => viewLeave.username)
                )
                .TransformUsing(Transformers.AliasToBean<ViewLeave>())
                .ListAsync<ViewLeave>();

            _logger.LogInformation($"Retrieved {leaves.Count} leave(s) for admin view");
            return leaves;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all leaves for admin");
            throw;
        }
    }
    #endregion

    #region Update Leave Status
    public async Task UpdateLeaveStatusAsync(int leaveId, string newStatus, string approver)
    {
        try
        {
            var leave = await _session.GetAsync<Leave>(leaveId);
            if (leave != null)
            {
                leave.status = newStatus;
                leave.approver = approver;

                using (var transaction = _session.BeginTransaction())
                {
                    try
                    {
                        await _session.UpdateAsync(leave);
                        await transaction.CommitAsync();
                        _logger.LogInformation($"Leave status updated for leave ID {leaveId}");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating leave status.");
            throw;
        }
    }
    #endregion
}
