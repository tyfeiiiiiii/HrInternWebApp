using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrInternWebApp.Entity 
{
    public class Leave
    {
        [Key]
        public int LeaveId { get; set; }

        [ForeignKey("Employee")]
        public int EmpId { get; set; }

        [ForeignKey("LeaveType")]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; } = "Pending";

        public string Approver { get; set; }

        // Navigation properties
        public virtual Employee Employee { get; set; }
        public virtual LeaveType LeaveType { get; set; }
    }

}
