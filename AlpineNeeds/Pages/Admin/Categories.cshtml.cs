using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CategoriesModel(ApplicationDbContext context) : BasePageModel
{
    public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    [BindProperty]
    public required Models.Category Category { get; set; }

    public async Task OnGetAsync()
    {
        var categories = await context.Categories
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.ParentCategoryId)
            .ThenBy(c => c.DisplayOrder)
            .ToListAsync();

        Categories = BuildCategoryTree(categories);
    }

    public async Task<IActionResult> OnPostUpdateOrderAsync(int id, int newOrder)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        category.DisplayOrder = newOrder;
        await context.SaveChangesAsync();
        AddPageSuccess("Category order updated successfully.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Category.ParentCategoryId.HasValue)
        {
            // Check for circular dependency
            var parentId = Category.ParentCategoryId;
            var visited = new HashSet<int>();
            while (parentId.HasValue)
            {
                if (parentId == Category.Id || !visited.Add(parentId.Value))
                {
                    AddError("Circular dependency detected in category hierarchy", "Category.ParentCategoryId");
                    return Page();
                }
                var parent = await context.Categories.FindAsync(parentId);
                if (parent == null) break;
                parentId = parent.ParentCategoryId;
            }
        }

        if (Category.Id == 0)
        {
            // Set display order to be last in the current level
            var maxOrder = await context.Categories
                .Where(c => c.ParentCategoryId == Category.ParentCategoryId)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? -1;
            Category.DisplayOrder = maxOrder + 1;
            await context.Categories.AddAsync(Category);
            AddPageSuccess("Category created successfully.");
        }
        else
        {
            var existingCategory = await context.Categories.FindAsync(Category.Id);
            if (existingCategory == null)
                return NotFound();

            // Check if we're trying to make a category its own descendant
            if (Category.ParentCategoryId.HasValue &&
                await HasDescendant(existingCategory.Id, Category.ParentCategoryId.Value))
            {
                AddError("Cannot make a category a child of its own descendant", "Category.ParentCategoryId");
                return Page();
            }

            existingCategory.Name = Category.Name;
            existingCategory.ParentCategoryId = Category.ParentCategoryId;
            context.Categories.Update(existingCategory);
            AddPageSuccess("Category updated successfully.");
        }

        await context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        // Get all descendant categories
        var descendantIds = new HashSet<int>();
        await CollectDescendantIds(id, descendantIds);

        try
        {
            // Delete all descendants and the category itself
            var categoriesToDelete = await context.Categories
                .Where(c => descendantIds.Contains(c.Id) || c.Id == id)
                .ToListAsync();

            context.Categories.RemoveRange(categoriesToDelete);

            // Reorder remaining categories at the same level
            var siblingCategories = await context.Categories
                .Where(c => c.ParentCategoryId == category.ParentCategoryId && c.Id != id)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            for (int i = 0; i < siblingCategories.Count; i++)
            {
                siblingCategories[i].DisplayOrder = i;
            }

            await context.SaveChangesAsync();
            AddPageSuccess($"Category '{category.Name}' and its descendants were deleted successfully.");
        }
        catch (Exception ex)
        {
            AddPageError($"Error deleting category: {ex.Message}");
        }
        
        return RedirectToPage();
    }

    private async Task CollectDescendantIds(int categoryId, HashSet<int> descendantIds)
    {
        var children = await context.Categories
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var childId in children)
        {
            descendantIds.Add(childId);
            await CollectDescendantIds(childId, descendantIds);
        }
    }

    private List<CategoryViewModel> BuildCategoryTree(List<Models.Category> categories, int? parentId = null, int level = 0)
    {
        var result = new List<CategoryViewModel>();
        var children = categories.Where(c => c.ParentCategoryId == parentId).ToList();

        foreach (var child in children)
        {
            var viewModel = new CategoryViewModel
            {
                Id = child.Id,
                Name = child.Name,
                DisplayOrder = child.DisplayOrder,
                Level = level,
                ParentCategoryId = child.ParentCategoryId
            };
            result.Add(viewModel);
            result.AddRange(BuildCategoryTree(categories, child.Id, level + 1));
        }

        return result;
    }

    private async Task<bool> HasDescendant(int categoryId, int potentialDescendantId)
    {
        var descendants = await context.Categories
            .Where(c => c.ParentCategoryId == categoryId)
            .ToListAsync();

        foreach (var descendant in descendants)
        {
            if (descendant.Id == potentialDescendantId ||
                await HasDescendant(descendant.Id, potentialDescendantId))
            {
                return true;
            }
        }
        return false;
    }
}

public class CategoryViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public int? ParentCategoryId { get; set; }
}
