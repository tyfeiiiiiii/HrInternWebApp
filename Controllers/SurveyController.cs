using Microsoft.AspNetCore.Mvc;
using NHibernate;
using HrInternWebApp.Entity;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NHibernate.Linq;

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
        // Retrieve the empId from the session
        var empIdStr = HttpContext.Session.GetString("EmployeeId");

        if (string.IsNullOrEmpty(empIdStr))
        {
            // If empId is not found in the session, return an error or redirect to login
            TempData["ErrorMessage"] = "You must be logged in to submit a survey.";
            return RedirectToAction("Login", "Authentication");  // Redirect to login page if not logged in
        }

        // Convert empId from string to int
        if (!int.TryParse(empIdStr, out int empId))
        {
            // If the empId is not a valid integer, handle the error
            TempData["ErrorMessage"] = "Invalid employee ID.";
            return RedirectToAction("Login", "Authentication");
        }

        // Get the employee using the empId from session
        var employee = await _sessionFactory.OpenSession().GetAsync<Employee>(empId);
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction("Login", "Authentication");
        }

        model.Employee = employee;
        model.SubmissionDate = DateTime.Now;

        if (!ModelState.IsValid)
        {
            return View("Index", model); // Return to the form if validation fails
        }

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            // Save the survey data
            await session.SaveAsync(model);

            // Get prediction results from Flask API
            var prediction = await GetPredictionAsync(model);

            // Save prediction results to the database
            var predictionResults = new SurveyPredictionResults
            {
                Employee = model.Employee,
                Survey = model,
                PredictionModel1 = prediction.Model1,
                PredictionModel2 = prediction.Model2,
                CreatedAt = DateTime.Now
            };

            // Save the prediction results
            await session.SaveAsync(predictionResults);

            // Commit the transaction to save both survey and prediction results
            await transaction.CommitAsync();
        }


        return RedirectToAction("Index", "Home");

    }

    [HttpGet]
    public async Task<IActionResult> DisplayPredictionResult()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var allPredictions = await session.Query<SurveyPredictionResults>()
                                              .Fetch(x => x.Employee)
                                              .ToListAsync();

            var latestPredictions = allPredictions
                .GroupBy(r => r.Employee.empId)
                .Select(g => g.OrderByDescending(r => r.CreatedAt).FirstOrDefault())
                .ToList();

            return View("PredictionResult", latestPredictions); // Use your actual view name here
        }
    }


    private async Task<PredictionResult> GetPredictionAsync(Survey survey)
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5000"); // Flask API URL

                var json = new
                {
                    OverTime = survey.OverTime,
                    MaritalStatus = survey.MaritalStatus,
                    MonthlyIncome = survey.MonthlyIncome,
                    StockOptionLevel = survey.StockOptionLevel,
                    BusinessTravel = survey.BusinessTravel,
                    TotalWorkingYears = survey.TotalWorkingYears,
                    JobInvolvement = survey.JobInvolvement,
                    YearsAtCompany = survey.YearsAtCompany,
                    Age = survey.Age,
                    DistanceFromHome = survey.DistanceFromHome,
                    SatisfactionLevel = survey.SatisfactionLevel,
                    LastEvaluation = survey.LastEvaluation,
                    AverageMonthlyHours = survey.AverageMonthlyHours,
                    WorkAccident = survey.WorkAccident ? 1 : 0,
                    PromotionLast5Years = survey.PromotionLast5Years ? 1 : 0,
                    Department = survey.Department,
                };

                var response = await client.PostAsJsonAsync("/predict", json);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PredictionResult>();
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {response.StatusCode}, {errorContent}");
                    return new PredictionResult { Model1 = 0, Model2 = 0 }; // Default on failure
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return new PredictionResult { Model1 = 0, Model2 = 0 }; // Default on error
        }
    }

    public class PredictionResult
    {
        public int Model1 { get; set; }
        public int Model2 { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> PredictionByEmployee(int empId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var predictions = await session.Query<SurveyPredictionResults>()
                                           .Where(x => x.Employee.empId == empId)
                                           .Fetch(x => x.Employee)
                                           .Fetch(x => x.Survey)
                                           .ToListAsync();

            var employeeName = predictions.FirstOrDefault()?.Employee?.username ?? "Unknown";

            ViewBag.EmpId = empId;
            ViewBag.Username = employeeName;

            return View("PredictionByEmp", predictions);
        }
    }
    public async Task<IActionResult> SearchPredictionByEmpId(string empId)
    {
        try
        {
            using (var session = _sessionFactory.OpenSession())
            {
                // Fetch SurveyPredictionResults with Employee eager-loaded
                var predictionsQuery = session.Query<SurveyPredictionResults>()
                                              .Fetch(x => x.Employee);

                // Execute query and get list of results
                var predictionsList = await predictionsQuery.ToListAsync();

                // Filter by empId if provided
                if (!string.IsNullOrEmpty(empId))
                {
                    predictionsList = predictionsList
                        .Where(x => x.Employee.empId.ToString().Contains(empId))
                        .ToList();
                }

                // Project only required fields
                var result = predictionsList.Select(p => new
                {
                    PredictionModel1 = p.PredictionModel1,
                    PredictionModel2 = p.PredictionModel2,
                    CreatedAt = p.CreatedAt,
                    Employee = new
                    {
                        empId = p.Employee.empId,
                        username = p.Employee.username
                    }
                });

                return Json(result);
            }
        }
        catch (Exception ex)
        {
            // Catch and return error message
            return Json(new { error = "An error occurred while retrieving data. Please try again." });
        }
    }



}
