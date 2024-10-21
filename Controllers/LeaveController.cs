using Microsoft.AspNetCore.Mvc;
using HrInternWebApp.Models;
using NHibernate;
using ISession = NHibernate.ISession; 

public class LeaveController : Controller
{
    private readonly ISession _session;

    public LeaveController(ISession session)
    {
        _session = session;
    }

    public IActionResult ViewLeave() => View(_session.Query<Leave>().ToList());

    public IActionResult ApplyLeave() => View();

    [HttpPost]
    public IActionResult ApplyLeave(Leave leave)
    {
        if (ModelState.IsValid)
        {
            _session.SaveOrUpdate(leave);
            return RedirectToAction("ViewLeave");
        }
        return View(leave);
    }
}
