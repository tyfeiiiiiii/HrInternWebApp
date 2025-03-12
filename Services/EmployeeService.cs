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
    public class EmployeeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(AppDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Get All Employees
        public async Task<IList<ViewEmp>> GetAllEmployeesAsync()
        {
            try
            {
                return await _context.Employees
                                     .Select(e => new ViewEmp
                                     {
                                         empId = e.empId,
                                         username = e.username,
                                         Role = e.Role,
                                         Department = e.Department,
                                         email = e.email,
                                         profilePic = e.profilePic
                                     })
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees.");
                throw;
            }
        }
        #endregion

        #region Get Employee By ID
        public async Task<ViewEmp> GetEmployeeByIdAsync(int empId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(empId);

                return employee == null ? null : new ViewEmp
                {
                    empId = employee.empId,
                    username = employee.username,
                    Role = employee.Role,
                    Department = employee.Department,
                    email = employee.email,
                    profilePic = employee.profilePic
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee with ID {EmployeeId}", empId);
                throw;
            }
        }
        #endregion

        #region Update Employee
        public async Task UpdateEmployeeAsync(ViewEmp viewEmp)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(viewEmp.empId);
                if (employee != null)
                {
                    employee.username = viewEmp.username;
                    employee.email = viewEmp.email;
                    employee.Role = viewEmp.Role;
                    employee.Department = viewEmp.Department;
                    if (!string.IsNullOrEmpty(viewEmp.profilePic))
                    {
                        employee.profilePic = viewEmp.profilePic;
                    }

                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update employee");
                throw;
            }
        }
        #endregion

        #region Delete Employee
        public async Task DeleteEmployeeAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee != null)
                {
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning($"Employee with ID {employeeId} not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete employee");
                throw;
            }
        }
        #endregion

        #region Get Filtered Employees
        public async Task<IList<ViewEmp>> GetFilteredEmployeesAsync(string empId)
        {
            try
            {
                return await _context.Employees
                                     .Where(e => e.empId.ToString().Contains(empId) || e.empId == int.Parse(empId))
                                     .Select(e => new ViewEmp
                                     {
                                         empId = e.empId,
                                         username = e.username,
                                         Role = e.Role,
                                         Department = e.Department,
                                         email = e.email,
                                         profilePic = e.profilePic
                                     })
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering employees.");
                throw;
            }
        }
        #endregion
    }

}
