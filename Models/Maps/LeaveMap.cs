using FluentNHibernate.Mapping;
using HrInternWebApp.Models.Identity;

namespace HrInternWebApp.Models.Map
{
    public class LeaveMap : ClassMap<Leave>
    {
        public LeaveMap()
        {
            Table("Leave");
            Id(x => x.LeaveId).Column("leaveId").GeneratedBy.Identity();
            Map(x => x.LeaveType).Column("leaveType").Not.Nullable();
            Map(x => x.StartDate).Column("startDate").Not.Nullable();
            Map(x => x.EndDate).Column("endDate").Not.Nullable();
            Map(x => x.Reason).Column("reason").Not.Nullable();
            Map(x => x.Status).Column("status").Nullable();
            Map(x => x.Approver).Column("approver").Nullable();

            References(x => x.Employee).Column( "empId").Not.Nullable(); // Foreign key to Employee
        }
    }
}
