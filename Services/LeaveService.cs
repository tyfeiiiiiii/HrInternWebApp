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

    public List<string> GetLeaveTypesByGender(string gender)
    {
        if (gender == "Male")
        {
            return new List<string>
        {
            "Medical Leave",
            "Annual Leave",
            "Hospitalization",
            "Examination",
            "Marriage",
            "Paternity Leave",
            "Military Service Leave",
            "Unpaid Leave",
            "Emergency Leave"
        };
        }
        else if (gender == "Female")
        {
            return new List<string>
        {
            "Medical Leave",
            "Annual Leave",
            "Hospitalization",
            "Examination",
            "Marriage",
            "Maternity Leave",
            "Child Care Leave",
            "Unpaid Leave",
            "Emergency Leave"
        };
        }
        else
        {
            return new List<string> { "Medical Leave", "Annual Leave", "Unpaid Leave" };
        }
    }


    //    public async Task ApplyLeaveAsync(ApplyLeave leaveRequest, int employeeId)
    //{
    //    try
    //    {
    //        Employee employee = await _session.GetAsync<Employee>(employeeId);
    //        if (employee == null)
    //        {
    //            _logger.LogWarning($"No employee found with ID {employeeId}");
    //            throw new InvalidOperationException($"Employee with ID {employeeId} does not exist.");
    //        }

    //        using (var transaction = _session.BeginTransaction())
    //        {
    //            try
    //            {
    //                // Save Leave Application
    //                Leave leaveEntity = new Leave
    //                {
    //                    leaveType = leaveRequest.leaveType,
    //                    startDate = leaveRequest.startDate,
    //                    endDate = leaveRequest.endDate,
    //                    reason = leaveRequest.reason,
    //                    status = "Pending",
    //                    approver = "Pending",
    //                    employee = employee
    //                };

    //                await _session.SaveAsync(leaveEntity);

    //                // Calculate Applied Leave Days
    //                int appliedLeaveDays = (leaveRequest.endDate.Value - leaveRequest.startDate.Value).Days + 1;

    //                // Update Leave Balance
    //                var leaveBalance = await _session.QueryOver<LeaveBalance>()
    //                                                 .Where(x => x.Employee.empId == employeeId)
    //                                                 .SingleOrDefaultAsync();

    //                if (leaveBalance != null)
    //                {
    //                    switch (leaveRequest.leaveType)
    //                    {
    //                        case "Medical Leave":
    //                            leaveBalance.MedicalLeaveUsed += appliedLeaveDays;
    //                            break;
    //                        case "Annual Leave":
    //                            leaveBalance.AnnualLeaveUsed += appliedLeaveDays;
    //                            break;
    //                        case "Hospitalization":
    //                            leaveBalance.HospitalizationUsed += appliedLeaveDays;
    //                            break;
    //                        case "Examination":
    //                            leaveBalance.ExaminationUsed += appliedLeaveDays;
    //                            break;
    //                        case "Marriage":
    //                            leaveBalance.MarriageUsed += appliedLeaveDays;
    //                            break;
    //                        case "Paternity Leave":
    //                            leaveBalance.PaternityLeaveUsed += appliedLeaveDays;
    //                            break;
    //                        case "Maternity Leave":
    //                            leaveBalance.MaternityLeaveUsed += appliedLeaveDays;
    //                            break;
    //                        case "Child Care Leave":
    //                            leaveBalance.ChildcareLeaveUsed += appliedLeaveDays;
    //                            break;
    //                        case "Unpaid Leave":
    //                            leaveBalance.UnpaidLeaveUsed += appliedLeaveDays;
    //                            break;
    //                        case "Emergency Leave":
    //                            leaveBalance.EmergencyLeaveUsed += appliedLeaveDays;
    //                            break;
    //                    }

    //                    await _session.UpdateAsync(leaveBalance);
    //                }

    //                await transaction.CommitAsync();
    //                _logger.LogInformation($"Leave record successfully saved for employee ID {employeeId}");
    //            }
    //            catch (Exception ex)
    //            {
    //                await transaction.RollbackAsync();
    //                _logger.LogError(ex, "Failed to apply leave");
    //                throw new Exception("Failed to apply leave", ex);
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while applying for leave.");
    //        throw;
    //    }
    //}
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

            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    // Save Leave Application
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

                    await _session.SaveAsync(leaveEntity);

                    // Calculate Applied Leave Days
                    int appliedLeaveDays = (leaveRequest.endDate.Value - leaveRequest.startDate.Value).Days + 1;

                    // Update Leave Balance
                    var leaveBalance = await _session.QueryOver<LeaveBalance>()
                        .Where(x => x.Employee.empId == employeeId)
                        .SingleOrDefaultAsync();

                    if (leaveBalance != null)
                    {
                        switch (leaveRequest.leaveType)
                        {
                            case "Medical Leave":
                                leaveBalance.MedicalLeaveUsed += appliedLeaveDays;
                                break;
                            case "Annual Leave":
                                leaveBalance.AnnualLeaveUsed += appliedLeaveDays;
                                break;
                            case "Hospitalization":
                                leaveBalance.HospitalizationUsed += appliedLeaveDays;
                                break;
                            case "Examination":
                                leaveBalance.ExaminationUsed += appliedLeaveDays;
                                break;
                            case "Marriage":
                                leaveBalance.MarriageUsed += appliedLeaveDays;
                                break;
                            case "Paternity Leave":
                                leaveBalance.PaternityLeaveUsed += appliedLeaveDays;
                                break;
                            case "Maternity Leave":
                                leaveBalance.MaternityLeaveUsed += appliedLeaveDays;
                                break;
                            case "Child Care Leave":
                                leaveBalance.ChildcareLeaveUsed += appliedLeaveDays;
                                break;
                            case "Unpaid Leave":
                                leaveBalance.UnpaidLeaveUsed += appliedLeaveDays;
                                break;
                            case "Emergency Leave":
                                leaveBalance.EmergencyLeaveUsed += appliedLeaveDays;
                                break;
                        }

                        leaveBalance.LastUpdated = DateTime.Now;
                        await _session.UpdateAsync(leaveBalance);
                    }

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

    #region get Filtered Leave
    public async Task<IList<ViewLeave>> GetFilteredLeavesAsync(string empId)
    {
        try
        {
            Employee employeeAlias = null;
            Leave leaveAlias = null;
            ViewLeave viewLeave = null;

            var query = _session.QueryOver(() => leaveAlias)
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
                .TransformUsing(Transformers.AliasToBean<ViewLeave>());

            // While enter empId 
            if (!string.IsNullOrEmpty(empId))
            {
                int empIdInt;
                if (int.TryParse(empId, out empIdInt)) 
                {
                    query.Where(() => employeeAlias.empId == empIdInt);
                }
                else
                {
                    query.Where(() => employeeAlias.empId.ToString().Contains(empId)); 
                }
            }
            var filteredLeaves = await query.ListAsync<ViewLeave>();

            _logger.LogInformation($"Retrieved {filteredLeaves.Count} filtered leave(s) for employee ID: {empId}");
            return filteredLeaves;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving filtered leaves for employee ID: {empId}", empId);
            throw;
        }
    }
    #endregion

    #region Delete Leave 
    public async Task DeleteLeaveAsync (int leaveId)
    {
        using (var transaction=_session.BeginTransaction())
        {
            try
            {
                var leave = await _session.GetAsync<Leave>(leaveId);
                if(leave!=null)
                {
                    await _session.DeleteAsync(leave);
                    await transaction.CommitAsync();
                }
                else
                {
                    _logger.LogWarning($"Leave with ID {leaveId} not found.");
                }
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete leave");
                throw;
            }
        }
    }
    #endregion
}
