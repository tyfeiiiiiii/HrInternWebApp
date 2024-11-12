using HrInternWebApp.Entity;
using System.ComponentModel.DataAnnotations;

public class Leave
{
    public virtual int leaveId { get; set; }
    public virtual string leaveType { get; set; }
    public virtual DateTime? startDate { get; set; }
    public virtual DateTime? endDate { get; set; }
    public virtual string reason { get; set; }
    public virtual string status { get; set; }
    public virtual string approver { get; set; }
    public virtual int empId { get; set; }
    public virtual Employee employee { get; set; } // Reference to Employee entity
}
