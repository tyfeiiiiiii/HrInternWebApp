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
            Map(x => x.password).Not.Nullable();
            Map(x => x.Role).Not.Nullable();
            Map(x => x.Department).Not.Nullable();

            HasMany(x => x.Leave).KeyColumn("empId").Inverse().Cascade.All();
        }
    }
}
