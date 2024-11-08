using System.Collections.Generic;
using System.Threading.Tasks;
using HrInternWebApp.Entity;
using NHibernate;
using NHibernate.Transform;
using Microsoft.Extensions.Logging;
using HrInternWebApp.Models.Identity;
using ISession = NHibernate.ISession;
using NHibernate.Linq;

public class LeaveService
{
    private readonly ISession _session;
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(ISession session, ISessionFactory sessionFactory, ILogger<LeaveService> logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    #region Apply Leave
    public async Task ApplyLeave(ApplyLeave leave, int employeeId)
    {
        try
        {
            if (_session.Connection.State == System.Data.ConnectionState.Closed)
            {
                _session.Connection.Open();
            }
            _logger.LogInformation($"Entering ApplyLeave for Employee ID: {employeeId}");
            //leave.employee = await _session.GetAsync<Employee>(employeeId);
            //var test = await _session.GetAsync<Employee>(employeeId);
            if (leave.employee == null)
            {
                _logger.LogWarning($"No employee found with ID {employeeId}");
                throw new InvalidOperationException($"Employee with ID {employeeId} does not exist.");
            }

            using (var transaction = _session.BeginTransaction())
            {
                await _session.SaveAsync(leave);
                await transaction.CommitAsync();
                _logger.LogInformation($"Leave record successfully saved for employee ID {employeeId}");
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

    // Fetch leave applications by employee ID (for users)
    public async Task<IList<ViewLeave>> GetLeavesByEmployee(int employeeId)
    {
        Leave leaveAlias = null;
        Employee employeeAlias = null;
        ViewLeave viewLeave = null;

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


    public async Task<IList<ViewLeave>> GetAllLeaves()
    {
        try
        {
            Leave leaveAlias = null;
            Employee employeeAlias = null;
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

    public async Task UpdateLeaveStatus(int leaveId, string newStatus, string approver)
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
                    await _session.UpdateAsync(leave);
                    await transaction.CommitAsync();
                }

                _logger.LogInformation($"Leave status updated for leave ID: {leaveId} by {approver}");
            }
            else
            {
                _logger.LogWarning($"Leave with ID {leaveId} not found");
                throw new KeyNotFoundException($"Leave with ID {leaveId} not found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave status for leave ID: {leaveId}");
            throw new Exception("Failed to update leave status", ex);
        }
    }

}
