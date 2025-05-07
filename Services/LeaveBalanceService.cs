using System;
using System.Threading.Tasks;
using NHibernate;
using HrInternWebApp.Entity;
using Microsoft.Extensions.Logging;
using NHibernate.Transaction;
using HrInternWebApp.Models.Identity;

namespace HrInternWebApp.Services
{
    public class LeaveBalanceService
    {
        private readonly NHibernate.ISession _session;
        private readonly ILogger<LeaveBalanceService> _logger;

        public LeaveBalanceService(NHibernate.ISession session, ILogger<LeaveBalanceService> logger)
        {
            _session = session;
            _logger = logger;
        }

        // Initialize leave balances when an employee signs up
        public async Task InitializeLeaveBalanceAsync(int empId, string gender)
        {
            var leaveBalance = new LeaveBalance
            {
                Employee = await _session.GetAsync<Employee>(empId),
                MedicalLeave = 14,
                AnnualLeave = 12,
                Hospitalization = 60,
                Examination = 5,
                Marriage = 7,
                PaternityLeave = gender == "Male" ? 7 : 0,
                MaternityLeave = gender == "Female" ? 90 : 0,
                ChildcareLeave = gender == "Female" ? 14 : 0,
                UnpaidLeave = 60,
                EmergencyLeave = 10,
                LastUpdated = DateTime.Now
            };

            await _session.SaveOrUpdateAsync(leaveBalance);
        }
        public async Task ApplyLeaveAsync(ApplyLeave leaveRequest, int employeeId)
        {
            try
            {
                using (var transaction = _session.BeginTransaction())
                {
                    var employee = await _session.GetAsync<Employee>(employeeId);
                    if (employee == null)
                    {
                        _logger.LogWarning($"No employee found with ID {employeeId}");
                        throw new InvalidOperationException($"Employee with ID {employeeId} does not exist.");
                    }

                    // Check for overlapping leave
                    var hasOverlap = await _session.QueryOver<Leave>()
                        .Where(x => x.employee.empId == employeeId &&
                                    x.startDate <= leaveRequest.endDate &&
                                    x.endDate >= leaveRequest.startDate)
                        .RowCountAsync() > 0;

                    if (hasOverlap)
                    {
                        throw new InvalidOperationException("Overlapping leave exists for the selected dates.");
                    }

                    // Save Leave Application
                    var leaveEntity = new Leave
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

                    var leaveBalance = await _session.QueryOver<LeaveBalance>()
                        .Where(x => x.Employee.empId == employeeId)
                        .SingleOrDefaultAsync();

                    if (leaveBalance != null)
                    {
                        DeductLeave(leaveBalance, leaveRequest.leaveType, appliedLeaveDays);
                        leaveBalance.LastUpdated = DateTime.Now;
                        await _session.UpdateAsync(leaveBalance);
                    }

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

        // Fetch leave balance
        public async Task<LeaveBalance> GetLeaveBalanceAsync(int empId)
        {
            _logger.LogInformation("Fetching leave balance for Employee ID: {EmployeeId}", empId);

            var leaveBalance = await _session.QueryOver<LeaveBalance>()
                .Where(lb => lb.Employee.empId == empId) // Use Employee.EmpId
                .SingleOrDefaultAsync();

            if (leaveBalance == null)
            {
                _logger.LogWarning("No leave balance found for Employee ID: {EmployeeId}", empId);
                return null;
            }

            _logger.LogInformation("Leave balance retrieved for Employee ID: {EmployeeId}", empId);
            return leaveBalance;
        }

        // Update leave balance
        public async Task UpdateLeaveBalanceAsync(int empId, string leaveType, int days)
        {
            _logger.LogInformation("Updating leave balance for Employee ID: {EmployeeId}, Leave Type: {LeaveType}, Days: {Days}", empId, leaveType, days);

            using (ITransaction transaction = _session.BeginTransaction())
            {
                var leaveBalance = await _session.QueryOver<LeaveBalance>()
                    .Where(lb => lb.Employee.empId == empId) // Use Employee.EmpId
                    .SingleOrDefaultAsync();

                if (leaveBalance == null)
                {
                    _logger.LogWarning("No leave balance found for Employee ID: {EmployeeId}", empId);
                    throw new InvalidOperationException("Leave balance not found.");
                }

                switch (leaveType)
                {
                    case "Medical Leave":
                        leaveBalance.MedicalLeaveUsed += days;
                        break;
                    case "Annual Leave":
                        leaveBalance.AnnualLeaveUsed += days;
                        break;
                    case "Hospitalization":
                        leaveBalance.HospitalizationUsed += days;
                        break;
                    case "Examination":
                        leaveBalance.ExaminationUsed += days;
                        break;
                    case "Marriage":
                        leaveBalance.MarriageUsed += days;
                        break;
                    case "Paternity Leave":
                        leaveBalance.PaternityLeaveUsed += days;
                        break;
                    case "Maternity Leave":
                        leaveBalance.MaternityLeaveUsed += days;
                        break;
                    case "Childcare Leave":
                        leaveBalance.ChildcareLeaveUsed += days;
                        break;
                    case "Unpaid Leave":
                        leaveBalance.UnpaidLeaveUsed += days;
                        break;
                    case "Emergency Leave":
                        leaveBalance.EmergencyLeaveUsed += days;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid leave type.");
                }

                leaveBalance.LastUpdated = DateTime.Now;
                await _session.SaveOrUpdateAsync(leaveBalance);
                await transaction.CommitAsync();
                _logger.LogInformation("Leave balance updated for Employee ID: {EmployeeId}", empId);
            }
        }
            private void DeductLeave(LeaveBalance lb, string leaveType, int days)
        {
            switch (leaveType)
            {
                case "Medical Leave": lb.MedicalLeaveUsed += days; break;
                case "Annual Leave": lb.AnnualLeaveUsed += days; break;
                case "Hospitalization": lb.HospitalizationUsed += days; break;
                case "Examination": lb.ExaminationUsed += days; break;
                case "Marriage": lb.MarriageUsed += days; break;
                case "Paternity Leave": lb.PaternityLeaveUsed += days; break;
                case "Maternity Leave": lb.MaternityLeaveUsed += days; break;
                case "Childcare Leave": lb.ChildcareLeaveUsed += days; break;
                case "Unpaid Leave": lb.UnpaidLeaveUsed += days; break;
                case "Emergency Leave": lb.EmergencyLeaveUsed += days; break;
                default:
                    throw new InvalidOperationException("Invalid leave type.");
            }
        }
    }
}