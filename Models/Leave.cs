using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Models
{
    public class Leave
    {
        public virtual int LeaveId { get; set; } 

        public virtual string LeaveType { get; set; }

        [Required(ErrorMessage = "Please fill in start data of your leave.")]
        [DataType(DataType.Date)]
        public virtual DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Please fill in end data of your leave.")]
        [DataType(DataType.Date)]
        public virtual DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Please fill in you reason.")]
        [StringLength(255)]
        public virtual string Reason { get; set; }

        public virtual string Status { get; set; }
        public virtual string Approver { get; set; }

        // Foreign key reference to Employee
        public virtual Employee Employee { get; set; }
    }
}
