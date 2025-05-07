using Microsoft.AspNetCore.Mvc;
using NHibernate;
using HrInternWebApp.Entity;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NHibernate.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

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
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine($"Error: {error.ErrorMessage}");
            }

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
            await transaction.CommitAsync();

            Console.WriteLine($"Model1: {prediction.Model1}, Model2: {prediction.Model2}");
            return RedirectToAction("Index", "Home");

        }
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


    //private async Task<PredictionResult> GetPredictionAsync(Survey survey)
    //{
    //    try
    //    {
    //        using (var client = new HttpClient())
    //        {
    //            client.BaseAddress = new Uri("http://localhost:5000"); // Flask API URL

    //            //var json = new
    //            //{
    //            //    OverTime = survey.OverTime,
    //            //    MaritalStatus = survey.MaritalStatus,
    //            //    MonthlyIncome = survey.MonthlyIncome,
    //            //    StockOptionLevel = survey.StockOptionLevel,
    //            //    BusinessTravel = survey.BusinessTravel,
    //            //    TotalWorkingYears = survey.TotalWorkingYears,
    //            //    JobInvolvement = survey.JobInvolvement,
    //            //    YearsAtCompany = survey.YearsAtCompany,
    //            //    Age = survey.Age,
    //            //    DistanceFromHome = survey.DistanceFromHome,
    //            //    SatisfactionLevel = survey.SatisfactionLevel,
    //            //    LastEvaluation = survey.LastEvaluation,
    //            //    AverageMonthlyHours = survey.AverageMonthlyHours,
    //            //    WorkAccident = survey.WorkAccident ? 1 : 0,
    //            //    PromotionLast5Years = survey.PromotionLast5Years ? 1 : 0,
    //            //    Department = survey.Department,
    //            //};
    //            var json = new
    //            {
    //                OverTime = survey.OverTime,
    //                MaritalStatus = survey.MaritalStatus,
    //                MonthlyIncome = survey.MonthlyIncome,
    //                StockOptionLevel = survey.StockOptionLevel,
    //                BusinessTravel = survey.BusinessTravel,
    //                TotalWorkingYears = survey.TotalWorkingYears,
    //                JobInvolvement = survey.JobInvolvement,
    //                YearsAtCompany = survey.YearsAtCompany,
    //                Age = survey.Age,
    //                DistanceFromHome = survey.DistanceFromHome,
    //                SatisfactionLevel = survey.SatisfactionLevel,
    //                LastEvaluation = survey.LastEvaluation,
    //                NumberProject = survey.NumberProject,
    //                AverageMonthlyHours = survey.AverageMonthlyHours,
    //                TimeSpendCompany = survey.TimeSpendCompany,
    //                WorkAccident = survey.WorkAccident,
    //                PromotionLast5Years = survey.PromotionLast5Years,
    //                Department = survey.Department,
    //                Salary = survey.Salary,
    //                SubmissionDate = survey.SubmissionDate
    //            };


    //            var response = await client.PostAsJsonAsync("/predict", json);

    //            //if (response.IsSuccessStatusCode)
    //            //{
    //            //    var result = await response.Content.ReadFromJsonAsync<PredictionResult>();
    //            //    return result;
    //            //}
    //            //else
    //            //{
    //            //    var errorContent = await response.Content.ReadAsStringAsync();
    //            //    Console.WriteLine($"Error: {response.StatusCode}, {errorContent}");
    //            //    return new PredictionResult { Model1 = 0, Model2 = 0 }; // Default on failure
    //            //}
    //            if (response.IsSuccessStatusCode)
    //            {
    //                var result = await response.Content.ReadFromJsonAsync<PredictionResult>();
    //                Console.WriteLine($"PredictionModel1: {result.Model1}, PredictionModel2: {result.Model2}");
    //                return result;
    //            }
    //            else
    //            {
    //                var errorContent = await response.Content.ReadAsStringAsync();
    //                Console.WriteLine($"Error: {response.StatusCode}, {errorContent}");
    //                return new PredictionResult { Model1 = 0, Model2 = 0 }; // Default on failure
    //            }

    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Exception: {ex.Message}");
    //        return new PredictionResult { Model1 = 0, Model2 = 0 }; // Default on error
    //    }
    //}

    //public class PredictionResult
    //{
    //    public int Model1 { get; set; }
    //    public int Model2 { get; set; }
    //}
    private async Task<PredictionResult> GetPredictionAsync(Survey survey)
    {
        try
        {
            Console.WriteLine("[DEBUG] Starting GetPredictionAsync...");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5000"); // Flask API URL
                client.Timeout = TimeSpan.FromSeconds(30); // Add timeout

                // Prepare the request data with all fields
                var requestData = new
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
                    WorkAccident = survey.WorkAccident,
                    PromotionLast5Years = survey.PromotionLast5Years,
                    Department = survey.Department,
                    Salary = survey.Salary ?? "Low",
                    SubmissionDate = survey.SubmissionDate
                };

                // Log the complete request data
                Console.WriteLine($"[DEBUG] Request Data being sent to Flask API:\n{JsonSerializer.Serialize(requestData, new JsonSerializerOptions { WriteIndented = true })}");

                // Make the request
                Console.WriteLine("[DEBUG] Sending POST request to Flask API...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var response = await client.PostAsJsonAsync("/predict", requestData);

                stopwatch.Stop();
                Console.WriteLine($"[DEBUG] Received response in {stopwatch.ElapsedMilliseconds}ms. Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    // Read and log the raw response content
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DEBUG] Raw API Response:\n{responseContent}");

                    try
                    {
                        // Attempt to deserialize
                        var result = JsonSerializer.Deserialize<PredictionResult>(responseContent);

                        // Verify the result isn't null
                        if (result == null)
                        {
                            Console.WriteLine("[ERROR] Deserialized result is null");
                            return new PredictionResult { Model1 = 0, Model2 = 0 };
                        }

                        Console.WriteLine($"[DEBUG] Successfully deserialized response. Model1: {result.Model1}, Model2: {result.Model2}");
                        return result;
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"[ERROR] JSON Deserialization failed: {jsonEx.Message}");
                        Console.WriteLine($"[DEBUG] Response content that failed deserialization: {responseContent}");
                        return new PredictionResult { Model1 = 0, Model2 = 0 };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] API Request failed. Status: {response.StatusCode}, Content: {errorContent}");
                    return new PredictionResult { Model1 = 0, Model2 = 0 };
                }
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"[ERROR] HTTP Request Exception: {httpEx.Message}");
            if (httpEx.InnerException != null)
            {
                Console.WriteLine($"[ERROR] Inner Exception: {httpEx.InnerException.Message}");
            }
            return new PredictionResult { Model1 = 0, Model2 = 0 };
        }
        catch (TaskCanceledException timeoutEx)
        {
            Console.WriteLine($"[ERROR] Request timeout: {timeoutEx.Message}");
            return new PredictionResult { Model1 = 0, Model2 = 0 };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unexpected exception: {ex.ToString()}");
            return new PredictionResult { Model1 = 0, Model2 = 0 };
        }
        finally
        {
            Console.WriteLine("[DEBUG] GetPredictionAsync completed");
        }
    }

    // Add this class to properly map the Flask response
    public class PredictionResult
    {
        [JsonPropertyName("prediction_model1")] // Map to Flask's JSON key
        public int Model1 { get; set; }

        [JsonPropertyName("prediction_model2")] // Map to Flask's JSON key
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
