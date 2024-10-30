using HrInternWebApp.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Models.Identity 
{
    public class Employee
    {
        public virtual int empId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public virtual string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public virtual string Password { get; set; }

        public virtual string Role { get; set; }

        public virtual string Department { get; set; }

        public virtual IList<ApplyLeave> Leave { get; set; } = new List<ApplyLeave>();
    }
}

