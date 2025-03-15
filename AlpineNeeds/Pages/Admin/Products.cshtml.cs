using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ProductsModel(ApplicationDbContext context) : BasePageModel
{
    private readonly int _pageSize = 10;

    public IList<Product> Items { get; set; } = new List<Product>();
    public IList<Models.Category> Categories { get; set; } = new List<Models.Category>();
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public string SortColumn { get; set; } = "name";
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    public async Task<IActionResult> OnGetAsync(int? pageNumber, string? sortColumn, string? sortOrder)
    {
        PageIndex = pageNumber ?? 1;
        if (!string.IsNullOrEmpty(sortColumn))
            SortColumn = sortColumn.ToLower();
        if (!string.IsNullOrEmpty(sortOrder))
            SortOrder = sortOrder.ToLower() == "desc" ? SortOrder.Descending : SortOrder.Ascending;

        var query = context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // Apply sorting
        query = SortColumn switch
        {
            "name" => SortOrder == SortOrder.Ascending 
                ? query.OrderBy(p => p.Name) 
                : query.OrderByDescending(p => p.Name),
            "price" => SortOrder == SortOrder.Ascending 
                ? query.OrderBy(p => p.Price) 
                : query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };

        var count = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(count / (double)_pageSize);

        // Ensure PageIndex is within valid range and handle empty result set
        if (TotalPages == 0)
        {
            TotalPages = 1;
            PageIndex = 1;
        }
        else if (PageIndex < 1)
        {
            PageIndex = 1;
        }
        else if (PageIndex > TotalPages)
        {
            PageIndex = TotalPages;
        }

        Items = await query
            .Skip((PageIndex - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync();

        Categories = await context.Categories.ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteProductAsync(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null)
        {
            AddPageError("Product not found.");
            return RedirectToPage();
        }

        try
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            AddPageSuccess("Product deleted successfully.");
        }
        catch (Exception ex)
        {
            AddPageError($"An error occurred while deleting the product: {ex.Message}");
        }

        return RedirectToPage();
    }
}

public enum SortOrder
{
    Ascending,
    Descending
}
