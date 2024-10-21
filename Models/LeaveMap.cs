using FluentNHibernate.Mapping;

namespace HrInternWebApp.Models
{
    public class LeaveMap : ClassMap<Leave>
    {
        public LeaveMap()
        {
            Table("Leave");
            Id(leave => leave.LeaveId).Column("leaveId");
            Map(leave => leave.LeaveType).Column("leaveType");
            Map(leave => leave.StartDate).Column("startDate").Not.Nullable();
            Map(leave => leave.EndDate).Column("endDate").Not.Nullable();
            Map(leave => leave.Reason).Column("reason").Not.Nullable();
            Map(leave => leave.Status).Column("status");
            Map(leave => leave.Approver).Column("approver");

            References(leave => leave.Employee, "empId"); // Define FK in Leave table
        }
    }
}
