using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // Required for PasswordHasher
using VolunteerFireDeptTemplate.Database;
using VolunteerFireDeptTemplate.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace VolunteerFireDeptTemplate.Controllers
{
    public class AccountController : Controller
    {
        private readonly VolunteerDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(VolunteerDbContext dbContext)
        {
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<User>();  // Initialize PasswordHasher
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(string name, string email, string password, string confirmPassword)
        {
            // Check if email is already registered
            if (_dbContext.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError(string.Empty, "Email is already registered.");
                return View();
            }

            // Validate that the passwords match
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            // Validate password against the required standards
            if (!IsPasswordValid(password))
            {
                ModelState.AddModelError(string.Empty, "Password must be at least 8 characters long, contain an uppercase letter, a number, and a special character.");
                return View();
            }

            // Hash the password before saving
            var hashedPassword = _passwordHasher.HashPassword(null, password);  // We don't have a user object yet, so passing null

            // Create a new user and add it to the database
            var user = new User { Name = name, Email = email, Password = hashedPassword };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            // Set success message and redirect to login page
            TempData["SuccessMessage"] = "You have successfully signed up. Please log in.";
            return RedirectToAction("SignUpSuccess");
        }

        [HttpGet]
        public IActionResult SignUpSuccess()
        {
            // Display the success message view
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View();
            }

            // Verify the password using the PasswordHasher
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
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

        // Helper method to validate the password
        private bool IsPasswordValid(string password)
        {
            // Password must meet the following criteria:
            // - At least 8 characters
            // - At least one uppercase letter
            // - At least one number
            // - At least one special character
            var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
            return regex.IsMatch(password);
        }
    }
}