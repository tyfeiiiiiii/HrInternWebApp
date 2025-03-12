
using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Entity
{
    public class Employee
    {
        [Key]
        public int EmpId { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, MaxLength(50)]
        public string Password { get; set; }

        public string Role { get; set; }
        public string Department { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public byte[]? ProfilePic { get; set; }

        public string Gender { get; set; }

        // Relationships
        public virtual ICollection<Leave> Leaves { get; set; }
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; }
    }

}

