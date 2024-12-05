using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerFireDeptTemplate.Database;
using VolunteerFireDeptTemplate.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace VolunteerFireDeptTemplate.Controllers
{
    [Authorize(Roles = "Admin")]  // Ensure only Admin users can access this controller
    public class AdminController : Controller
    {
        private readonly VolunteerDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;

        public AdminController(VolunteerDbContext dbContext, PasswordHasher<User> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;  // Use dependency injection for PasswordHasher
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

        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(); // Display password change form
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // Check if User.Identity is null
            if (User.Identity?.Name == null)
            {
                ModelState.AddModelError(string.Empty, "User is not authenticated.");
                return View();  // Return to password change form if user is not authenticated
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View();  // Return to password change form if user not found
            }

            // Validate that the passwords match
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            // Verify the old password
            if (_passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid old password.");
                return View();
            }

            // Hash the new password and update it
            var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
            user.Password = hashedPassword;

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Password successfully changed!";
            return RedirectToAction("AdminDashboard"); // Redirect back to the admin dashboard
        }
    }
}