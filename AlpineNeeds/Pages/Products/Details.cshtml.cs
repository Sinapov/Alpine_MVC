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

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity)
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