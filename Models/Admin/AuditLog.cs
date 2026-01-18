using System.ComponentModel.DataAnnotations;

namespace BilingualLearningSystem.Models.Admin
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string AdminEmail { get; set; } // Who did it?

        [Required]
        public string Action { get; set; } // e.g., "Suspended User", "Promoted to Expert"

        [Required]
        public string TargetUser { get; set; } // Which user was affected?

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Details { get; set; } // Optional: "Reason: Hate speech"
    }
}