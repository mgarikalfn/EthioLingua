using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BilingualLearningSystem.Models.Identity;
using BilingualLearningSystem.Services;

namespace BilingualLearningSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuditService _auditService;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
        }


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
                userRoles[user.Id] = roles.FirstOrDefault() ?? "None";
            }

            ViewBag.UserRoles = userRoles;

            return View(users);
        }



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


        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            if (!await _roleManager.RoleExistsAsync(newRole))
                return BadRequest("Invalid role");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Prevent self-demotion
            if (user.Id == _userManager.GetUserId(User) && newRole != "Admin")
                return BadRequest("Admin cannot demote themselves");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            await _auditService.LogAction(
                User.Identity.Name,
                $"Changed role to {newRole}",
                user.Email
            );

            return RedirectToAction(nameof(Index));
        }


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
