using ISession = NHibernate.ISession;
using HrInternWebApp.Models.Identity;
using NHibernate;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

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
    public void ApplyLeave(ApplyLeave leave, int employeeId)
    {
        // Associate the employee with the leave request
        leave.Employee = _session.Get<Employee>(employeeId);

        // Check if the leave should be treated as a ViewLeave (depending on your logic)
        ViewLeave viewLeave = new ViewLeave
        {
            LeaveType = leave.LeaveType,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            Reason = leave.Reason,
            Status = "Pending",
            Approver = "Pending",
            Employee = leave.Employee
        };

        using (var transaction = _session.BeginTransaction())
        {
            try
            {
                // Save as ViewLeave to ensure correct LeaveClassType is set
                _session.Save(viewLeave);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Failed to apply leave", ex);
            }
        }
    }
    #endregion

    #region Fetch Leaves

    // Fetch leave applications by employee ID (for users)
    public IList<ViewLeave> GetLeavesByEmployee(int employeeId)
    {
        try
        {
            var leaves = _session.Query<ViewLeave>()
                                 .Where(l => l.Employee.empId == employeeId)
                                 .ToList();

            _logger.LogInformation($"Retrieved {leaves.Count} leave(s) for employee ID: {employeeId}");
            return leaves;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaves for employee ID: {EmployeeId}", employeeId);
            throw;
        }
    }

    // Fetch all leave applications (for admins)
    public IList<ViewLeave> GetAllLeaves()
    {
        try
        {
            var leaves = _session.Query<ViewLeave>().ToList();

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
    public void UpdateLeaveStatus(int leaveId, string newStatus, string approver)
    {
        var leave = _session.Get<ViewLeave>(leaveId);
        if (leave != null)
        {
            leave.Status = newStatus;
            leave.Approver = approver;

            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    _session.Update(leave);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Failed to update leave status", ex);
                }
            }
        }
        else
        {
            throw new KeyNotFoundException($"Leave with ID {leaveId} not found.");
        }
    }
    #endregion
}
