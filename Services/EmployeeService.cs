//using HrInternWebApp.Entity;
//using HrInternWebApp.Models.Identity;
//using NHibernate;
//using ISession = NHibernate.ISession;
//using Microsoft.Extensions.Logging;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using NHibernate.Linq;

//public class EmployeeService
//{
//    private readonly ISession _session;
//    private readonly ILogger<EmployeeService> _logger;

//    public EmployeeService(ISession session, ILogger<EmployeeService> logger)
//    {
//        _session = session;
//        _logger = logger;
//    }

//    #region Get All Employees
//    public async Task<IList<ViewEmp>> GetAllEmployeesAsync()
//    {
//        try
//        {
//            // Fetch all Employee entities from the database
//            var employees = await _session.Query<Employee>()
//                                          .Select(e => new ViewEmp
//                                          {
//                                              empId = e.empId,
//                                              username = e.username,
//                                              Role = e.Role,
//                                              Department = e.Department,
//                                              email = e.email,
//                                              profilePic = e.profilePic
//                                          })
//                                          .ToListAsync(); // Map the Employee entity to ViewEmp model
//            return employees;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error fetching employees.");
//            throw;
//        }
//    }
//    #endregion

//    #region Get Employee By ID
//    public async Task<ViewEmp> GetEmployeeByIdAsync(int empId)
//    {
//        try
//        {
//            var employee = await _session.GetAsync<Employee>(empId);

//            if (employee != null)
//            {
//                return new ViewEmp
//                {
//                    empId = employee.empId,
//                    username = employee.username,
//                    Role = employee.Role,
//                    Department = employee.Department,
//                    email = employee.email,
//                    profilePic = employee.profilePic
//                };
//            }
//            return null;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error retrieving employee with ID {EmployeeId}", empId);
//            throw;
//        }
//    }

//    #endregion

//    #region Get Filtered Employees
//    public async Task<IList<ViewEmp>> GetFilteredEmployeesAsync(string empId)
//    {
//        try
//        {
//            // Query and filter employees based on empId (use Employee entity for querying)
//            var filteredEmployees = await _session.Query<Employee>()
//                                                   .Where(e => e.empId.ToString().Contains(empId))
//                                                   .Select(e => new ViewEmp
//                                                   {
//                                                       empId = e.empId,
//                                                       username = e.username,
//                                                       Role = e.Role,
//                                                       Department = e.Department,
//                                                       email = e.email,
//                                                       profilePic = e.profilePic
//                                                   })
//                                                   .ToListAsync();
//            return filteredEmployees; // Return mapped ViewEmp models
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error filtering employees.");
//            throw;
//        }
//    }
//    #endregion

//    #region Update Employee
//    public async Task UpdateEmployeeAsync(Employee employee)
//    {
//        using (var transaction = _session.BeginTransaction())
//        {
//            try
//            {
//                await _session.UpdateAsync(employee);
//                await transaction.CommitAsync();
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                _logger.LogError(ex, "Failed to update employee");
//                throw;
//            }
//        }
//    }

//    #endregion

//    #region Delete Employee
//    public async Task DeleteEmployeeAsync(int employeeId)
//    {
//        using (var transaction = _session.BeginTransaction())
//        {
//            try
//            {
//                // Fetch the Employee entity by employeeId
//                var employee = await _session.GetAsync<Employee>(employeeId);
//                if (employee != null)
//                {
//                    // Delete the Employee entity from the database
//                    await _session.DeleteAsync(employee);
//                    await transaction.CommitAsync();
//                }
//                else
//                {
//                    _logger.LogWarning($"Employee with ID {employeeId} not found.");
//                }
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                _logger.LogError(ex, "Failed to delete employee");
//                throw;
//            }
//        }
//    }
//    #endregion
//}
// EmployeeService.cs

using HrInternWebApp.Entity;
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
                var employee = await _session.GetAsync<Employee>(employeeId);
                if (employee != null)
                {
                    await _session.DeleteAsync(employee);
                    await transaction.CommitAsync();
                }
                else
                {
                    _logger.LogWarning($"Employee with ID {employeeId} not found.");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete employee");
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
            // Query and filter employees based on empId (use Employee entity for querying)
            var filteredEmployees = await _session.Query<Employee>()
                                                   .Where(e => e.empId.ToString().Contains(empId))
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


