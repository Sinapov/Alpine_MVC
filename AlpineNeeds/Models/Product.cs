using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AlpineNeeds.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Name { get; set; }
        
        public string? Description { get; set; }
        
        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        // Collections for Colors and Sizes
        public List<string> Colors { get; set; } = new();
        public List<string> Sizes { get; set; } = new();
    }
}
