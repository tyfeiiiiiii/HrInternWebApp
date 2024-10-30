using HrInternWebApp.Models.Identity;
using Microsoft.AspNetCore.Http;
using NHibernate;
using ISession = NHibernate.ISession;

public class LeaveService
{
    private readonly ISession _session;

    public LeaveService(ISession session)
    {
        _session = session;
    }

    // Fetch all leave applications by employee ID (for normal employees)
    public IList<ViewLeave> GetLeavesByEmployee(int employeeId)
    {
        return _session.Query<ViewLeave>()
                       .Where(l => l.Employee.empId == employeeId)
                       .ToList();
    }

    // Fetch all leave applications (for admin)
    public IList<ViewLeave> GetAllLeaves()
    {
        return _session.Query<ViewLeave>().ToList();
    }

    // Apply for a new Leave
    public void ApplyLeave(ApplyLeave leave, int employeeId)
    {
        leave.Employee = _session.Get<Employee>(employeeId);  // Associate employee
        using (ITransaction transaction = _session.BeginTransaction())
        {
            _session.Save(leave);
            transaction.Commit();
        }
    }

    // Update leave status (for admin)
    public void UpdateLeaveStatus(int leaveId, string newStatus, string approver)
    {
        var leave = _session.Get<ViewLeave>(leaveId);
        if (leave != null)
        {
            leave.Status = newStatus;
            leave.Approver = approver;

            using (ITransaction transaction = _session.BeginTransaction())
            {
                _session.Update(leave);
                transaction.Commit();
            }
        }
    }
}
