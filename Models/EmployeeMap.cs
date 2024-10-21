using FluentNHibernate.Mapping;

namespace HrInternWebApp.Models
{
    public class EmployeeMap : ClassMap<Employee>
    {
        public EmployeeMap()
        {
            Table("Employee");

            Id(emp => emp.EmpId);
            Map(emp => emp.Username);
            Map(emp => emp.Password);

            HasMany(emp => emp.Leave)
                .KeyColumn("empId")          // Define FK in the Leave table
                .Inverse();                 
        }
    }

}
