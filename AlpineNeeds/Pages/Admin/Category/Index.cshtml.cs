using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AlpineNeeds.Pages.Admin.Category;

[Authorize(Roles = "Admin")]
public class IndexModel(ApplicationDbContext context, IStringLocalizer<IndexModel> localizer) : BasePageModel
{
    public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

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
        AddPageSuccess(localizer["Category order updated successfully."]);
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound();
        
        var descendantIds = new HashSet<int>();
        await CollectDescendantIds(id, descendantIds);
        try
        {
            var categoriesToDelete = await context.Categories
                .Where(c => descendantIds.Contains(c.Id) || c.Id == id)
                .ToListAsync();
            context.Categories.RemoveRange(categoriesToDelete);
            
            var siblingCategories = await context.Categories
                .Where(c => c.ParentCategoryId == category.ParentCategoryId && c.Id != id)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            for (int i = 0; i < siblingCategories.Count; i++)
            {
                siblingCategories[i].DisplayOrder = i;
            }
            await context.SaveChangesAsync();
            AddPageSuccess(localizer["Category '{0}' and its descendants were deleted successfully.", category.Name]);
        }
        catch (Exception ex)
        {
            AddPageError(localizer["Error deleting category: {0}", ex.Message]);
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
}

public class CategoryViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public int? ParentCategoryId { get; set; }
}