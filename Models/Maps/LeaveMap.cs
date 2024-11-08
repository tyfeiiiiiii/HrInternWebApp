using FluentNHibernate.Mapping;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Maps
{
    public class LeaveMap : ClassMap<Leave>
    {
        public LeaveMap()
        {
            Table("Leave"); // Map to the Leave table in the database
            Id(x => x.leaveId).Column("leaveId").GeneratedBy.Identity();
            Map(x => x.leaveType).Not.Nullable();
            Map(x => x.startDate).Not.Nullable();
            Map(x => x.endDate).Not.Nullable();
            Map(x => x.reason).Not.Nullable();
            Map(x => x.status).Nullable();
            Map(x => x.approver).Nullable();

            //reference to Employee
            References(x => x.employee).Column("empId").Not.Nullable();
        }
    }
}