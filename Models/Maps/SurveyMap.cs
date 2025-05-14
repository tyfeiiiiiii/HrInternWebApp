
using FluentNHibernate.Mapping;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Maps
{
    public class SurveyMap : ClassMap<Survey>
    {
        public SurveyMap()
        {
            Table("Survey");  // Ensure this matches your DB table name

            Id(x => x.SurveyId).GeneratedBy.Identity(); // Primary key

            References(x => x.Employee).Column("empId").Not.Nullable(); // FK to Employee

            Map(x => x.OverTime);
            Map(x => x.MaritalStatus);
            Map(x => x.MonthlyIncome);
            Map(x => x.StockOptionLevel);
            Map(x => x.BusinessTravel);
            Map(x => x.TotalWorkingYears);
            Map(x => x.JobInvolvement);
            Map(x => x.YearsAtCompany);
            Map(x => x.Age);
            Map(x => x.DistanceFromHome);

            Map(x => x.SatisfactionLevel);
            Map(x => x.LastEvaluation);
            Map(x => x.NumberProject);
            Map(x => x.AverageMonthlyHours);
            Map(x => x.WorkAccident);
            Map(x => x.PromotionLast5Years);
            Map(x => x.Department);
            Map(x => x.Salary);              
            Map(x => x.SubmissionDate);
        }
    }
}
