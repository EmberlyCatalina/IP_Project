using Microsoft.AspNetCore.Mvc;
using VolunteerFireDeptTemplate.Models;

namespace VolunteerFireDeptTemplate.Controllers
{
    public class JoinController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Volunteer volunteer)
        {
            if (ModelState.IsValid)
            {
                TempData["Message"] = "Thank you for signing up!";
                return RedirectToAction("Index");
            }
            return View(volunteer);
        }
    }
}
