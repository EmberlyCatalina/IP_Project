namespace VolunteerFireDeptTemplate.Database
{
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class VolunteerDbContext : DbContext
    {
        public VolunteerDbContext(DbContextOptions<VolunteerDbContext> options): base(options) { }

        public DbSet<Volunteer> Volunteers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}

