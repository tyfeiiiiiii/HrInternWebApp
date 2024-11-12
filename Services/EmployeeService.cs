using NHibernate;
using HrInternWebApp.Entity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using ISession = NHibernate.ISession;

public class EmployeeService
{
    #region Fields
    private readonly ISession _session;
    private readonly ILogger<EmployeeService> _logger;
    #endregion

    #region Constructor
    public EmployeeService(ISession session, ILogger<EmployeeService> logger)
    {
        _session = session;
        _logger = logger;
    }
    #endregion

    #region Get All Employees
    public IList<Employee> GetAllEmployees()
    {
        try
        {
            var employees = _session.Query<Employee>().ToList();
            _logger.LogInformation($"Retrieved {employees.Count} employee(s) from the database.");
            return employees;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all employees.");
            throw;
        }
    }
    #endregion

    #region Get Employee By ID
    public Employee GetEmployeeById(int id)
    {
        try
        {
            var employee = _session.Get<Employee>(id);
            if (employee == null)
            {
                _logger.LogWarning($"Employee with ID {id} not found.");
            }
            return employee;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with ID {Id}", id);
            throw;
        }
    }
    #endregion

    #region Create Employee
    public void CreateEmployee(Employee employee)
    {
        try
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Save(employee);
                transaction.Commit();
                _logger.LogInformation($"Employee created successfully with ID {employee.empId}.");
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error creating employee.");
            throw;
        }
    }
    #endregion

    #region Update Employee
    public void UpdateEmployee(Employee employee)
    {
        try
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Update(employee);
                transaction.Commit();
                _logger.LogInformation($"Employee with ID {employee.empId} updated successfully.");
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error updating employee with ID {Id}", employee.empId);
            throw;
        }
    }
    #endregion

    #region Delete Employee
    public void DeleteEmployee(int id)
    {
        try
        {
            var employee = _session.Get<Employee>(id);
            if (employee != null)
            {
                using (var transaction = _session.BeginTransaction())
                {
                    _session.Delete(employee);
                    transaction.Commit();
                    _logger.LogInformation($"Employee with ID {id} deleted successfully.");
                }
            }
            else
            {
                _logger.LogWarning($"Employee with ID {id} not found for deletion.");
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee with ID {Id}", id);
            throw;
        }
    }
    #endregion
}
