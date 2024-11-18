using HrInternWebApp.Entity;
using NHibernate;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrInternWebApp.Models.Identity;
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

    public async Task CreateEmployeeAsync(Employee employee)
    {
        using (var transaction = _session.BeginTransaction())
        {
            try
            {
                await _session.SaveAsync(employee);
                await transaction.CommitAsync();
                _logger.LogInformation($"Employee record successfully created for ID {employee.empId}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create employee");
                throw;
            }
        }
    }

    public async Task<IList<Employee>> GetAllEmployeesAsync()
    {
        try
        {
            return await _session.QueryOver<Employee>().ListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all employees");
            throw;
        }
    }

    public async Task<Employee> GetEmployeeByIdAsync(int employeeId)
    {
        try
        {
            return await _session.GetAsync<Employee>(employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with ID {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        using (var transaction = _session.BeginTransaction())
        {
            try
            {
                await _session.UpdateAsync(employee);
                await transaction.CommitAsync();
                _logger.LogInformation($"Employee record updated for ID {employee.empId}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update employee");
                throw;
            }
        }
    }

    public async Task DeleteEmployeeAsync(int employeeId)
    {
        using (var transaction = _session.BeginTransaction())
        {
            try
            {
                var employee = await _session.GetAsync<Employee>(employeeId);
                if (employee != null)
                {
                    // Add any additional cleanup of related data if necessary here

                    await _session.DeleteAsync(employee);
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Employee record deleted for ID {employeeId}");
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

}
