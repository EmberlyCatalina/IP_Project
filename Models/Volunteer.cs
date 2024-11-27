using System.ComponentModel.DataAnnotations;

namespace VolunteerFireDeptTemplate.Models
{
    public class Volunteer
    {
        [Key] // primary key
        public int Id { get; set; }  // Add an Id property to serve as the primary key

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Experience or Interests")]
        public string Experience { get; set; } = string.Empty;

        [Display(Name = "Availability")]
        public string Availability { get; set; } = string.Empty;
    }
}
