using BilingualLearningSystem.Data;
using BilingualLearningSystem.Models.Admin;

namespace BilingualLearningSystem.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAction(string adminEmail, string action, string targetUser, string details = "")
        {
            var log = new AuditLog
            {
                AdminEmail = adminEmail,
                Action = action,
                TargetUser = targetUser,
                Details = details,
                Timestamp = DateTime.Now
            } ;

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}