using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.Extensions.Localization;

namespace AlpineNeeds.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel(
    UserManager<ApplicationUser> userManager,
    IStringLocalizer<UsersModel> localizer) : BasePageModel
{

    public PaginatedList<UserViewModel> Model { get; private set; } = null!;
    public string SortColumn { get; set; } = "username";
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    public int PageIndex { get; set; } = 1;
    public IList<UserViewModel> Items => Model?.Items ?? new List<UserViewModel>();
    public bool HasPreviousPage => Model?.HasPreviousPage ?? false;
    public bool HasNextPage => Model?.HasNextPage ?? false;
    public int TotalPages => Model?.TotalPages ?? 0;

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
        var usersQuery = userManager.Users.AsQueryable();

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
            case "firstname":
                usersQuery = sortOrder.ToLower() == "desc"
                    ? usersQuery.OrderByDescending(u => u.FirstName)
                    : usersQuery.OrderBy(u => u.FirstName);
                break;
            case "lastname":
                usersQuery = sortOrder.ToLower() == "desc"
                    ? usersQuery.OrderByDescending(u => u.LastName)
                    : usersQuery.OrderBy(u => u.LastName);
                break;
            default:
                usersQuery = usersQuery.OrderBy(u => u.UserName);
                break;
        }

        // Get paginated results
        var paginatedUsers = await PaginatedList<ApplicationUser>.CreateAsync(
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
            var roles = await userManager.GetRolesAsync(user);
            var lockoutEnd = user.LockoutEnd;
            var isLockedOut = lockoutEnd != null && lockoutEnd > DateTimeOffset.Now;

            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                FullName = user.FullName,
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
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            AddPageError(localizer["User not found."]);
            return NotFound();
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            AddPageError(localizer["Failed to delete user."]);
            return RedirectToPage();
        }

        AddPageSuccess(localizer["User deleted successfully."]);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAdminRoleAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            AddPageError(localizer["User not found."]);
            return NotFound();
        }

        var isAdmin = await userManager.IsInRoleAsync(user, "Admin");

        if (isAdmin)
        {
            var result = await userManager.RemoveFromRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                AddPageError(localizer["Failed to remove admin role."]);
                return RedirectToPage();
            }

            if (!await userManager.IsInRoleAsync(user, "User"))
            {
                await userManager.AddToRoleAsync(user, "User");
            }

            AddPageSuccess(localizer["Admin role removed successfully."]);
        }
        else
        {
            var result = await userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                AddPageError(localizer["Failed to assign admin role."]);
                return RedirectToPage();
            }

            AddPageSuccess(localizer["Admin role assigned successfully."]);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleUserLockoutAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            AddPageError(localizer["User not found."]);
            return NotFound();
        }

        var lockoutEnd = user.LockoutEnd;
        var isLockedOut = lockoutEnd != null && lockoutEnd > DateTimeOffset.Now;

        if (isLockedOut)
        {
            var result = await userManager.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded)
            {
                AddPageError(localizer["Failed to unlock user."]);
                return RedirectToPage();
            }

            AddPageSuccess(localizer["User unlocked successfully."]);
        }
        else
        {
            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(1));
            if (!result.Succeeded)
            {
                AddPageError(localizer["Failed to lock user."]);
                return RedirectToPage();
            }

            AddPageSuccess(localizer["User locked successfully."]);
        }

        return RedirectToPage();
    }
}

public class UserViewModel
{
    public required string Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public required List<string> Roles { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLockedOut { get; set; }
}