using Microsoft.AspNetCore.Identity;

namespace BilingualLearningSystem.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Custom fields for your language app
        public string FullName { get; set; } = string.Empty;
        public string? BaseLanguage { get; set; }
        public string? TargetLanguage { get; set; }
        
        // This tracks if the user is active or banned
        public UserStatus Status { get; set; } = UserStatus.Active;
    }

    public enum UserStatus
    {
        Active,
        Flagged,
        Muted,
        Suspended
    }
}