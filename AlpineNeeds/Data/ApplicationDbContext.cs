using AlpineNeeds.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using AlpineNeeds.Options;
using AlpineNeeds.Utilities;

namespace AlpineNeeds.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Main categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Облекло", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Обувки / Гети", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Трекинг", DisplayOrder = 3 },
                new Category { Id = 4, Name = "Алпинизъм / Катерене", DisplayOrder = 4 }
            );
            
            // Clothing subcategories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 5, Name = "Якета мембрана", ParentCategoryId = 1, DisplayOrder = 1 },
                new Category { Id = 6, Name = "Якета пух / Primaloft", ParentCategoryId = 1, DisplayOrder = 2 },
                new Category { Id = 7, Name = "Якета софтшел", ParentCategoryId = 1, DisplayOrder = 3 },
                new Category { Id = 8, Name = "Полари / Флийс", ParentCategoryId = 1, DisplayOrder = 4 },
                new Category { Id = 9, Name = "Панталони мембрана", ParentCategoryId = 1, DisplayOrder = 5 },
                new Category { Id = 10, Name = "Панталони", ParentCategoryId = 1, DisplayOrder = 6 },
                new Category { Id = 11, Name = "Термо бельо", ParentCategoryId = 1, DisplayOrder = 7 },
                new Category { Id = 12, Name = "Тениски / Блузи", ParentCategoryId = 1, DisplayOrder = 8 },
                new Category { Id = 13, Name = "Чорапи", ParentCategoryId = 1, DisplayOrder = 9 },
                new Category { Id = 14, Name = "Ръкавици", ParentCategoryId = 1, DisplayOrder = 10 },
                new Category { Id = 15, Name = "Шапки / Шалове", ParentCategoryId = 1, DisplayOrder = 11 },
                new Category { Id = 16, Name = "Дъждобрани", ParentCategoryId = 1, DisplayOrder = 12 },
                new Category { Id = 17, Name = "Аксесоари", ParentCategoryId = 1, DisplayOrder = 13 }
            );
            
            // Footwear subcategories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 18, Name = "Високи", ParentCategoryId = 2, DisplayOrder = 1 },
                new Category { Id = 19, Name = "Средни", ParentCategoryId = 2, DisplayOrder = 2 },
                new Category { Id = 20, Name = "Ниски", ParentCategoryId = 2, DisplayOrder = 3 }
            );
            
            // Trekking subcategories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 21, Name = "Осветление", ParentCategoryId = 3, DisplayOrder = 1 },
                new Category { Id = 22, Name = "Палатки", ParentCategoryId = 3, DisplayOrder = 2 },
                new Category { Id = 23, Name = "Посуда / Примуси", ParentCategoryId = 3, DisplayOrder = 3 },
                new Category { Id = 24, Name = "Раници / Чанти", ParentCategoryId = 3, DisplayOrder = 4 },
                new Category { Id = 25, Name = "Спални чували", ParentCategoryId = 3, DisplayOrder = 5 },
                new Category { Id = 26, Name = "Шалтета / Постелки", ParentCategoryId = 3, DisplayOrder = 6 },
                new Category { Id = 27, Name = "Щеки", ParentCategoryId = 3, DisplayOrder = 7 },
                new Category { Id = 28, Name = "Снегоходки / Котки", ParentCategoryId = 3, DisplayOrder = 8 },
                new Category { Id = 29, Name = "Гети / Дъждобрани", ParentCategoryId = 3, DisplayOrder = 9 },
                new Category { Id = 30, Name = "Трекинг аксесоари", ParentCategoryId = 3, DisplayOrder = 10 }
            );
            
            // Climbing subcategories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 31, Name = "Въжета", ParentCategoryId = 4, DisplayOrder = 1 },
                new Category { Id = 32, Name = "Седалки", ParentCategoryId = 4, DisplayOrder = 2 },
                new Category { Id = 33, Name = "Еспадрили", ParentCategoryId = 4, DisplayOrder = 3 },
                new Category { Id = 34, Name = "Карабинери / Примки", ParentCategoryId = 4, DisplayOrder = 4 },
                new Category { Id = 35, Name = "Съоръжения от лента", ParentCategoryId = 4, DisplayOrder = 5 },
                new Category { Id = 36, Name = "Каски", ParentCategoryId = 4, DisplayOrder = 6 },
                new Category { Id = 37, Name = "Клеми / Френдове", ParentCategoryId = 4, DisplayOrder = 7 },
                new Category { Id = 38, Name = "Магнезий / Торбички Mg", ParentCategoryId = 4, DisplayOrder = 8 },
                new Category { Id = 39, Name = "Осигуряващи съоръжения", ParentCategoryId = 4, DisplayOrder = 9 },
                new Category { Id = 40, Name = "Клинове скални / ледени", ParentCategoryId = 4, DisplayOrder = 10 },
                new Category { Id = 41, Name = "Ледокопи / Котки", ParentCategoryId = 4, DisplayOrder = 11 },
                new Category { Id = 42, Name = "Виа Ферата", ParentCategoryId = 4, DisplayOrder = 12 },
                new Category { Id = 43, Name = "Аксесоари", ParentCategoryId = 4, DisplayOrder = 13 }
            );

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany()
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // New method for seeding roles and admin user
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminCredentials>>().Value;

                // Create roles if they don't exist
                string[] roleNames = { "Admin", "User" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Create default admin if it doesn't exist
                if (adminOptions.Email != null && await userManager.FindByEmailAsync(adminOptions.Email) == null)
                {
                    var admin = new ApplicationUser
                    {
                        UserName = adminOptions.Email,
                        Email = adminOptions.Email,
                        EmailConfirmed = true,
                        FirstName = "Admin",
                        LastName = "User"
                    };

                    if (adminOptions.Password != null)
                    {
                        var result = await userManager.CreateAsync(admin, adminOptions.Password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(admin, "Admin");
                        }
                    }
                }
            }
        }
        // Method for seeding products and their images from Excel file
        public static async Task SeedProductsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // Check if products already exist
            if (await dbContext.Products.AnyAsync())
            {
                return; // Database already has products
            }

            try
            {
                // Read products from Excel file
                var categories = await dbContext.Categories.ToListAsync();
                var products = ExcelUtility.ReadProductsFromExcel(webHostEnvironment, categories);

                if (products != null && products.Any())
                {
                    // Add the products to the database
                    foreach (var product in products)
                    {
                        // Extract image information
                        var imageUrls = product.ProductImages.ToList();

                        // Clear the images before saving the product
                        product.ProductImages.Clear();

                        // Add the product to the database
                        await dbContext.Products.AddAsync(product);
                        await dbContext.SaveChangesAsync();

                        // Add images for the product
                        if (imageUrls.Any())
                        {
                            foreach (var imageUrl in imageUrls)
                            {
                                await dbContext.ProductImages.AddAsync(new ProductImage
                                {
                                    ImageUrl = imageUrl.ImageUrl,
                                    ProductId = product.Id
                                });
                            }

                            await dbContext.SaveChangesAsync();
                        }
                    }

                    Console.WriteLine($"Successfully seeded {products.Count} products from Excel file");
                }
                else
                {
                    Console.WriteLine("No products found in Excel file or file could not be processed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding products from Excel: {ex.Message}");
            }
        }
    }
}
