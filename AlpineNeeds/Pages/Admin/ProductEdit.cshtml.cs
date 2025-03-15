using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ProductEditModel(ApplicationDbContext context) : BasePageModel
{

    [BindProperty]
    public Product Product { get; set; } = null!;


    public List<Models.Category> Categories { get; set; } = new List<Models.Category>();


    public bool IsNewProduct => Product?.Id == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Categories = await context.Categories.ToListAsync();

        if (!Categories.Any())
        {
            AddPageError("Please create at least one category before adding products.");
            return RedirectToPage("./Products");
        }

        if (id.HasValue)
        {
            var product = await context.Products
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
            Categories = await context.Categories.ToListAsync();
            return Page();
        }

        try
        {
            Product.Colors ??= new List<string>();
            Product.Sizes ??= new List<string>();
            
            // Ensure Category is loaded
            Product.Category = await context.Categories.FindAsync(Product.CategoryId) 
                ?? throw new InvalidOperationException("Selected category not found");

            if (Product.Id == 0)
            {
                context.Products.Add(Product);
            }
            else
            {
                var existingProduct = await context.Products
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

                context.Products.Update(existingProduct);
            }

            await context.SaveChangesAsync();
            AddPageSuccess($"Product {(Product.Id == 0 ? "created" : "updated")} successfully.");
            return RedirectToPage("./Products");
        }
        catch (Exception ex)
        {
            AddPageError($"Error {(Product.Id == 0 ? "creating" : "updating")} product: {ex.Message}");
            Categories = await context.Categories.ToListAsync();
            return Page();
        }
    }
}
