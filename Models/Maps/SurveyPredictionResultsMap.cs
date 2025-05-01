using FluentNHibernate.Mapping;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Models.Maps
{
    public class SurveyPredictionResultsMap : ClassMap<SurveyPredictionResults>
    {
        public SurveyPredictionResultsMap()
        {
            Table("SurveyPredictionResults");

            Id(x => x.Id).GeneratedBy.Identity();

            References(x => x.Employee, "EmpId") // foreign key column is EmpId
                .Not.Nullable()
                .Cascade.None(); // don't cascade Employee changes from here

            References(x => x.Survey, "SurveyId") // 👈 foreign key to Survey
             .Not.Nullable()
             .Cascade.None();
            //References(x => x.Survey).Not.Nullable();  // Ensure Survey is not nullable
            //References(x => x.Employee).Not.Nullable();

            Map(x => x.PredictionModel1).Not.Nullable();
            Map(x => x.PredictionModel2).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable()
                .Default("GETDATE()"); // SQL default
        }
    }
}
