using BilingualLearningSystem.Models.Admin;

namespace BilingualLearningSystem.Models.ViewModels
{
    public class ReportTicketViewModel
    {
        public int Id { get; set; }
        public string ReporterName { get; set; }
        public string ReportedUserName { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public ReportStatus Status { get; set; }
    }
}