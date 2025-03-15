using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;

namespace AlpineNeeds.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : BasePageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PaginatedList<UserViewModel> Model { get; private set; } = null!;
    public string SortColumn { get; set; } = "username";
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    public int PageIndex { get; set; } = 1;
    public IList<UserViewModel> Items => Model?.Items ?? new List<UserViewModel>();
    public bool HasPreviousPage => Model?.HasPreviousPage ?? false;
    public bool HasNextPage => Model?.HasNextPage ?? false;
    public int TotalPages => Model?.TotalPages ?? 0;

    public UsersModel(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> OnGetAsync(string? sortColumn = "username", string? sortOrder = "asc", int? pageNumber = 1)
    {
        int pageSize = 10;
        pageNumber ??= 1;
        sortColumn ??= "username";
        sortOrder ??= "asc";
        SortColumn = sortColumn;
        SortOrder = sortOrder.ToLower() == "desc" ? SortOrder.Descending : SortOrder.Ascending;
        PageIndex = pageNumber.Value;

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
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Roles = roles.ToList(),
                IsAdmin = roles.Contains("Admin"),
                IsLockedOut = isLockedOut
            });
        }

        Model = new PaginatedList<UserViewModel>(
            userViewModels,
            paginatedUsers.TotalPages * pageSize,
            paginatedUsers.PageIndex,
            pageSize,
            paginatedUsers.SortColumn,
            paginatedUsers.SortOrder
        );

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            AddPageError("Failed to delete user.");
            return RedirectToPage();
        }

        AddPageSuccess("User deleted successfully.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAdminRoleAsync(string id)
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
                AddPageError("Failed to remove admin role.");
                return RedirectToPage();
            }

            // Ensure user has User role
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            AddPageSuccess("Admin role removed successfully.");
        }
        else
        {
            // Add admin role
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                AddPageError("Failed to assign admin role.");
                return RedirectToPage();
            }

            AddPageSuccess("Admin role assigned successfully.");
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleUserLockoutAsync(string id)
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
                AddPageError("Failed to unlock user.");
                return RedirectToPage();
            }

            AddPageSuccess("User unlocked successfully.");
        }
        else
        {
            // Lock the user for one year
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(1));
            if (!result.Succeeded)
            {
                AddPageError("Failed to lock user.");
                return RedirectToPage();
            }

            AddPageSuccess("User locked successfully.");
        }

        return RedirectToPage();
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