namespace HrInternWebApp.Entity
{
    public class Survey
    {
        public virtual int SurveyId { get; set; } // Primary Key
        public virtual Employee Employee { get; set; } // FK reference to Employee
        public virtual double SatisfactionLevel { get; set; }
        public virtual double LastEvaluation { get; set; }
        public virtual int NumberProject { get; set; }
        public virtual int AverageMonthlyHours { get; set; }
        public virtual int TimeSpendCompany { get; set; }
        public virtual bool WorkAccident { get; set; }
        public virtual bool PromotionLast5Years { get; set; }
        public virtual string Department { get; set; }
        public virtual string Salary { get; set; }
        public virtual DateTime SubmissionDate { get; set; } = DateTime.Now;
    }
}
