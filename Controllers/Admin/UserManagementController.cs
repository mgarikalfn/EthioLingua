using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BilingualLearningSystem.Models.Identity;

namespace BilingualLearningSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // READ: The Main Dashboard
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // UPDATE: Promotion / Demotion Logic
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction(nameof(Index));
        }

        // UPDATE: Governance (Suspend/Mute)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string userId, UserStatus status)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Status = status;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}