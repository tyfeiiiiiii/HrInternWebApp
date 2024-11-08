using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Identity
{
    public class ViewLeave
    {
        public virtual int leaveId { get; set; }
        public virtual int empId { get; set; }
        public virtual string username { get; set; }
        public virtual string leaveType { get; set; }
        public virtual DateTime? startDate { get; set; }
        public virtual DateTime? endDate { get; set; }
        public virtual string reason { get; set; }
        public virtual string status { get; set; }
        public virtual string approver { get; set; }
    }

}
