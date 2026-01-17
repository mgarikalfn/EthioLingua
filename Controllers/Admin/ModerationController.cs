using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BilingualLearningSystem.Data;
using BilingualLearningSystem.Models.Admin;
using BilingualLearningSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;
using BilingualLearningSystem.Models.ViewModels;

namespace BilingualLearningSystem.Controllers.Admin
{
    // Restrict access to Admins only
    [Authorize(Roles = "Admin")]
    public class ModerationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ModerationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // READ: Display all report tickets
        [HttpGet]
        public async Task<IActionResult> Index()
{
    // We join the ReportTickets with the Users table to get Names instead of IDs
    var reports = await (from ticket in _context.ReportTickets
                         join reporter in _context.Users on ticket.ReporterId equals reporter.Id
                         join reported in _context.Users on ticket.ReportedUserId equals reported.Id
                         select new ReportTicketViewModel
                         {
                             Id = ticket.Id,
                             ReporterName = reporter.FullName,
                             ReportedUserName = reported.FullName,
                             Reason = ticket.Reason,
                             CreatedAt = ticket.CreatedAt,
                             Status = ticket.Status
                         }).OrderByDescending(r => r.CreatedAt).ToListAsync();

    return View(reports);
}

        // UPDATE: Mark a ticket as Resolved or Reviewed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ReportStatus status)
        {
            var ticket = await _context.ReportTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = status;
            _context.Update(ticket);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Ticket #{id} has been marked as {status}.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE: Remove a ticket (Permanent Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.ReportTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _context.ReportTickets.Remove(ticket);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Report ticket deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // READ: Detailed view of a single ticket (Optional but good for UX)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.ReportTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // You could also fetch the Reported User's details here to show the Admin
            var reportedUser = await _userManager.FindByIdAsync(ticket.ReportedUserId);
            ViewBag.ReportedUserName = reportedUser?.FullName ?? "Unknown User";

            return View(ticket);
        }
    }
}