using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Entity
{
    public class LeaveBalance
    {
        public int EmpId { get; set; }
        public int LeaveTypeId { get; set; }


        public int Balance { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Employee Employee { get; set; }
        public virtual LeaveType LeaveType { get; set; }
    }

}
