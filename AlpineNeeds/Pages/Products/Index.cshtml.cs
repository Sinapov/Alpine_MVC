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
        
        // Hierarchical category structure
        public List<CategoryViewModel> CategoryHierarchy { get; set; } = new();

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
        
        // Dictionary to store all category IDs under each parent category
        private Dictionary<int, HashSet<int>> _categoryHierarchyMap = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get all categories for filter and include parent relationship
            Categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
            
            // Build category hierarchy
            BuildCategoryHierarchy();
            
            // Build category relationship map for filtering
            BuildCategoryHierarchyMap();

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

            // Apply category filter if provided, including sub-categories
            if (CategoryIds != null && CategoryIds.Any())
            {
                // Create a set of all category IDs to include (selected categories and their children)
                var categoriesToInclude = new HashSet<int>();
                
                foreach(var categoryId in CategoryIds)
                {
                    // Add the selected category itself
                    categoriesToInclude.Add(categoryId);
                    
                    // Add all child categories if this category exists in our hierarchy map
                    if (_categoryHierarchyMap.ContainsKey(categoryId))
                    {
                        foreach(var childId in _categoryHierarchyMap[categoryId])
                        {
                            categoriesToInclude.Add(childId);
                        }
                    }
                }
                
                query = query.Where(p => categoriesToInclude.Contains(p.CategoryId));
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

        // Build the hierarchical category structure for the view
        private void BuildCategoryHierarchy()
        {
            // First, find all root categories (those without parent)
            var rootCategories = Categories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name);

            foreach (var rootCategory in rootCategories)
            {
                var categoryViewModel = new CategoryViewModel
                {
                    Category = rootCategory,
                    Level = 0,
                    Children = GetChildCategories(rootCategory.Id, 1)
                };
                
                CategoryHierarchy.Add(categoryViewModel);
            }
        }

        // Recursively build subcategories
        private List<CategoryViewModel> GetChildCategories(int parentId, int level)
        {
            var children = Categories
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
                
            var childViewModels = new List<CategoryViewModel>();
            
            foreach (var child in children)
            {
                var childViewModel = new CategoryViewModel
                {
                    Category = child,
                    Level = level,
                    Children = GetChildCategories(child.Id, level + 1)
                };
                
                childViewModels.Add(childViewModel);
            }
            
            return childViewModels;
        }
        
        // Build a map of category IDs to all their descendant category IDs
        private void BuildCategoryHierarchyMap()
        {
            // Initialize the map for each category
            foreach (var category in Categories)
            {
                _categoryHierarchyMap[category.Id] = new HashSet<int>();
            }
            
            // For each category, find all its descendants
            foreach (var category in Categories)
            {
                if (category.ParentCategoryId.HasValue)
                {
                    // Add this category to its parent's descendants
                    AddCategoryToAncestors(category.Id, category.ParentCategoryId.Value);
                }
            }
        }
        
        // Recursively add a category to all its ancestors' descendant lists
        private void AddCategoryToAncestors(int categoryId, int ancestorId)
        {
            if (_categoryHierarchyMap.ContainsKey(ancestorId))
            {
                // Add this category to its ancestor's descendants
                _categoryHierarchyMap[ancestorId].Add(categoryId);
                
                // Find the ancestor's parent and continue recursively
                var ancestor = Categories.FirstOrDefault(c => c.Id == ancestorId);
                if (ancestor?.ParentCategoryId != null)
                {
                    AddCategoryToAncestors(categoryId, ancestor.ParentCategoryId.Value);
                }
            }
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
    
    // View model for hierarchical category display
    public class CategoryViewModel
    {
        public Category Category { get; set; } = default!;
        public int Level { get; set; }
        public List<CategoryViewModel> Children { get; set; } = new();
        public bool HasChildren => Children.Any();
    }
}