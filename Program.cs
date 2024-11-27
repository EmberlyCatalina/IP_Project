using VolunteerFireDeptTemplate.Database;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews(); // Use AddControllersWithViews for MVC apps

// Configure HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7149;
});


builder.Services.AddDbContext<VolunteerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



var app = builder.Build();

// Enable HTTPS redirection in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// Ensure you have a default route setup
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map your controllers
app.MapControllers();



app.Run();
