using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication; // Required for managing authentication
using Microsoft.AspNetCore.Identity; // Required for PasswordHasher
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using VolunteerFireDeptTemplate.Database;
using VolunteerFireDeptTemplate.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Linq;

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
                return View(); // Stay on the same page if there's an error
            }

            // Validate that the passwords match
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(); // Stay on the same page if passwords don't match
            }

            // Validate password against the required standards
            if (!IsPasswordValid(password))
            {
                ModelState.AddModelError(string.Empty, "Password must be at least 8 characters long, contain an uppercase letter, a number, and a special character.");
                return View(); // Stay on the same page if the password doesn't meet the requirements
            }

            // Temp user to avoid passing null object and suppress warning
            var tempUser = new User();  // Temporary user without any real data
            // Hash the password before saving
            var hashedPassword = _passwordHasher.HashPassword(tempUser, password);  // We don't have a user object yet, so passing null

            // Create a new user and add it to the database
            var user = new User { Name = name, Email = email, Password = hashedPassword };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            // Set success message and redirect to the success page
            TempData["SuccessMessage"] = "You have successfully signed up. Please log in.";
            return RedirectToAction("SignUpSuccess"); // Redirect only when the user is successfully added
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
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View();  // Return to login page with error
            }

            // Create the claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()) // You can also store the User ID for future reference
            };

            // Create the identity for the user
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Set the authentication cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Store the user name in TempData for the Welcome page
            TempData["UserName"] = user.Name;

            // Pass the user’s name directly to the view
            return RedirectToAction("Welcome", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            // Sign out the user by clearing the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to login page
            return RedirectToAction("Login");
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