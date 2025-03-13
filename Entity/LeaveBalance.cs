namespace HrInternWebApp.Entity
{
    public class LeaveBalance
    {
        public virtual int LeaveBalanceId { get; set; } // Primary key
        public virtual Employee Employee { get; set; }  // Foreign key reference
        public virtual int MedicalLeave { get; set; }
        public virtual int AnnualLeave { get; set; }
        public virtual int Hospitalization { get; set; }
        public virtual int Examination { get; set; }
        public virtual int Marriage { get; set; }
        public virtual int PaternityLeave { get; set; }
        public virtual int MaternityLeave { get; set; }
        public virtual int ChildcareLeave { get; set; }
        public virtual int UnpaidLeave { get; set; }
        public virtual int EmergencyLeave { get; set; }
        public virtual DateTime LastUpdated { get; set; }
    }
}
