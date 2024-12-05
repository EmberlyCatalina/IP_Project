using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VolunteerFireDeptTemplate.Database;
using VolunteerFireDeptTemplate.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;

namespace VolunteerFireDeptTemplate.Controllers
{
    public class AccountController : Controller
    {
        private readonly VolunteerDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(VolunteerDbContext dbContext, PasswordHasher<User> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;  // Use dependency injection for PasswordHasher
        }

        // GET: /Account/SignUp
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: /Account/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(string name, string email, string password, string confirmPassword)
        {
            if (!ModelState.IsValid)
            {
                return View(); // Return to view if there are validation errors
            }

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

            // Hash the password before saving
            var hashedPassword = _passwordHasher.HashPassword(new User(), password); // Hash password for storage

            // Create a new user and add it to the database
            var user = new User { Name = name, Email = email, Password = hashedPassword, Role = "User" };
            _dbContext.Users.Add(user);

            try
            {
                _dbContext.SaveChanges(); // Attempt to save changes to the database
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the user: " + ex.Message);
                return View(); // Return with error message if saving fails
            }

            // Set success message and redirect to the success page
            TempData["SuccessMessage"] = "You have successfully signed up. Please log in.";
            return RedirectToAction("SignUpSuccess");
        }

        // GET: /Account/SignUpSuccess
        [HttpGet]
        public IActionResult SignUpSuccess()
        {
            return View();
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (!ModelState.IsValid)
            {
                return View(); // Stay on the same page if there are validation errors
            }

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
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role) // Add the role claim
            };

            // Create the identity for the user
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Set the authentication cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Redirect based on role (Admin or User)
            if (user.Role == "Admin")
            {
                return RedirectToAction("AdminDashboard", "Admin"); // Admin Dashboard
            }

            return RedirectToAction("UserDashboard", "Account"); // Regular user dashboard
        }

        // GET: /Account/UserDashboard
        [HttpGet]
        [Authorize]
        public IActionResult UserDashboard()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirstValue("UserId");
            var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View("UserDashboard");
            }

            // Verify current password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, currentPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                return View("UserDashboard");
            }

            // Validate that new password matches confirm password
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "New password and confirm password do not match.");
                return View("UserDashboard");
            }

            // Validate new password against the required standards
            if (!IsPasswordValid(newPassword))
            {
                ModelState.AddModelError(string.Empty, "New password must be at least 8 characters long, contain an uppercase letter, a number, and a special character.");
                return View("UserDashboard");
            }

            // Hash the new password
            user.Password = _passwordHasher.HashPassword(user, newPassword);

            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been changed successfully!";
            return RedirectToAction("UserDashboard");
        }

        // Helper method to validate the password
        private bool IsPasswordValid(string password)
        {
            var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
            return regex.IsMatch(password);
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user by clearing the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to login page
            return RedirectToAction("Login");
        }
    }
}