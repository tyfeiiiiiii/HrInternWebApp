using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Models
{
    public class Leave
    {
        public virtual int id { get; set; }
        public virtual string leaveType { get; set; }
        [Required]public virtual DateTime startDate { get; set; }
        [Required]public virtual DateTime endDate { get; set; }
        [Required]public virtual string reason { get; set; }
        public virtual string status {  get; set; }
        public virtual string approver {  get; set; }

        // Foreign key reference to Employee
        public virtual Employee Employee { get; set; }

    }
}
