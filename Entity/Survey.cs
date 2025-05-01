namespace HrInternWebApp.Entity
{
    public class Survey
    {
        public virtual int SurveyId { get; set; } // Primary Key
        public virtual Employee Employee { get; set; } // FK reference to Employee

        // Features from columns1 (Set 1)
        public virtual string OverTime { get; set; }  // Categorical: Yes/No
        public virtual string MaritalStatus { get; set; }  // Categorical: Married/Single
        public virtual double MonthlyIncome { get; set; }  // Monetary value (float)
        public virtual int StockOptionLevel { get; set; }  // Integer representing stock options
        public virtual string BusinessTravel { get; set; }  // Categorical: 'Travel_Rarely', 'Travel_Frequently'
        public virtual int TotalWorkingYears { get; set; }  // Integer (years)
        public virtual int JobInvolvement { get; set; }  // Integer representing job involvement
        public virtual int YearsAtCompany { get; set; }  // Integer (years)
        public virtual int Age { get; set; }  // Integer (age)
        public virtual int DistanceFromHome { get; set; }  // Integer (distance)

        // Features from columns2 (Set 2)
        public virtual double SatisfactionLevel { get; set; }  // Double
        public virtual double LastEvaluation { get; set; }  // Double
        public virtual int AverageMonthlyHours { get; set; }  // Integer
        public virtual bool WorkAccident { get; set; }  // Boolean
        public virtual bool PromotionLast5Years { get; set; }  // Boolean
        public virtual string Department { get; set; }  // String (department name)

        public virtual DateTime SubmissionDate { get; set; } = DateTime.Now;  // Automatically set current time for submission
    }
}
