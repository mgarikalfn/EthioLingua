using Microsoft.EntityFrameworkCore;
using BilingualLearningSystem.Data;
using BilingualLearningSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE CONFIGURATION
// ---------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. IDENTITY CONFIGURATION
// ---------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // Password settings (relaxed for development)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. MVC SERVICES
// ---------------------------------------------------------
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. AUTOMATIC MIGRATION & SEEDING
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // This applies migrations and creates the DB if it doesn't exist
        await context.Database.MigrateAsync(); 
        
        // Seed initial roles and the admin user
        await DbSeeder.SeedRolesAndAdminAsync(services);
        
        Console.WriteLine(">>> SUCCESS: Database Migrated & Admin Seeded.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> ERROR during startup: {ex.Message}");
    }
}

// 5. MIDDLEWARE PIPELINE
// ---------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Order is critical: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();