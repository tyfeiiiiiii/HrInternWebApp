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
                AnnualLeave = 14,
                Hospitalization = 60,
                Examination = 5,
                Marriage = 3,
                PaternityLeave = gender == "Male" ? 7 : 0,
                MaternityLeave = gender == "Female" ? 98 : 0,
                ChildcareLeave = gender == "Female" ? 6 : 0,
                UnpaidLeave = 365,
                EmergencyLeave = 5,
                LastUpdated = DateTime.Now
            };

            await _session.SaveOrUpdateAsync(leaveBalance);
        }

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
    }
}