using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Product Product { get; set; } = default!;
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
        public int Quantity { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Get the requested product with its images and category
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;

            // Get related products (same category, excluding current product)
            RelatedProducts = await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == Product.CategoryId && p.Id != Product.Id)
                .OrderBy(p => Guid.NewGuid()) // Random order
                .Take(8) // Get up to 8 related products
                .ToListAsync();

            return Page();
        }
    }
}