using FluentNHibernate.Mapping;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Maps
{
    public class LeaveBalanceMap : ClassMap<LeaveBalance>
    {
        public LeaveBalanceMap()
        {
            Table("LeaveBalance");

            Id(x => x.LeaveBalanceId)
                .GeneratedBy.Identity(); // Auto-increment primary key

            // One-to-One Relationship with Employee (Foreign Key)
            References(x => x.Employee)
                .Column("EmpId") // Foreign key column
                .Not.Nullable()
                .Cascade.None(); // No cascading to avoid unintended deletes

            // Leave Balances
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

            // Used Leave Tracking
            Map(x => x.MedicalLeaveUsed).Not.Nullable().Default("0");
            Map(x => x.AnnualLeaveUsed).Not.Nullable().Default("0");
            Map(x => x.HospitalizationUsed).Not.Nullable().Default("0");
            Map(x => x.ExaminationUsed).Not.Nullable().Default("0");
            Map(x => x.MarriageUsed).Not.Nullable().Default("0");
            Map(x => x.PaternityLeaveUsed).Not.Nullable().Default("0");
            Map(x => x.MaternityLeaveUsed).Not.Nullable().Default("0");
            Map(x => x.ChildcareLeaveUsed).Not.Nullable().Default("0");
            Map(x => x.UnpaidLeaveUsed).Not.Nullable().Default("0");
            Map(x => x.EmergencyLeaveUsed).Not.Nullable().Default("0");

            Map(x => x.LastUpdated)
                .Not.Nullable()
                .Default("CURRENT_TIMESTAMP"); // Auto-updating timestamp
        }
    }
}
