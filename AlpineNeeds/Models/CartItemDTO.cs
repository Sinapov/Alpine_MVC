using System.ComponentModel.DataAnnotations;

namespace AlpineNeeds.Models
{
    // DTO for cart items to be stored in session for guest users
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        
        // Calculate subtotal for this item
        public decimal Subtotal => Price * Quantity;
    }
}