using HrInternWebApp.Data;
using HrInternWebApp.Models.Identity;
using HrInternWebApp.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace HrInternWebApp.Services {
    public class LeaveService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(AppDbContext context, ILogger<LeaveService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<string> GetLeaveTypesByGender(string gender)
        {
            return gender switch
            {
                "Male" => new List<string> { "Medical Leave", "Annual Leave", "Hospitalization", "Examination", "Marriage", "Paternity Leave", "Military Service Leave", "Unpaid Leave", "Emergency Leave" },
                "Female" => new List<string> { "Medical Leave", "Annual Leave", "Hospitalization", "Examination", "Marriage", "Maternity Leave", "Child Care Leave", "Unpaid Leave", "Emergency Leave" },
                _ => new List<string> { "Medical Leave", "Annual Leave", "Unpaid Leave" },
            };
        }

        public async Task ApplyLeaveAsync(ApplyLeave leaveRequest, int employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning($"No employee found with ID {employeeId}");
                    throw new InvalidOperationException($"Employee with ID {employeeId} does not exist.");
                }

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

                _context.Leaves.Add(leaveEntity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Leave record successfully saved for employee ID {employeeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying for leave.");
                throw;
            }
        }

        public async Task<IList<ViewLeave>> GetLeavesByEmployeeAsync(int employeeId)
        {
            return await _context.Leaves
                .Where(l => l.employee.empId == employeeId)
                .Select(l => new ViewLeave
                {
                    leaveId = l.leaveId,
                    leaveType = l.leaveType,
                    startDate = l.startDate,
                    endDate = l.endDate,
                    reason = l.reason,
                    status = l.status,
                    approver = l.approver,
                    empId = l.employee.empId,
                    username = l.employee.username
                })
                .ToListAsync();
        }

        public async Task<IList<ViewLeave>> GetAllLeavesAsync()
        {
            return await _context.Leaves
                .Select(l => new ViewLeave
                {
                    leaveId = l.leaveId,
                    leaveType = l.leaveType,
                    startDate = l.startDate,
                    endDate = l.endDate,
                    reason = l.reason,
                    status = l.status,
                    approver = l.approver,
                    empId = l.employee.empId,
                    username = l.employee.username
                })
                .ToListAsync();
        }

        public async Task UpdateLeaveStatusAsync(int leaveId, string newStatus, string approver)
        {
            var leave = await _context.Leaves.FindAsync(leaveId);
            if (leave == null)
            {
                _logger.LogWarning($"Leave with ID {leaveId} not found.");
                throw new KeyNotFoundException($"Leave with ID {leaveId} not found.");
            }

            leave.status = newStatus;
            leave.approver = approver;

            _context.Leaves.Update(leave);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Leave status updated for leave ID {leaveId}");
        }

        public async Task<IList<ViewLeave>> GetFilteredLeavesAsync(string empId)
        {
            return await _context.Leaves
                .Where(l => string.IsNullOrEmpty(empId) || l.employee.empId.ToString().Contains(empId))
                .Select(l => new ViewLeave
                {
                    leaveId = l.leaveId,
                    leaveType = l.leaveType,
                    startDate = l.startDate,
                    endDate = l.endDate,
                    reason = l.reason,
                    status = l.status,
                    approver = l.approver,
                    empId = l.employee.empId,
                    username = l.employee.username
                })
                .ToListAsync();
        }

        public async Task DeleteLeaveAsync(int leaveId)
        {
            var leave = await _context.Leaves.FindAsync(leaveId);
            if (leave == null)
            {
                _logger.LogWarning($"Leave with ID {leaveId} not found.");
                return;
            }

            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Leave with ID {leaveId} deleted.");
        }
    }

}
