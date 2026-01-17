using Microsoft.EntityFrameworkCore;
using BilingualLearningSystem.Data;
using BilingualLearningSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES CONFIGURATION
// ---------------------------------------------------------

// Add Database Context using Pomelo MySQL driver
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add Identity Service with basic password rules
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2. DATABASE INITIALIZATION & SEEDING
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database and tables are created (without deleting existing data)
        context.Database.EnsureCreated(); 
        
        // Run the seeder to create Roles and the Admin User
        await DbSeeder.SeedRolesAndAdminAsync(services);
        
        Console.WriteLine(">>> SUCCESS: Database is up-to-date and Admin seeded.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> ERROR during initialization: {ex.Message}");
    }
}

// 3. MIDDLEWARE PIPELINE
// ---------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();