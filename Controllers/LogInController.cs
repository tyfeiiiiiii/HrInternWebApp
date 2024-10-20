using HrInternWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Cfg;
using NHSession = NHibernate.ISession;//if directly use ISession, do not know refer to Hibernate or AspNetCore.ISession

namespace HrInternWebApp.Controllers
{
    public class LogInController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //used to submit data
        public IActionResult Login(LogIn model)
        {
            if (ModelState.IsValid)
            {
                ISessionFactory factory = new Configuration().Configure().BuildSessionFactory();

                using (NHSession session = factory.OpenSession())
                {
                    var employee = AuthenticateUser(session, model.username, model.password);

                    if (employee != null) //if user found
                    {
                        //Save employee details in session
                        HttpContext.Session.SetInt32("UserId", employee.empId);
                        HttpContext.Session.SetString("Username", employee.username);

                        // after successful login 
                        return RedirectToAction("Home", "Leave");
                    }
                    //if credential invald, show error message
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                }
                factory.Close();
            }
            return View(model);//if validation fail, return to log in view
        }

        private Employee AuthenticateUser(NHSession session, string username, string password)
        {
            var employee = session.Query<Employee>().FirstOrDefault(employee => employee.username == username && employee.password == password);
            return employee;//return employee if found
        }

    }
}
