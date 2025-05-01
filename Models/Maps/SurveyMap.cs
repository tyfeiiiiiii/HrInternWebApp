using FluentNHibernate.Mapping;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Maps
{
    public class SurveyMap : ClassMap<Survey>
    {
        public SurveyMap()
        {
            Table("Survey");  // Make sure the table name matches in your database

            Id(x => x.SurveyId).GeneratedBy.Identity(); // SurveyId is the primary key

            References(x => x.Employee).Column("empId").Not.Nullable(); // Foreign key reference to Employee

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
            Map(x => x.AverageMonthlyHours);
            Map(x => x.WorkAccident);
            Map(x => x.PromotionLast5Years);
            Map(x => x.Department);
            Map(x => x.SubmissionDate);
        }
    }
}