using AlpineNeeds.Models;

namespace AlpineNeeds.Services
{
    public interface ICartService
    {
        // Get current user's cart (from session for guests, from DB for logged-in users)
        Task<List<CartItemDTO>> GetCartAsync();
        
        // Add an item to the cart
        Task<int> AddToCartAsync(int productId, int quantity, string? color = null, string? size = null);
        
        // Update an item's quantity in the cart
        Task UpdateQuantityAsync(int productId, int quantity, string? color = null, string? size = null);
        
        // Remove an item from the cart
        Task RemoveFromCartAsync(int productId, string? color = null, string? size = null);
        
        // Clear the entire cart
        Task ClearCartAsync();
        
        // Get the total number of items in the cart
        Task<int> GetCartItemCountAsync();
        
        // Get the total price of all items in the cart
        Task<decimal> GetCartTotalAsync();
        
        // Merge a guest user's session cart into their database cart upon login
        Task MergeCartAsync();
    }
}