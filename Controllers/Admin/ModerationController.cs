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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.ReportTickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            var reportedUser = await _userManager.FindByIdAsync(ticket.ReportedUserId);
            var reporterUser = await _userManager.FindByIdAsync(ticket.ReporterId);

            var model = new ModerationDetailViewModel
            {
                Ticket = ticket,
                ReportedUser = reportedUser,
                ReporterName = reporterUser?.FullName ?? "Unknown"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeAction(
            int ticketId,
            string action)
        {
            var ticket = await _context.ReportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(ticket.ReportedUserId);
            if (user == null)
                return NotFound();

            switch (action)
            {
                case "Mute":
                    user.Status = UserStatus.Muted;
                    break;

                case "Suspend":
                    user.Status = UserStatus.Suspended;
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    break;

                case "ResolveOnly":
                    break;

                default:
                    return BadRequest("Invalid moderation action");
            }

            ticket.Status = ReportStatus.Resolved;

            await _userManager.UpdateAsync(user);
            _context.Update(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuditHistory()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return View(logs);
        }

    }
}