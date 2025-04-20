using Microsoft.AspNetCore.Mvc;
using HrInternWebApp.Entity;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NHibernate;

public class SurveyController : Controller
{
    private readonly ISessionFactory _sessionFactory;

    public SurveyController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Submit(Survey model)
    {
        if (!ModelState.IsValid)
            return View(model);

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            model.SubmissionDate = DateTime.Now;
            model.Employee = session.Get<Employee>(model.Employee.empId);
            await session.SaveAsync(model);
            await transaction.CommitAsync();
        }

        // After saving the model, redirect to Report to show prediction
        return RedirectToAction("Report", new { id = model.SurveyId });
    }

    [HttpGet]
    public async Task<IActionResult> Report(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var survey = await session.GetAsync<Survey>(id);
            if (survey == null)
                return NotFound();

            var prediction = await GetPredictionAsync(survey);
            ViewBag.Prediction = prediction;
            return View(survey);
        }
    }

    private async Task<string> GetPredictionAsync(Survey survey)
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5000"); // Flask API URL
                var json = new
                {
                    SatisfactionLevel = survey.SatisfactionLevel,
                    LastEvaluation = survey.LastEvaluation,
                    NumberProject = survey.NumberProject,
                    AverageMonthlyHours = survey.AverageMonthlyHours,
                    TimeSpendCompany = survey.TimeSpendCompany,
                    WorkAccident = survey.WorkAccident ? 1 : 0,
                    PromotionLast5Years = survey.PromotionLast5Years ? 1 : 0,
                    Department = survey.Department,
                    Salary = survey.Salary
                };

                // Make a POST request to Flask server
                var response = await client.PostAsJsonAsync("/predict", json);

                // If the request is successful, read the prediction result
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result;  // Prediction result from Flask
                }
                else
                {
                    return "Failed to get prediction from Flask API.";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
