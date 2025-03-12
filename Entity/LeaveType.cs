using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Entity
{
    public class LeaveType
    {
        [Key]
        public int LeaveTypeId { get; set; }

        [Required, MaxLength(50)]
        public string LeaveTypeName { get; set; }

        // Relationships
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; }
        public virtual ICollection<Leave> Leaves { get; set; }
    }

}
