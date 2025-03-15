using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlpineNeeds.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string ImageUrl { get; set; }
        
        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public required virtual Product Product { get; set; }
    }
}
