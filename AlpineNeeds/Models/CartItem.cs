using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlpineNeeds.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        public int CartId { get; set; }
        
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }
        
        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        
        public int Quantity { get; set; }
        
        // Product Attributes
        public string Color { get; set; }
        public string Size { get; set; }
    }
}
