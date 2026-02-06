using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BilingualLearningSystem.Models.Identity;
using BilingualLearningSystem.Services;

namespace BilingualLearningSystem.Controllers.Admin
{
    /// <summary>
    /// Admin-only controller for managing application users, their roles, and statuses.
    /// Also records administrative actions to the audit log.
    /// </summary>
    [Authorize(Roles = "Admin")]       
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementController"/>.
        /// </summary>
        /// <param name="userManager">ASP.NET Core Identity user manager.</param>
        /// <param name="roleManager">ASP.NET Core Identity role manager.</param>
        /// <param name="auditService">Service for recording admin audit events.</param>
        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
        }

        /// <summary>
        /// Lists users, computes basic system metrics, resolves each user's primary role,
        /// and returns the management view.
        /// </summary>
        /// <returns>The Index view with users, role map, and metrics in ViewBag.</returns>
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            // ===== System Metrics =====
            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.Status == UserStatus.Active);
            ViewBag.SuspendedUsers = users.Count(u => u.Status == UserStatus.Suspended);

            // ===== Resolve Roles =====
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Fallback to "No Role" if the list is empty
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        /// <summary>
        /// Creates a new user with the specified role and logs the action.
        /// </summary>
        /// <param name="email">Email address to use as username.</param>
        /// <param name="password">Initial password for the user.</param>
        /// <param name="role">Role to assign; must exist.</param>
        /// <returns>
        /// Redirects to <see cref="Index"/> on success; returns 400 for invalid role
        /// or identity errors.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser(
            string email,
            string password,
            string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest("Invalid role");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Status = UserStatus.Active
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, role);

            await _auditService.LogAction(
                User.Identity.Name,
                $"Created user with role {role}",
                email
            );

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Replaces the user's existing roles with the provided role and audits the change.
        /// </summary>
        /// <param name="userId">The target user's identifier.</param>
        /// <param name="newRole">The new role to assign; must exist.</param>
        /// <returns>
        /// Redirects to <see cref="Index"/>. Sets TempData with success or error messages.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                TempData["Error"] = "Missing User ID or Role selection.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Safety check: Does the role actually exist in the database?
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                TempData["Error"] = $"The role '{newRole}' does not exist in the database system.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove old roles
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Add new role
            var addResult = await _userManager.AddToRoleAsync(user, newRole);

            if (addResult.Succeeded)
            {
                await _auditService.LogAction(
                    User.Identity.Name,
                    "Role Change",
                    user.Email,
                    $"Changed role to {newRole}");

                TempData["Success"] = $"User {user.FullName ?? user.Email} updated to {newRole}.";
            }
            else
            {
                // Capture specific Identity errors (e.g., database connection, concurrency)
                TempData["Error"] = string.Join(", ", addResult.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates the user's status and enforces Identity lockout when suspended.
        /// </summary>
        /// <param name="userId">The target user's identifier.</param>
        /// <param name="status">The new status to set.</param>
        /// <returns>Redirects to <see cref="Index"/> after update and audit logging.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string userId, UserStatus status)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.Status = status;

            // Enforce suspension at Identity level
            if (status == UserStatus.Suspended)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnd = null;
            }

            await _userManager.UpdateAsync(user);

            await _auditService.LogAction(
                User.Identity.Name,
                $"Changed status to {status}",
                user.Email
            );

            return RedirectToAction(nameof(Index));
        }
    }
}