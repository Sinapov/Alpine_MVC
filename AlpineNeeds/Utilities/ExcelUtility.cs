using AlpineNeeds.Models;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AlpineNeeds.Utilities
{
    public static class ExcelUtility
    {
        public static List<Product> ReadProductsFromExcel(IWebHostEnvironment webHostEnvironment, List<Category> categories)
        {
            var products = new List<Product>();
            string excelPath = Path.Combine(webHostEnvironment.ContentRootPath, "Data", "ProductSeedData.xlsx");

            // Check if the Excel file exists
            if (!File.Exists(excelPath))
            {
                throw new FileNotFoundException($"Product seed data Excel file not found at {excelPath}");
            }

            using (var workbook = new XLWorkbook(excelPath))
            {
                var worksheet = workbook.Worksheet(1); // Assuming data is in first worksheet
                var rows = worksheet.RowsUsed();

                // Skip header row
                bool isFirstRow = true;
                
                foreach (var row in rows)
                {
                    if (isFirstRow)
                    {
                        isFirstRow = false;
                        continue;
                    }

                    // Parse product data from Excel cells
                    string name = row.Cell(1).GetString();
                    string description = row.Cell(2).GetString();
                    decimal price = decimal.Parse(row.Cell(3).GetString());
                    string categoryName = row.Cell(4).GetString();
                    string colors = row.Cell(5).GetString();
                    string sizes = row.Cell(6).GetString();
                    int stockQuantity = int.Parse(row.Cell(7).GetString());
                    string brand = row.Cell(8).GetString();
                    bool isFeatured = bool.Parse(row.Cell(9).GetString());
                    string imageUrls = row.Cell(10).GetString();


                    var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                    int categoryId = category != null ? category.Id : 0;

                    // Create product
                    var product = new Product
                    {
                        Name = name,
                        Description = description,
                        Price = price,
                        CategoryId = categoryId,
                        Colors = ParseStringToList(colors),
                        Sizes = ParseStringToList(sizes),
                        StockQuantity = stockQuantity,
                        Brand = brand,
                        IsFeatured = isFeatured,
                        ProductImages = new List<ProductImage>()
                    };

                    // Add image URLs to product
                    if (!string.IsNullOrWhiteSpace(imageUrls))
                    {
                        foreach (var url in ParseStringToList(imageUrls))
                        {
                            product.ProductImages.Add(new ProductImage { ImageUrl = $"/images/products/{url}" });
                        }
                    }

                    products.Add(product);
                }
            }

            return products;
        }

        private static List<string> ParseStringToList(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new List<string>();
            }

            // Split by comma and trim spaces
            return input.Split(';')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
    }
}
