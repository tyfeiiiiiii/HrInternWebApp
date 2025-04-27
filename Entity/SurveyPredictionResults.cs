namespace HrInternWebApp.Entity
{
    public class SurveyPredictionResults
    {
        public virtual int Id { get; set; }
        public virtual Employee Employee { get; set; } // Many-to-One (each prediction linked to one employee)
        public virtual int PredictionModel1 { get; set; }
        public virtual int PredictionModel2 { get; set; }
        public virtual DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
