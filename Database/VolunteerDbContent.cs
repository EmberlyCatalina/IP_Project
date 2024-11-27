namespace VolunteerFireDeptTemplate.Database
{
    using Microsoft.EntityFrameworkCore;
    using Models;

namespace VolunteerFireDeptTemplate.Data
{
    public class VolunteerDbContext : DbContext
    {
        public VolunteerDbContext(DbContextOptions<VolunteerDbContext> options): base(options) { }

        public DbSet<Volunteer> Volunteers { get; set; }
    }
}

}
