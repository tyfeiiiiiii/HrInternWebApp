using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Models.Identity
{
    public class ApplyLeave
    {
        public virtual int LeaveId { get; set; }


        [Required(ErrorMessage = "Please fill in you leave type.")]
        public virtual string LeaveType { get; set; }

        [Required(ErrorMessage = "Please fill in start data of your leave.")]
        [DataType(DataType.Date)]
        public virtual DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Please fill in end data of your leave.")]
        [DataType(DataType.Date)]
        public virtual DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Please fill in you reason.")]
        [StringLength(255)]
        public virtual string Reason { get; set; }

        // Foreign key reference to Employee
        public virtual Employee Employee { get; set; }

    }
    public class ViewLeave : ApplyLeave
    {
        public virtual string Status { get; set; }
        public virtual string Approver { get; set; }
    }

}
