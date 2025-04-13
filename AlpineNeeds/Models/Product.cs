using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlpineNeeds.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Product Name")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Product Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between {1} and {2}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // Collections for Colors and Sizes
        [Display(Name = "Available Colors")]
        public List<string> Colors { get; set; } = new();

        [Display(Name = "Available Sizes")]
        public List<string> Sizes { get; set; } = new();

        // Collection of product images
        [Display(Name = "Product Images")]
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        [Required]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;

        [Display(Name = "In Stock")]
        public bool InStock => StockQuantity > 0;

        [StringLength(100)]
        [Display(Name = "Brand")]
        public string? Brand { get; set; }
    }
}
