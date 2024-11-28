using System.ComponentModel.DataAnnotations;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Identity
{
    public class ViewEmp
    {
        [Required]
        public virtual int empId { get; set; }
        [Required]
        public virtual string username { get; set; }
        [Required]
        public virtual string Role { get; set; }
        [Required]
        public virtual string Department { get; set; }
        [Required]
        public virtual string email { get; set; }
        public virtual byte[] profilePic { get; set; }
    }
}
