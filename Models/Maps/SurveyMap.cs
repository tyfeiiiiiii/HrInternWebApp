//using FluentNHibernate.Mapping;
//using HrInternWebApp.Entity;

//namespace HrInternWebApp.Models.Maps
//{
//    public class SurveyMap : ClassMap<Survey>
//    {
//        public SurveyMap()
//        {
//            Table("Survey");
//            Id(x => x.SurveyId).GeneratedBy.Identity();

//            // FK Relationship to Employee table
//            References(x => x.Employee).Column("EmployeeId").Not.Nullable().Cascade.None();

//            Map(x => x.SatisfactionLevel).Not.Nullable();
//            Map(x => x.LastEvaluation).Not.Nullable();
//            Map(x => x.NumberProject).Not.Nullable();
//            Map(x => x.AverageMonthlyHours).Not.Nullable();
//            Map(x => x.TimeSpendCompany).Not.Nullable();
//            Map(x => x.WorkAccident).Not.Nullable();
//            Map(x => x.PromotionLast5Years).Not.Nullable();
//            Map(x => x.Department).Not.Nullable();
//            Map(x => x.Salary).Not.Nullable();
//            Map(x => x.SubmissionDate).Not.Nullable();
//        }
//    }
//}
