using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ProductEditModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) : BasePageModel
{
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    [BindProperty]
    public Product Product { get; set; } = null!;

    [BindProperty]
    public List<IFormFile> ProductImages { get; set; } = [];

    [BindProperty]
    public List<int> ImageIdsToDelete { get; set; } = [];

    [BindProperty]
    public List<int> ImageDisplayOrder { get; set; } = [];

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
                .Include(p => p.ProductImages)
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
                await context.SaveChangesAsync(); // Save to get the Product ID for images
            }
            else
            {
                var existingProduct = await context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
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

                // Handle image deletions
                if (ImageIdsToDelete.Count > 0)
                {
                    var imagesToDelete = existingProduct.ProductImages
                        .Where(img => ImageIdsToDelete.Contains(img.Id))
                        .ToList();

                    foreach (var image in imagesToDelete)
                    {
                        // Delete the image file if it exists
                        DeleteImageFile(image.ImageUrl);
                        existingProduct.ProductImages.Remove(image);
                        context.ProductImages.Remove(image);
                    }
                }

                // Reorder images based on the display order
                if (ImageDisplayOrder.Count > 0)
                {
                    var imageDictionary = existingProduct.ProductImages.ToDictionary(img => img.Id);
                    var orderedImages = new List<ProductImage>();
                    
                    foreach (var imageId in ImageDisplayOrder)
                    {
                        if (imageDictionary.TryGetValue(imageId, out ProductImage? image))
                        {
                            orderedImages.Add(image);
                        }
                    }
                    
                    existingProduct.ProductImages = orderedImages;
                }

                Product = existingProduct;
            }

            // Handle new image uploads
            if (ProductImages.Count > 0)
            {
                await ProcessUploadedImages();
            }

            await context.SaveChangesAsync();
            AddPageSuccess($"Product {(IsNewProduct ? "created" : "updated")} successfully.");
            return RedirectToPage("./Products");
        }
        catch (Exception ex)
        {
            AddPageError($"Error {(Product.Id == 0 ? "creating" : "updating")} product: {ex.Message}");
            Categories = await context.Categories.ToListAsync();
            return Page();
        }
    }

    private async Task ProcessUploadedImages()
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
        
        // Create the directory if it doesn't exist
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        foreach (var imageFile in ProductImages)
        {
            if (imageFile.Length > 0)
            {
                // Create a unique file name
                string uniqueFileName = $"{Product.Id}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Add to database
                var productImage = new ProductImage
                {
                    ImageUrl = $"/images/products/{uniqueFileName}",
                    ProductId = Product.Id,
                    Product = Product
                };

                Product.ProductImages.Add(productImage);
            }
        }
    }

    private void DeleteImageFile(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            // Convert URL to file path
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        catch
        {
            // Log error but don't throw exception to continue with db operations
        }
    }
}
