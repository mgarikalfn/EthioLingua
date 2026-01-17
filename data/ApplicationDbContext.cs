using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BilingualLearningSystem.Models.Identity;
using BilingualLearningSystem.Models.Admin; // This points to Step 2

namespace BilingualLearningSystem.Data
{
    // Inheriting from IdentityDbContext automatically creates all User/Role tables
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ReportTicket> ReportTickets { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // This ensures Identity tables are configured correctly for MySQL
        }
    }
}