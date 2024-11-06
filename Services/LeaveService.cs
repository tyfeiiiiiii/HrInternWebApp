using System.Collections.Generic;
using System.Threading.Tasks;
using HrInternWebApp.Entity;
using NHibernate;
using NHibernate.Transform;
using Microsoft.Extensions.Logging;
using HrInternWebApp.Models.Identity;
using ISession = NHibernate.ISession;

public class LeaveService
{
    private readonly ISession _session;
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(ISession session, ILogger<LeaveService> logger)
    {
        _session = session;
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _logger = logger;
    }

    public async Task TestSessionAsync()
    {
        // This method uses a new session from _sessionFactory for testing purposes
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                // Example operation
                var employee = await session.GetAsync<Employee>(3); // Make sure employee ID 3 exists
                _logger.LogInformation("Employee retrieved: {0}", employee?.username);

                // Insert a leave record
                var leave = new Leave
                {
                    empId = 3,
                    leaveType = "Test Leave",
                    startDate = DateTime.Now,
                    endDate = DateTime.Now.AddDays(1),
                    reason = "Testing",
                    status = "Pending"
                };

                await session.SaveAsync(leave);
                await transaction.CommitAsync();

                _logger.LogInformation("Leave successfully saved with leave ID: {0}", leave.leaveId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestSessionAsync");
                await transaction.RollbackAsync();
            }
        }
    }

    #region Apply Leave
    public async Task ApplyLeave(ApplyLeave leave, int employeeId)
    {
        try
        {
            // Associate the employee with the leave request
            leave.employee = await _session.GetAsync<Employee>(employeeId);

            using (var transaction = _session.BeginTransaction())
            {
                await _session.SaveAsync(leave);
                await transaction.CommitAsync();
            }

            _logger.LogInformation($"Leave application submitted for employee ID: {employeeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying for leave");
            throw new Exception("Failed to apply for leave", ex);
        }
    }

    #endregion

    #region Fetch Leaves

    // Fetch leave applications by employee ID (for users)
    public async Task<IList<EditLeave>> GetLeavesByEmployee(int employeeId)
    {
        try
        {
            Leave leaveAlias = null;
            Employee employeeAlias = null;
            EditLeave getModel = null;

            var leaves = await _session.QueryOver(() => leaveAlias)
                .JoinAlias(() => leaveAlias.employee, () => employeeAlias)
                .Where(() => leaveAlias.employee.empId == employeeId)
                .SelectList(list => list
                    .Select(() => leaveAlias.leaveId).WithAlias(() => getModel.leaveId)
                    .Select(() => leaveAlias.leaveType).WithAlias(() => getModel.leaveType)
                    .Select(() => leaveAlias.startDate).WithAlias(() => getModel.startDate)
                    .Select(() => leaveAlias.endDate).WithAlias(() => getModel.endDate)
                    .Select(() => leaveAlias.reason).WithAlias(() => getModel.reason)
                    .Select(() => leaveAlias.status).WithAlias(() => getModel.status)
                    .Select(() => leaveAlias.approver).WithAlias(() => getModel.approver)
                    .Select(() => employeeAlias.empId).WithAlias(() => getModel.empId)
                    .Select(() => employeeAlias.username).WithAlias(() => getModel.username)
                )
                .TransformUsing(Transformers.AliasToBean<EditLeave>())
                .ListAsync<EditLeave>();

            _logger.LogInformation($"Retrieved {leaves.Count} leave(s) for employee ID: {employeeId}");
            return leaves;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaves for employee ID: {employeeId}");
            throw;
        }
    }


    public async Task<IList<EditLeave>> GetAllLeaves()
    {
        try
        {
            Leave leaveAlias = null;
            Employee employeeAlias = null;
            EditLeave getModel = null;

            var leaves = await _session.QueryOver(() => leaveAlias)
                .JoinAlias(() => leaveAlias.employee, () => employeeAlias)
                .SelectList(list => list
                    .Select(() => leaveAlias.leaveId).WithAlias(() => getModel.leaveId)
                    .Select(() => leaveAlias.leaveType).WithAlias(() => getModel.leaveType)
                    .Select(() => leaveAlias.startDate).WithAlias(() => getModel.startDate)
                    .Select(() => leaveAlias.endDate).WithAlias(() => getModel.endDate)
                    .Select(() => leaveAlias.reason).WithAlias(() => getModel.reason)
                    .Select(() => leaveAlias.status).WithAlias(() => getModel.status)
                    .Select(() => leaveAlias.approver).WithAlias(() => getModel.approver)
                    .Select(() => employeeAlias.empId).WithAlias(() => getModel.empId)
                    .Select(() => employeeAlias.username).WithAlias(() => getModel.username)
                )
                .TransformUsing(Transformers.AliasToBean<EditLeave>())
                .ListAsync<EditLeave>();

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
