namespace VolunteerFireDeptTemplate.Database
{
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class VolunteerDbContext : DbContext
    {
        public VolunteerDbContext(DbContextOptions<VolunteerDbContext> options): base(options) { }

        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<User> Users { get; set; }
    }
}

