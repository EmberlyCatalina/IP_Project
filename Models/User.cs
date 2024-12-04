namespace VolunteerFireDeptTemplate.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!; // hashed password
        public string Role { get; set; } = "User"; // default to User
    }
}