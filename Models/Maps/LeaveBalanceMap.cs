using FluentNHibernate.Mapping;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Maps
{
    public class LeaveBalanceMap : ClassMap<LeaveBalance>
    {
        public LeaveBalanceMap()
        {
            Table("LeaveBalance");

            Id(x => x.LeaveBalanceId).GeneratedBy.Identity(); // Primary key

            // One-to-One Relationship with Employee
            References(x => x.Employee)
                .Column("EmpId")
                .Unique() // Ensures one-to-one relationship
                .Not.Nullable()
                .Cascade.All();

            // Properties
            Map(x => x.MedicalLeave).Not.Nullable();
            Map(x => x.AnnualLeave).Not.Nullable();
            Map(x => x.Hospitalization).Not.Nullable();
            Map(x => x.Examination).Not.Nullable();
            Map(x => x.Marriage).Not.Nullable();
            Map(x => x.PaternityLeave).Not.Nullable();
            Map(x => x.MaternityLeave).Not.Nullable();
            Map(x => x.ChildcareLeave).Not.Nullable();
            Map(x => x.UnpaidLeave).Not.Nullable();
            Map(x => x.EmergencyLeave).Not.Nullable();
            Map(x => x.LastUpdated).Not.Nullable();
        }
    }
}
