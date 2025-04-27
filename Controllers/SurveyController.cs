//using Microsoft.AspNetCore.Mvc;
//using HrInternWebApp.Entity;
//using System;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using NHibernate;

//public class SurveyController : Controller
//{
//    private readonly ISessionFactory _sessionFactory;

//    public SurveyController(ISessionFactory sessionFactory)
//    {
//        _sessionFactory = sessionFactory;
//    }

//    [HttpGet]
//    public IActionResult Index()
//    {
//        return View();
//    }

//    [HttpPost]
//    public async Task<IActionResult> Submit(Survey model)
//    {
//        if (!ModelState.IsValid)
//            return View(model);

//        using (var session = _sessionFactory.OpenSession())
//        using (var transaction = session.BeginTransaction())
//        {
//            model.SubmissionDate = DateTime.Now;
//            model.Employee = session.Get<Employee>(model.Employee.empId);
//            await session.SaveAsync(model);
//            await transaction.CommitAsync();
//        }

//        // After saving the model, redirect to Report to show prediction
//        return RedirectToAction("Report", new { id = model.SurveyId });
//    }

//    [HttpGet]
//    public async Task<IActionResult> Report(int id)
//    {
//        using (var session = _sessionFactory.OpenSession())
//        {
//            var survey = await session.GetAsync<Survey>(id);
//            if (survey == null)
//                return NotFound();

//            var prediction = await GetPredictionAsync(survey);
//            ViewBag.Prediction = prediction;
//            return View(survey);
//        }
//    }

//    private async Task<string> GetPredictionAsync(Survey survey)
//    {
//        try
//        {
//            using (var client = new HttpClient())
//            {
//                client.BaseAddress = new Uri("http://localhost:5000"); // Flask API URL
//                var json = new
//                {
//                    // Set 1: Features (columns1)
//                    OverTime = survey.OverTime,
//                    MaritalStatus = survey.MaritalStatus,
//                    MonthlyIncome = survey.MonthlyIncome,
//                    StockOptionLevel = survey.StockOptionLevel,
//                    BusinessTravel = survey.BusinessTravel,
//                    TotalWorkingYears = survey.TotalWorkingYears,
//                    JobInvolvement = survey.JobInvolvement,
//                    YearsAtCompany = survey.YearsAtCompany,
//                    Age = survey.Age,
//                    DistanceFromHome = survey.DistanceFromHome,

//                    // Set 2: Features (columns2)
//                    SatisfactionLevel = survey.SatisfactionLevel,
//                    LastEvaluation = survey.LastEvaluation,
//                    NumberProject = survey.NumberProject,
//                    AverageMonthlyHours = survey.AverageMonthlyHours,
//                    TimeSpendCompany = survey.TimeSpendCompany,
//                    WorkAccident = survey.WorkAccident ? 1 : 0,  // Convert boolean to int (1 or 0)
//                    PromotionLast5Years = survey.PromotionLast5Years ? 1 : 0,  // Convert boolean to int (1 or 0)
//                    Department = survey.Department,
//                    Salary = survey.Salary,
//                };

//                // Make a POST request to Flask server
//                var response = await client.PostAsJsonAsync("/predict", json);

//                // If the request is successful, read the prediction result
//                if (response.IsSuccessStatusCode)
//                {
//                    var result = await response.Content.ReadAsStringAsync();
//                    return result;  // Prediction result from Flask
//                }
//                else
//                {
//                    return "Failed to get prediction from Flask API.";
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            return $"Error: {ex.Message}";
//        }
//    }
//}
//using Microsoft.AspNetCore.Mvc;
//using HrInternWebApp.Entity;
//using System;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using NHibernate;
//using NHibernate.Linq;

//public class SurveyController : Controller
//{
//    private readonly ISessionFactory _sessionFactory;

//    public SurveyController(ISessionFactory sessionFactory)
//    {
//        _sessionFactory = sessionFactory;
//    }

//    [HttpGet]
//    public IActionResult Index()
//    {
//        return View();
//    }

//    [HttpPost]
//    public async Task<IActionResult> Submit(Survey model)
//    {
//        if (!ModelState.IsValid)
//        {
//            foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
//            {
//                Console.WriteLine(modelError.ErrorMessage);  // You can log these or inspect them in Debug mode
//            }
//            return View("Index", model);   // If model is invalid, return to the form
//        }

//        using (var session = _sessionFactory.OpenSession())
//        using (var transaction = session.BeginTransaction())
//        {
//            // Save the survey data
//            model.SubmissionDate = DateTime.Now;
//            model.Employee = session.Get<Employee>(model.Employee.empId);
//            await session.SaveAsync(model);

//            // Get prediction results from Flask API
//            var prediction = await GetPredictionAsync(model);

//            // Save prediction results to the database
//            var predictionResults = new SurveyPredictionResults
//            {
//                Employee = model.Employee,
//                PredictionModel1 = prediction.Model1,
//                PredictionModel2 = prediction.Model2
//            };

//            // Save the prediction results
//            await session.SaveAsync(predictionResults);

//            // Commit the transaction to save both survey and prediction results
//            await transaction.CommitAsync();
//        }

//        // After saving both the survey and prediction, **redirect** to the PredictionResult view
//        return RedirectToAction("DisplayPredictionResult", new { id = model.SurveyId });
//    }


//    [HttpGet]
//    public async Task<IActionResult> DisplayPredictionResult(int id)
//    {
//        using (var session = _sessionFactory.OpenSession())
//        {
//            // Get the survey data by ID
//            var survey = await session.GetAsync<Survey>(id);
//            if (survey == null)
//                return NotFound();  // If survey not found, return 404

//            // Get prediction results from the database for the employee
//            var predictionResults = await session.Query<SurveyPredictionResults>()
//                                                 .Where(x => x.Employee.empId == survey.Employee.empId)
//                                                 .FirstOrDefaultAsync();

//            if (predictionResults != null)
//            {
//                // Pass prediction results to ViewBag
//                ViewBag.PredictionModel1 = predictionResults.PredictionModel1;
//                ViewBag.PredictionModel2 = predictionResults.PredictionModel2;
//            }

//            // Return the PredictionResult view with survey data
//            return View("PredictionResult", survey);
//        }
//    }

//    private async Task<PredictionResult> GetPredictionAsync(Survey survey)
//    {
//        try
//        {
//            using (var client = new HttpClient())
//            {
//                client.BaseAddress = new Uri("http://localhost:5000"); // Flask API URL

//                var json = new
//                {
//                    OverTime = survey.OverTime,
//                    MaritalStatus = survey.MaritalStatus,
//                    MonthlyIncome = survey.MonthlyIncome,
//                    StockOptionLevel = survey.StockOptionLevel,
//                    BusinessTravel = survey.BusinessTravel,
//                    TotalWorkingYears = survey.TotalWorkingYears,
//                    JobInvolvement = survey.JobInvolvement,
//                    YearsAtCompany = survey.YearsAtCompany,
//                    Age = survey.Age,
//                    DistanceFromHome = survey.DistanceFromHome,
//                    SatisfactionLevel = survey.SatisfactionLevel,
//                    LastEvaluation = survey.LastEvaluation,
//                    NumberProject = survey.NumberProject,
//                    AverageMonthlyHours = survey.AverageMonthlyHours,
//                    TimeSpendCompany = survey.TimeSpendCompany,
//                    WorkAccident = survey.WorkAccident ? 1 : 0,
//                    PromotionLast5Years = survey.PromotionLast5Years ? 1 : 0,
//                    Department = survey.Department,
//                    Salary = survey.Salary,
//                };

//                var response = await client.PostAsJsonAsync("/predict", json);

//                if (response.IsSuccessStatusCode)
//                {
//                    var result = await response.Content.ReadFromJsonAsync<PredictionResult>();
//                    return result;
//                }
//                else
//                {
//                    var errorContent = await response.Content.ReadAsStringAsync();
//                    Console.WriteLine($"Error: {response.StatusCode}, {errorContent}");
//                    return new PredictionResult { Model1 = 0, Model2 = 0 }; // Default on failure
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Exception: {ex.Message}");
//            return new PredictionResult { Model1 = 0, Model2 = 0 }; // Default on error
//        }
//    }

//    public class PredictionResult
//    {
//        public int Model1 { get; set; }
//        public int Model2 { get; set; }
//    }
//}
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
                PredictionModel1 = prediction.Model1,
                PredictionModel2 = prediction.Model2
            };

            // Save the prediction results
            await session.SaveAsync(predictionResults);

            // Commit the transaction to save both survey and prediction results
            await transaction.CommitAsync();
        }

        // After saving both the survey and prediction, **redirect** to the PredictionResult view
        return RedirectToAction("DisplayPredictionResult", new { id = model.SurveyId });
    }


    [HttpGet]
    public async Task<IActionResult> DisplayPredictionResult(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            // Get the survey data by ID
            var survey = await session.GetAsync<Survey>(id);
            if (survey == null)
                return NotFound();  // If survey not found, return 404

            // Get prediction results from the database for the employee
            var predictionResults = await session.Query<SurveyPredictionResults>()
                                                 .Where(x => x.Employee.empId == survey.Employee.empId)
                                                 .FirstOrDefaultAsync();

            if (predictionResults != null)
            {
                // Pass prediction results to ViewBag
                ViewBag.PredictionModel1 = predictionResults.PredictionModel1;
                ViewBag.PredictionModel2 = predictionResults.PredictionModel2;
            }

            // Return the PredictionResult view with survey data
            return View("PredictionResult", survey);
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
                    NumberProject = survey.NumberProject,
                    AverageMonthlyHours = survey.AverageMonthlyHours,
                    TimeSpendCompany = survey.TimeSpendCompany,
                    WorkAccident = survey.WorkAccident ? 1 : 0,
                    PromotionLast5Years = survey.PromotionLast5Years ? 1 : 0,
                    Department = survey.Department,
                    Salary = survey.Salary,
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
}
