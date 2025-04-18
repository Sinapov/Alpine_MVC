using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;

namespace AlpineNeeds.Pages.Admin.Category;

[Authorize(Roles = "Admin")]
public class ManageModel(ApplicationDbContext context, IStringLocalizer<ManageModel> localizer) : BasePageModel
{
    [BindProperty]
    public Models.Category? Category { get; set; }
    
    public SelectList ParentCategoryItems { get; set; } = new SelectList(Array.Empty<SelectListItem>());

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id.HasValue)
        {
            Category = await context.Categories.FindAsync(id.Value);
            if (Category == null)
            {
                return NotFound();
            }
            await LoadParentCategoryItems(id.Value);
        }
        else
        {
            await LoadParentCategoryItems(null);
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadParentCategoryItems(Category?.Id);
            return Page();
        }

        if (Category == null)
        {
            ModelState.AddModelError(string.Empty, localizer["Category information is missing."]);
            await LoadParentCategoryItems(null);
            return Page();
        }

        if (Category.ParentCategoryId.HasValue)
        {
            var parentExists = await context.Categories.AnyAsync(c => c.Id == Category.ParentCategoryId);
            if (!parentExists)
            {
                ModelState.AddModelError("Category.ParentCategoryId", localizer["Selected parent category does not exist."]);
                await LoadParentCategoryItems(Category.Id);
                return Page();
            }
            
            if (Category.Id != 0 && await HasDescendant(Category.Id, Category.ParentCategoryId.Value))
            {
                ModelState.AddModelError("Category.ParentCategoryId", localizer["Cannot make a category a child of its own descendant."]);
                await LoadParentCategoryItems(Category.Id);
                return Page();
            }
        }

        if (Category.Id == 0)
        {
            var maxOrder = await context.Categories
                .Where(c => c.ParentCategoryId == Category.ParentCategoryId)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? -1;
            Category.DisplayOrder = maxOrder + 1;
            
            await context.Categories.AddAsync(Category);
            await context.SaveChangesAsync();
            AddPageSuccess(localizer["Category created successfully."]);
        }
        else
        {
            var existingCategory = await context.Categories.FindAsync(Category.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = Category.Name;
            existingCategory.ParentCategoryId = Category.ParentCategoryId;
            
            try
            {
                await context.SaveChangesAsync();
                AddPageSuccess(localizer["Category updated successfully."]);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CategoryExists(Category.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }
        
        return RedirectToPage("./Index");
    }
    
    private async Task<bool> CategoryExists(int id)
    {
        return await context.Categories.AnyAsync(c => c.Id == id);
    }
    
    private async Task LoadParentCategoryItems(int? currentCategoryId)
    {
        var categories = currentCategoryId.HasValue
            ? await context.Categories
                .Where(c => c.Id != currentCategoryId)
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.DisplayOrder)
                .ToListAsync()
            : await context.Categories
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.DisplayOrder)
                .ToListAsync();
        
        var categoryList = BuildCategorySelectList(categories);
        ParentCategoryItems = new SelectList(categoryList, "Value", "Text");
    }

    private List<SelectListItem> BuildCategorySelectList(List<Models.Category> categories, int? parentId = null, string indent = "")
    {
        var items = new List<SelectListItem>();
        var children = categories.Where(c => c.ParentCategoryId == parentId).ToList();
        
        foreach (var child in children)
        {
            items.Add(new SelectListItem
            {
                Text = indent + child.Name,
                Value = child.Id.ToString()
            });
            
            items.AddRange(BuildCategorySelectList(categories, child.Id, indent + "-- "));
        }
        
        return items;
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