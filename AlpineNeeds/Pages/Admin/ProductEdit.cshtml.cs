using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ProductEditModel : BasePageModel
{
    private readonly ApplicationDbContext _context;

    public ProductEditModel(ApplicationDbContext context)
    {
        _context = context;
        Categories = new List<Models.Category>();
    }

    [BindProperty]
    public Product Product { get; set; } = null!;
    
    public List<Models.Category> Categories { get; set; }
    
    public bool IsNewProduct => Product?.Id == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Categories = await _context.Categories.ToListAsync();

        if (!Categories.Any())
        {
            AddPageError("Please create at least one category before adding products.");
            return RedirectToPage("./Products");
        }

        if (id.HasValue)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;
        }
        else
        {
            Product = new Product
            {
                Name = string.Empty,
                Description = string.Empty,
                Price = 0,
                Colors = new List<string>(),
                Sizes = new List<string>(),
                Category = Categories.First(),
                CategoryId = Categories.First().Id
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _context.Categories.ToListAsync();
            return Page();
        }

        try
        {
            Product.Colors ??= new List<string>();
            Product.Sizes ??= new List<string>();
            
            // Ensure Category is loaded
            Product.Category = await _context.Categories.FindAsync(Product.CategoryId) 
                ?? throw new InvalidOperationException("Selected category not found");

            if (Product.Id == 0)
            {
                _context.Products.Add(Product);
            }
            else
            {
                var existingProduct = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == Product.Id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = Product.Name;
                existingProduct.Description = Product.Description;
                existingProduct.CategoryId = Product.CategoryId;
                existingProduct.Category = Product.Category;
                existingProduct.Price = Product.Price;
                existingProduct.Colors = Product.Colors;
                existingProduct.Sizes = Product.Sizes;

                _context.Products.Update(existingProduct);
            }

            await _context.SaveChangesAsync();
            AddPageSuccess($"Product {(Product.Id == 0 ? "created" : "updated")} successfully.");
            return RedirectToPage("./Products");
        }
        catch (Exception ex)
        {
            AddPageError($"Error {(Product.Id == 0 ? "creating" : "updating")} product: {ex.Message}");
            Categories = await _context.Categories.ToListAsync();
            return Page();
        }
    }
}
