using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;

namespace HrInternWebApp.Models
{
    public class EmployeeMap : ClassMap<Employee>
    {
        public EmployeeMap()
        {
            Table("Employee");

            Id(emp => emp.empId);
            Map(emp => emp.username);
            Map(emp => emp.password);

            HasMany(emp => emp.Leave)
                .KeyColumn("empId")          // Define FK in the Leave table
                .Inverse();                 
        }
    }

}
