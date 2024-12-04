using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VolunteerFireDeptTemplate.Database;
using VolunteerFireDeptTemplate.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews(); // Use AddControllersWithViews for MVC apps

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to login page if not authenticated
        options.LogoutPath = "/Account/Logout"; // Logout path
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Set session timeout to 20 minutes
        options.SlidingExpiration = true; // Refresh the cookie expiration time on each request
    });

// Add DbContext for accessing the database
builder.Services.AddDbContext<VolunteerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add PasswordHasher service to DI container
builder.Services.AddSingleton<PasswordHasher<User>>();

var app = builder.Build();

// Seed data for admin user (run at startup)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<VolunteerDbContext>();
    var passwordHasher = services.GetRequiredService<PasswordHasher<User>>(); // Get PasswordHasher from DI container
    await SeedData.Initialize(services, dbContext, passwordHasher);  // Seed the admin user and test data
}

// Enable HTTPS redirection in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Ensure you have a default route setup
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Seed data class
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, VolunteerDbContext dbContext, PasswordHasher<User> passwordHasher)
    {
        // Add a default admin user if not already in the database
        if (!dbContext.Users.Any(u => u.Email == "admin@admin.com"))
        {
            var adminUser = new User
            {
                Name = "Admin User",
                Email = "admin@admin.com",
                Role = "Admin"  // Set default role to Admin
            };

            var hashedPassword = passwordHasher.HashPassword(adminUser, "AdminPassword123!");
            adminUser.Password = hashedPassword;

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        // Seed 3 test users if they don't exist
        var testUsers = new[]
        {
            new User { Name = "Test User 1", Email = "testuser1@example.com", Role = "User" },
            new User { Name = "Test User 2", Email = "testuser2@example.com", Role = "User" },
            new User { Name = "Test User 3", Email = "testuser3@example.com", Role = "User" }
        };

        foreach (var user in testUsers)
        {
            if (!dbContext.Users.Any(u => u.Email == user.Email))
            {
                var hashedPassword = passwordHasher.HashPassword(user, "Passw0rd!");
                user.Password = hashedPassword;
                dbContext.Users.Add(user);
            }
        }

        await dbContext.SaveChangesAsync();

        // Seed 3 test volunteers
        var testVolunteers = new[]
        {
            new Volunteer { FullName = "Volunteer 1", Email = "volunteer1@example.com", PhoneNumber = "123-456-7890", Experience = "Firefighter", Availability = "Weekends" },
            new Volunteer { FullName = "Volunteer 2", Email = "volunteer2@example.com", PhoneNumber = "123-456-7891", Experience = "Medic", Availability = "Weekdays" },
            new Volunteer { FullName = "Volunteer 3", Email = "volunteer3@example.com", PhoneNumber = "123-456-7892", Experience = "Driver", Availability = "Evenings" }
        };

        foreach (var volunteer in testVolunteers)
        {
            if (!dbContext.Volunteers.Any(v => v.Email == volunteer.Email))
            {
                dbContext.Volunteers.Add(volunteer);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}