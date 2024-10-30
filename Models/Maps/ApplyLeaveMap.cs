using FluentNHibernate.Mapping;
using HrInternWebApp.Models.Identity;

namespace HrInternWebApp.Models.Map
{
    public class ApplyLeaveMap : ClassMap<ApplyLeave>
    {
        public ApplyLeaveMap()
        {
            Table("Leave"); // Map to the Leave table in the database
            Id(x => x.LeaveId).Column("leaveId").GeneratedBy.Identity();

            // Map common properties from ApplyLeave
            Map(x => x.LeaveType).Column("leaveType").Not.Nullable();
            Map(x => x.StartDate).Column("startDate").Not.Nullable();
            Map(x => x.EndDate).Column("endDate").Not.Nullable();
            Map(x => x.Reason).Column("reason").Not.Nullable();

            // Foreign key reference to Employee
            References(x => x.Employee).Column("empId").Not.Nullable();

            // Discriminator column to identify if a row is ApplyLeave or ViewLeave
            DiscriminateSubClassesOnColumn("LeaveClassType").Not.Nullable();
        }
    }
}
