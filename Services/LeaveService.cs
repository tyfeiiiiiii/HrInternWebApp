using NHibernate;
using HrInternWebApp.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HrInternWebApp.Services
{
    public class LeaveService
    {
        private readonly NHibernate.ISession _session;

        public LeaveService(NHibernate.ISession session)
        {
            _session = session;
        }

        // Fetch all Leave applications by Employee ID
        public IList<Leave> GetLeavesByEmployee(int employeeId)
        {
            return _session.Query<Leave>()
                           .Where(l => l.Employee.EmpId == employeeId)
                           .ToList();
        }

        // Apply for a new Leave
        public void ApplyLeave(Leave leave, int employeeId)
        {
            leave.Employee = _session.Get<Employee>(employeeId); // Link Employee to Leave
            using (ITransaction transaction = _session.BeginTransaction())
            {
                _session.SaveOrUpdate(leave);
                transaction.Commit();
            }
        }

        // Fetch all leaves (for admin or reporting purposes)
        public IList<Leave> GetAllLeaves()
        {
            return _session.Query<Leave>().ToList();
        }

        // Update Leave status (e.g., Approve or Reject)
        public void UpdateLeaveStatus(int leaveId, string newStatus, string approver)
        {
            var leave = _session.Get<Leave>(leaveId);
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

        // Get Leave by ID
        public Leave GetLeaveById(int leaveId)
        {
            return _session.Get<Leave>(leaveId);
        }

        // Delete Leave (optional functionality)
        public void DeleteLeave(int leaveId)
        {
            var leave = _session.Get<Leave>(leaveId);
            if (leave != null)
            {
                using (ITransaction transaction = _session.BeginTransaction())
                {
                    _session.Delete(leave);
                    transaction.Commit();
                }
            }
        }
    }
}
