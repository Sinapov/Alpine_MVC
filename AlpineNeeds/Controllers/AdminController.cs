using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlpineNeeds.Models;
using System.Threading.Tasks;

namespace AlpineNeeds.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(string? sortColumn = "username", string? sortOrder = "asc", int? pageNumber = 1)
        {
            int pageSize = 10;
            pageNumber ??= 1;
            sortColumn ??= "username";
            sortOrder ??= "asc";
            
            // Get users query
            var usersQuery = _userManager.Users.AsQueryable();

            // Apply sorting
            switch (sortColumn.ToLower())
            {
                case "username":
                    usersQuery = sortOrder.ToLower() == "desc" 
                        ? usersQuery.OrderByDescending(u => u.UserName)
                        : usersQuery.OrderBy(u => u.UserName);
                    break;
                case "email":
                    usersQuery = sortOrder.ToLower() == "desc" 
                        ? usersQuery.OrderByDescending(u => u.Email)
                        : usersQuery.OrderBy(u => u.Email);
                    break;
                default:
                    usersQuery = usersQuery.OrderBy(u => u.UserName);
                    break;
            }

            // Get paginated results
            var paginatedUsers = await PaginatedList<IdentityUser>.CreateAsync(
                usersQuery, 
                pageNumber.Value,
                pageSize,
                sortColumn,
                sortOrder
            );

            // Convert to view models
            var userViewModels = new List<UserViewModel>();
            foreach (var user in paginatedUsers.Items)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var lockoutEnd = user.LockoutEnd;
                var isLockedOut = lockoutEnd != null && lockoutEnd > DateTimeOffset.Now;

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    IsAdmin = roles.Contains("Admin"),
                    IsLockedOut = isLockedOut
                });
            }

            var viewModel = new PaginatedList<UserViewModel>(
                userViewModels,
                paginatedUsers.TotalPages * pageSize,
                paginatedUsers.PageIndex,
                pageSize,
                paginatedUsers.SortColumn,
                paginatedUsers.SortOrder
            );

            return View(viewModel);
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to delete user.";
                return RedirectToAction(nameof(Users));
            }

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/ToggleAdminRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                // Remove admin role
                var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    TempData["Error"] = "Failed to remove admin role.";
                    return RedirectToAction(nameof(Users));
                }

                // Ensure user has User role
                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                TempData["Success"] = "Admin role removed successfully.";
            }
            else
            {
                // Add admin role
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    TempData["Error"] = "Failed to assign admin role.";
                    return RedirectToAction(nameof(Users));
                }

                TempData["Success"] = "Admin role assigned successfully.";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/ToggleUserLockout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLockout(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var lockoutEnd = user.LockoutEnd;
            var isLockedOut = lockoutEnd != null && lockoutEnd > DateTimeOffset.Now;

            if (isLockedOut)
            {
                // Unlock the user
                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (!result.Succeeded)
                {
                    TempData["Error"] = "Failed to unlock user.";
                    return RedirectToAction(nameof(Users));
                }

                TempData["Success"] = "User unlocked successfully.";
            }
            else
            {
                // Lock the user for one year
                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(1));
                if (!result.Succeeded)
                {
                    TempData["Error"] = "Failed to lock user.";
                    return RedirectToAction(nameof(Users));
                }

                TempData["Success"] = "User locked successfully.";
            }

            return RedirectToAction(nameof(Users));
        }
    }

    public class UserViewModel
    {
        public required string Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public required List<string> Roles { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsLockedOut { get; set; }
    }
}