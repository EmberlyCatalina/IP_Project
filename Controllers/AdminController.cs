using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerFireDeptTemplate.Database;
using VolunteerFireDeptTemplate.Models;

namespace VolunteerFireDeptTemplate.Controllers
{
    [Authorize(Roles = "Admin")]  // Ensure only Admin users can access this controller
    public class AdminController : Controller
    {
        private readonly VolunteerDbContext _dbContext;

        public AdminController(VolunteerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult AdminDashboard()  // Updated to match the view name
        {
            // Get the list of all users
            var users = _dbContext.Users.ToList();

            // Get the list of all volunteers
            var volunteers = _dbContext.Volunteers.ToList();

            // Pass the lists of users and volunteers to the view
            ViewBag.Users = users;
            ViewBag.Volunteers = volunteers;

            return View();  // This will render the AdminDashboard.cshtml view
        }
    }
}