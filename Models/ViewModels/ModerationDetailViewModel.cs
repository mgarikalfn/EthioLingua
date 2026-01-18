using BilingualLearningSystem.Models.Admin;
using BilingualLearningSystem.Models.Identity;

public class ModerationDetailViewModel
{
    public ReportTicket Ticket { get; set; }
    public ApplicationUser ReportedUser { get; set; }
    public string ReporterName { get; set; }
}
