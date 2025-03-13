using FluentNHibernate.Mapping;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Maps
{
    public class EmployeeMap : ClassMap<Employee>
    {
        public EmployeeMap()
        {
            Table("Employee");

            Id(x => x.empId).GeneratedBy.Assigned();
            Map(x => x.username).Not.Nullable();
            Map(x => x.password).Nullable();
            Map(x => x.Role).Not.Nullable();
            Map(x => x.Department).Not.Nullable();
            Map(x => x.email).Not.Nullable();
            Map(x => x.profilePic).CustomType("BinaryBlob").Nullable();
            Map(x => x.Gender).Not.Nullable();   

        HasMany(x => x.Leave).KeyColumn("empId").Inverse().Cascade.All();

            // One-to-One Relationship with LeaveBalance
            HasOne(x => x.LeaveBalance)
         .ForeignKey("EmpId") // Ensures FK is mapped correctly
         .Cascade.All();
        }
    }
}
