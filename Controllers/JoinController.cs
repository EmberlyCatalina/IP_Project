using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using VolunteerFireDeptTemplate.Models;
using VolunteerFireDeptTemplate.Database;
using System.Threading.Tasks; // Needed for async functionality

namespace VolunteerFireDeptTemplate.Controllers
{
    public class JoinController : Controller
    {
        private readonly VolunteerDbContext _context;

        // Inject VolunteerDbContext into the controller
        public JoinController(VolunteerDbContext context)
        {
            _context = context;
        }

        // GET: /Join
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Volunteer volunteer)
        {
            // Check if the model is valid
            if (ModelState.IsValid)
            {
                // Add the new volunteer to the Volunteers DbSet
                _context.Volunteers.Add(volunteer);

                // Save the changes to the database asynchronously
                await _context.SaveChangesAsync();
                
                // Call the method to send the email
                SendEmail(volunteer);

                // Set a success message using TempData
                TempData["Message"] = "Thank you for signing up as a volunteer!";

                // Redirect to the same page
                return RedirectToAction("Index");
            }

            // If the form submission is not valid, re-render the form with validation messages
            return View(volunteer);
        }

        // Method to send the email to the fire chief
        private void SendEmail(Volunteer volunteer)
        {
            try
            {
                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("testforschool492@gmail.com");  // Placeholder email address
                mailMessage.To.Add("wsonger@islander.tamucc.edu");  // Placeholder email address of the fire chief
                mailMessage.Subject = "New Volunteer Sign-Up";
                mailMessage.Body = $@"
                    Full Name: {volunteer.FullName}
                    Email: {volunteer.Email}
                    Phone: {volunteer.PhoneNumber}
                    Experience: {volunteer.Experience}
                    Availability: {volunteer.Availability}
                ";
                mailMessage.IsBodyHtml = false;  // Plain text email body

                var smtpClient = new SmtpClient("smtp.gmail.com")  // SMTP server
                {
                    Port = 587,  // Typical SMTP port
                    Credentials = new NetworkCredential("testforschool492@gmail.com", "testpass27!@"),  // SMTP credentials
                    EnableSsl = true  // Use SSL for security
                };

                smtpClient.Send(mailMessage);  // Send the email
            }
            catch (Exception ex)
            {
                // Log any exceptions (optional) or handle the error as needed
                Console.WriteLine("Email failed to send: " + ex.Message);
            }
        }
    }
}