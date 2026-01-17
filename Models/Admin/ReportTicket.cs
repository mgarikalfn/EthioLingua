using System.ComponentModel.DataAnnotations;

namespace BilingualLearningSystem.Models.Admin
{
    public enum ReportStatus { Open, UnderReview, Resolved, Dismissed }

    public class ReportTicket
    {
        public int Id { get; set; }

        [Required]
        public string ReporterId { get; set; } // The user who complained

        [Required]
        public string ReportedUserId { get; set; } // The user being complained about

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ReportStatus Status { get; set; } = ReportStatus.Open;

        // Optional: Link to a specific chat message if you have a Chat table
        public int? ChatMessageId { get; set; } 
    }
}