using Microsoft.AspNetCore.Mvc;
using HrInternWebApp.Entity;

public class EmployeeController : Controller
{
    private readonly EmployeeService _employeeService;

    #region Constructor
    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }
    #endregion

    #region Employee Home
    public IActionResult EmpHome()
    {
        return View("EmpHome"); 
    }
    #endregion

    #region Create Employee
    // GET: Show create form
    public IActionResult Create()
    {
        return View("CreateEmp"); 
    }

    // POST: Create a new employee
    [HttpPost]
    public IActionResult Create(Employee employee)
    {
        if (ModelState.IsValid)
        {
            _employeeService.CreateEmployee(employee);
            return RedirectToAction(nameof(View)); // Redirect to View after creation
        }
        return View("CreateEmp", employee); // Return CreateEmp.cshtml with model state errors
    }
    #endregion

    #region View Employees (Index)
    public IActionResult View() 
    {
        var employees = _employeeService.GetAllEmployees();
        return View("ViewEmp", employees); // Return ViewEmp.cshtml with employees list
    }
    #endregion

    #region Edit Employee
    // GET: Show edit form
    public IActionResult Update(int id) // Matches UpdateEmp.cshtml
    {
        var employee = _employeeService.GetEmployeeById(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View("UpdateEmp", employee); // Return UpdateEmp.cshtml
    }

    // POST: Save edited employee
    [HttpPost]
    public IActionResult Update(Employee employee)
    {
        if (ModelState.IsValid)
        {
            _employeeService.UpdateEmployee(employee);
            return RedirectToAction(nameof(View)); // Redirect to ViewEmp after update
        }
        return View("UpdateEmp", employee); // Return UpdateEmp.cshtml with model state errors
    }
    #endregion

    #region Delete Employee
    // GET: Show delete confirmation
    public IActionResult Delete(int id)
    {
        var employee = _employeeService.GetEmployeeById(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View("DeleteEmp", employee); // Return DeleteEmp.cshtml
    }

    // POST: Confirm deletion
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        _employeeService.DeleteEmployee(id);
        return RedirectToAction(nameof(View)); // Redirect to ViewEmp after deletion
    }
    #endregion
}
