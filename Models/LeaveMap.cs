using FluentNHibernate.Mapping; 

namespace HrInternWebApp.Models
{
    public class LeaveMap:ClassMap<Leave>
    {
        public LeaveMap()
        {
            Table("Leave");
            Id(leave => leave.id);
            Map(leave => leave.leaveType);
            Map(leave => leave.startDate);
            Map(leave => leave.endDate);
            Map(leave => leave.reason);
            Map(leave => leave.status);
            Map(leave => leave.approver);

            References(leave => leave.Employee, "empId"); // Define FK in Leave table
        }
    }
}
