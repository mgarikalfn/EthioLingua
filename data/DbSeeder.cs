using Microsoft.AspNetCore.Identity;
using BilingualLearningSystem.Models.Identity;

namespace BilingualLearningSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Create Roles if they don't exist
            string[] roles = { "Admin", "LanguageExpert", "Learner" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Create the Super Admin
            var adminEmail = "admin@bilingual.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true,
                    Status = UserStatus.Active // Added status
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // 3. Create a Test Learner (Reporter)
            var learnerEmail = "learner@test.com";
            if (await userManager.FindByEmailAsync(learnerEmail) == null)
            {
                var learner = new ApplicationUser
                {
                    UserName = learnerEmail,
                    Email = learnerEmail,
                    FullName = "Abebe Kebede",
                    EmailConfirmed = true,
                    Status = UserStatus.Active
                };
                await userManager.CreateAsync(learner, "Student123!");
                await userManager.AddToRoleAsync(learner, "Learner");
            }

            // 4. Create a Test Language Expert (Reported User)
            var expertEmail = "expert@test.com";
            if (await userManager.FindByEmailAsync(expertEmail) == null)
            {
                var expert = new ApplicationUser
                {
                    UserName = expertEmail,
                    Email = expertEmail,
                    FullName = "Dr. Martha Smith",
                    EmailConfirmed = true,
                    Status = UserStatus.Active
                };
                await userManager.CreateAsync(expert, "Expert123!");
                await userManager.AddToRoleAsync(expert, "LanguageExpert");
            }

            Console.WriteLine(">>> SUCCESS: Roles and Test Users seeded.");
        }
    }
}