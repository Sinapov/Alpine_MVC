using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    
    public async Task OnGetAsync()
    {
        // Get featured products with images
        FeaturedProducts = await _context.Products
            .Where(p => p.IsFeatured)
            .Include(p => p.ProductImages)
            .OrderBy(p => p.Name)
            .Take(6)
            .ToListAsync();

        // If there aren't enough featured products, get the most recent ones to fill in
        if (FeaturedProducts.Count < 4)
        {
            var additionalProducts = await _context.Products
                .Where(p => !p.IsFeatured)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.Id)  // Assuming newer products have higher IDs
                .Take(6 - FeaturedProducts.Count)
                .ToListAsync();
                
            FeaturedProducts.AddRange(additionalProducts);
        }

        // Get main categories (those without parent)
        Categories = await _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
}