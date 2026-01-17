using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BilingualLearningSystem.Data;
using BilingualLearningSystem.Models.Admin;
using System.Security.Claims;

namespace BilingualLearningSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReport(string reportedUserId, string reason)
        {
            if (string.IsNullOrEmpty(reason)) return BadRequest();

            var report = new ReportTicket
            {
                ReporterId = User.FindFirstValue(ClaimTypes.NameIdentifier), // Current User
                ReportedUserId = reportedUserId,
                Reason = reason,
                CreatedAt = DateTime.Now,
                Status = ReportStatus.Open
            };

            _context.ReportTickets.Add(report);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Report submitted successfully. Admin will review it.";
            return RedirectToAction("Index", "Home");
        }
    }
}