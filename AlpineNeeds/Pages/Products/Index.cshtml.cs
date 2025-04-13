using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AlpineNeeds.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PaginatedList<Product> Products { get; set; } = default!;

        public List<Category> Categories { get; set; } = new();

        public List<string> Brands { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<int>? CategoryIds { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Brand { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(0, int.MaxValue)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(0, int.MaxValue)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StockFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public decimal LowestPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public int TotalProducts { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get all categories for filter
            Categories = await _context.Categories.ToListAsync();

            // Get all distinct brands for filter
            Brands = await _context.Products
                .Where(p => p.Brand != null)
                .Select(p => p.Brand!)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            // Start with all products
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm) ||
                                         p.Description != null && p.Description.Contains(SearchTerm) ||
                                         p.Brand != null && p.Brand.Contains(SearchTerm));
            }

            // Apply category filter if provided
            if (CategoryIds != null && CategoryIds.Any())
            {
                query = query.Where(p => CategoryIds.Contains(p.CategoryId));
            }

            // Apply brand filter if provided
            if (!string.IsNullOrEmpty(Brand) && Brand != "All")
            {
                query = query.Where(p => p.Brand == Brand);
            }

            // Apply price range filter if provided
            if (MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= MinPrice.Value);
            }

            if (MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= MaxPrice.Value);
            }

            // Apply stock filter if provided
            if (StockFilter == "InStock")
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            // Get price range for the slider
            LowestPrice = await _context.Products.MinAsync(p => p.Price);
            HighestPrice = await _context.Products.MaxAsync(p => p.Price);

            // Set default price range if not provided
            if (!MinPrice.HasValue)
            {
                MinPrice = LowestPrice;
            }

            if (!MaxPrice.HasValue)
            {
                MaxPrice = HighestPrice;
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(SortBy))
            {
                query = SortBy switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "newest" => query.OrderByDescending(p => p.Id),  // Assuming Id correlates with creation time
                    _ => query.OrderBy(p => p.Name)
                };
            }
            else
            {
                // Default sort by name
                query = query.OrderBy(p => p.Name);
            }

            // Set the page size
            const int pageSize = 24;

            // Count total products for display
            TotalProducts = await query.CountAsync();

            // Get paginated list of products
            Products = await PaginatedList<Product>.CreateAsync(
                query, PageIndex, pageSize, SortBy ?? "name", SortBy?.Contains("desc") == true ? "desc" : "asc");

            return Page();
        }

        public async Task<IActionResult> OnGetFilterAsync()
        {
            // This method is used for AJAX filtering
            // It returns partial view result with filtered products
            return await OnGetAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity = 1)
        {
            // Get the product
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            // TODO: Implement add to cart logic here
            // This would typically involve:
            // 1. Getting the current user
            // 2. Finding or creating their cart
            // 3. Adding the product to the cart or increasing quantity

            // Return JSON result for AJAX
            return new JsonResult(new { success = true, message = $"Added {product.Name} to cart" });
        }
    }
}