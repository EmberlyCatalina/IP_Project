using Microsoft.AspNetCore.Mvc;
using VolunteerFireDeptTemplate.Database;
using VolunteerFireDeptTemplate.Models;
using System.Linq;

namespace VolunteerFireDeptTemplate.Controllers
{
    public class AccountController : Controller
    {
        private readonly VolunteerDbContext _dbContext;

        public AccountController(VolunteerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(string name, string email, string password)
        {
            if (_dbContext.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError(string.Empty, "Email is already registered.");
                return View();
            }

            var user = new User { Name = name, Email = email, Password = password };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View();
            }

            // Store the user's name in TempData to pass it to the Welcome view
            TempData["UserName"] = user.Name;

            // Redirect to the Welcome action
            return RedirectToAction("Welcome");
        }

        public IActionResult Welcome()
        {
            // Pass the name from TempData to the view
            ViewBag.UserName = TempData["UserName"]?.ToString();
            return View();
        }
    }
}
