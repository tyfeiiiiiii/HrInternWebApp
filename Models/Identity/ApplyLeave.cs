using HrInternWebApp.Entity;
using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Models.Identity
{
    public class ApplyLeave
    {
        public int leaveId { get; set; }

        [Required(ErrorMessage = "Please fill in your leave type.")]
        public string leaveType { get; set; }

        [Required(ErrorMessage = "Please fill in the start date of your leave.")]
        [DataType(DataType.Date)]
        public DateTime? startDate { get; set; }

        [Required(ErrorMessage = "Please fill in the end date of your leave.")]
        [DataType(DataType.Date)]
        public DateTime? endDate { get; set; }

        [Required(ErrorMessage = "Please provide a reason for your leave.")]
        [StringLength(255)]
        public string reason { get; set; }
        public virtual Employee employee { get; set; }
    }
}
