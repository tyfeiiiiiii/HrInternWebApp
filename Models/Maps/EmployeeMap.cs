using FluentNHibernate.Mapping;
using HrInternWebApp.Models.Identity;

namespace HrInternWebApp.Models.Map
{
    public class EmployeeMap : ClassMap<Employee>
    {
        public EmployeeMap()
        {
            Table("Employee");

            Id(x => x.EmpId);
            Map(x => x.Username).Column("username").Not.Nullable();
            Map(x => x.Password).Column("password").Not.Nullable();
            Map(x => x.Role).Column("Role").Not.Nullable();
            Map(x => x.Department).Column("department").Not.Nullable();

            HasMany(x => x.Leave)
                .KeyColumn("empId")   // Foreign key in the Leave table
                .Inverse()
                .Cascade.All();
        }
    }
}
