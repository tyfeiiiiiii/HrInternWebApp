﻿using HrInternWebApp.Entity;
using HrInternWebApp.Models.Identity;
using NHibernate;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

public class EmployeeService
{
    private readonly ISession _session;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ISession session, ILogger<EmployeeService> logger)
    {
        _session = session;
        _logger = logger;
    }

    #region Get All Employees
    public async Task<IList<ViewEmp>> GetAllEmployeesAsync()
    {
        try
        {
            var employees = await _session.Query<Employee>()
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
            return employees;
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
            var employee = await _session.GetAsync<Employee>(empId);

            if (employee != null)
            {
                return new ViewEmp
                {
                    empId = employee.empId,
                    username = employee.username,
                    Role = employee.Role,
                    Department = employee.Department,
                    email = employee.email,
                    profilePic = employee.profilePic
                };
            }

            return null;
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
        using (var transaction = _session.BeginTransaction())
        {
            try
            {
                var employee = await _session.GetAsync<Employee>(viewEmp.empId);
                if (employee != null)
                {
                    // Update employee properties
                    employee.username = viewEmp.username;
                    employee.email = viewEmp.email;
                    employee.Role = viewEmp.Role;
                    employee.Department = viewEmp.Department;
                    if (employee.profilePic != null)
                    {
                        employee.profilePic = viewEmp.profilePic;
                    }

                    // Save the changes
                    await _session.UpdateAsync(employee);
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update employee");
                throw;
            }
        }
    }
    #endregion

    #region Delete Employee
    public async Task DeleteEmployeeAsync(int employeeId)
    {
        using (var transaction = _session.BeginTransaction())
        {
            try
            {
                // 1. Delete Leave records
                var leaves = await _session.Query<Leave>()
                    .Where(l => l.employee.empId == employeeId)
                    .ToListAsync();

                foreach (var leave in leaves)
                {
                    await _session.DeleteAsync(leave);
                }

                // 2. Delete LeaveBalance record
                var leaveBalance = await _session.Query<LeaveBalance>()
                    .FirstOrDefaultAsync(lb => lb.Employee.empId == employeeId);

                if (leaveBalance != null)
                {
                    await _session.DeleteAsync(leaveBalance);
                }

                // 3. Optional: Delete SurveyPredictionResults
                var surveyResults = await _session.Query<SurveyPredictionResults>()
                   .Where(sr => sr.Employee.empId == employeeId)
                    .ToListAsync();

                foreach (var result in surveyResults)
                {
                    await _session.DeleteAsync(result);
                }

                // 4. Optional: Delete Surveys
                var surveys = await _session.Query<Survey>()
                    .Where(s => s.Employee.empId == employeeId)
                    .ToListAsync();

                foreach (var survey in surveys)
                {
                    await _session.DeleteAsync(survey);
                }

                // 5. Delete Employee
                var employee = await _session.GetAsync<Employee>(employeeId);
                if (employee != null)
                {
                    await _session.DeleteAsync(employee);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete employee and related data");
                throw;
            }
        }
    }

    #endregion
    #region Get Filtered Employees
    public async Task<IList<ViewEmp>> GetFilteredEmployeesAsync(string empId)
    {
        try
        {
            var filteredEmployees = await _session.Query<Employee>()
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
            return filteredEmployees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering employees.");
            throw;
        }
    }
    #endregion
}


