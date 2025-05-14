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
                .Cascade.None(); 

            References(x => x.Survey, "SurveyId") 
             .Not.Nullable()
             .Cascade.None();

            Map(x => x.PredictionModel1).Not.Nullable();
            Map(x => x.PredictionModel2).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable()
                .Default("GETDATE()"); // SQL default
        }
    }
}
