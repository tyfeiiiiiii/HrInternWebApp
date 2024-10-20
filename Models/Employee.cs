using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace HrInternWebApp.Models
{
    public class Employee
    {
        public virtual int empId { get; set; }
        public virtual string username { get; set; }
        public virtual string password { get; set; }
        public virtual IList<Leave> Leave { get; set; }  // One employee can have many leaves
    }
}
