using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace AlpineNeeds.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "ShoppingCart";

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        
        private string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get current user's cart (from session for guests, from DB for logged-in users)
        public async Task<List<CartItemDTO>> GetCartAsync()
        {
            if (IsAuthenticated && !string.IsNullOrEmpty(UserId))
            {
                return await GetDatabaseCartAsync();
            }
            else
            {
                return GetSessionCart();
            }
        }

        // Add an item to the cart
        public async Task<int> AddToCartAsync(int productId, int quantity, string? color = null, string? size = null)
        {
            // First, verify the product exists and get its data
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (IsAuthenticated && !string.IsNullOrEmpty(UserId))
            {
                return await AddToDatabaseCartAsync(product, quantity, color, size);
            }
            else
            {
                return AddToSessionCart(product, quantity, color, size);
            }
        }

        // Update an item's quantity in the cart
        public async Task UpdateQuantityAsync(int productId, int quantity, string? color = null, string? size = null)
        {
            if (quantity <= 0)
            {
                await RemoveFromCartAsync(productId, color, size);
                return;
            }

            if (IsAuthenticated && !string.IsNullOrEmpty(UserId))
            {
                await UpdateDatabaseQuantityAsync(productId, quantity, color, size);
            }
            else
            {
                UpdateSessionQuantity(productId, quantity, color, size);
            }
        }

        // Remove an item from the cart
        public async Task RemoveFromCartAsync(int productId, string? color = null, string? size = null)
        {
            if (IsAuthenticated && !string.IsNullOrEmpty(UserId))
            {
                await RemoveFromDatabaseCartAsync(productId, color, size);
            }
            else
            {
                RemoveFromSessionCart(productId, color, size);
            }
        }

        // Clear the entire cart
        public async Task ClearCartAsync()
        {
            if (IsAuthenticated && !string.IsNullOrEmpty(UserId))
            {
                await ClearDatabaseCartAsync();
            }
            else
            {
                ClearSessionCart();
            }
        }

        // Get the total number of items in the cart
        public async Task<int> GetCartItemCountAsync()
        {
            var cartItems = await GetCartAsync();
            return cartItems.Sum(i => i.Quantity);
        }

        // Get the total price of all items in the cart
        public async Task<decimal> GetCartTotalAsync()
        {
            var cartItems = await GetCartAsync();
            return cartItems.Sum(i => i.Price * i.Quantity);
        }

        // Merge a guest user's session cart into their database cart upon login
        public async Task MergeCartAsync()
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(UserId))
            {
                return; // Nothing to merge if not authenticated
            }

            var sessionCart = GetSessionCart();
            if (sessionCart.Count == 0)
            {
                return; // Nothing to merge
            }

            foreach (var item in sessionCart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    await AddToDatabaseCartAsync(product, item.Quantity, item.Color, item.Size);
                }
            }

            // Clear the session cart after merging
            ClearSessionCart();
        }

        #region Private Helper Methods

        // --- Session cart operations (for guests) ---

        private List<CartItemDTO> GetSessionCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            string cartJson = session?.GetString(CartSessionKey) ?? "";
            
            return string.IsNullOrEmpty(cartJson) 
                ? new List<CartItemDTO>() 
                : JsonSerializer.Deserialize<List<CartItemDTO>>(cartJson) ?? new List<CartItemDTO>();
        }

        private int AddToSessionCart(Product product, int quantity, string? color = null, string? size = null)
        {
            var cart = GetSessionCart();
            
            // Check if item already exists in the cart with the same attributes
            var existingItem = cart.FirstOrDefault(i => i.ProductId == product.Id && 
                                                       i.Color == color && 
                                                       i.Size == size);
            
            if (existingItem != null)
            {
                // Update quantity of existing item
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add new item to cart
                var imageUrl = product.ProductImages.FirstOrDefault()?.ImageUrl ?? "";
                
                cart.Add(new CartItemDTO
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = imageUrl,
                    Price = product.Price,
                    Quantity = quantity,
                    Color = color,
                    Size = size
                });
            }
            
            // Save cart back to session
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
            
            return cart.Sum(i => i.Quantity);
        }

        private void UpdateSessionQuantity(int productId, int quantity, string? color = null, string? size = null)
        {
            var cart = GetSessionCart();
            
            var item = cart.FirstOrDefault(i => i.ProductId == productId && 
                                              i.Color == color && 
                                              i.Size == size);
            
            if (item != null)
            {
                item.Quantity = quantity;
                
                // Save cart back to session
                var session = _httpContextAccessor.HttpContext?.Session;
                session?.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
            }
        }

        private void RemoveFromSessionCart(int productId, string? color = null, string? size = null)
        {
            var cart = GetSessionCart();
            
            var item = cart.FirstOrDefault(i => i.ProductId == productId && 
                                              i.Color == color && 
                                              i.Size == size);
            
            if (item != null)
            {
                cart.Remove(item);
                
                // Save cart back to session
                var session = _httpContextAccessor.HttpContext?.Session;
                session?.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
            }
        }

        private void ClearSessionCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(CartSessionKey);
        }

        // --- Database cart operations (for logged-in users) ---

        private async Task<List<CartItemDTO>> GetDatabaseCartAsync()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return new List<CartItemDTO>();
            }

            // Find user's cart or create a new one
            var cart = await GetOrCreateCartAsync();

            // Return the cart items as DTOs
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(ci => ci.CartId == cart.Id)
                .Select(ci => new CartItemDTO
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ImageUrl = ci.Product.ProductImages.FirstOrDefault().ImageUrl,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    Color = ci.Color,
                    Size = ci.Size
                })
                .ToListAsync();

            return cartItems;
        }

        private async Task<int> AddToDatabaseCartAsync(Product product, int quantity, string? color = null, string? size = null)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                throw new Exception("User not authenticated");
            }

            // Find user's cart or create a new one
            var cart = await GetOrCreateCartAsync();

            // Check if the item already exists in the cart with the same attributes
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && 
                                          ci.ProductId == product.Id && 
                                          ci.Color == color && 
                                          ci.Size == size);

            if (existingItem != null)
            {
                // Update quantity of existing item
                existingItem.Quantity += quantity;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Add new item to cart
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    Color = color,
                    Size = size,
                    Cart = cart,
                    Product = product
                };

                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
            }

            // Return the total number of items in the cart
            return await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .SumAsync(ci => ci.Quantity);
        }

        private async Task UpdateDatabaseQuantityAsync(int productId, int quantity, string? color = null, string? size = null)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return;
            }

            // Find user's cart
            var cart = await GetOrCreateCartAsync();

            // Find the cart item
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && 
                                          ci.ProductId == productId &&
                                          ci.Color == color && 
                                          ci.Size == size);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
        }

        private async Task RemoveFromDatabaseCartAsync(int productId, string? color = null, string? size = null)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return;
            }

            // Find user's cart
            var cart = await GetOrCreateCartAsync();

            // Find the cart item
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && 
                                          ci.ProductId == productId &&
                                          ci.Color == color && 
                                          ci.Size == size);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ClearDatabaseCartAsync()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return;
            }

            // Find user's cart
            var cart = await GetOrCreateCartAsync();

            // Remove all items from the cart
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        private async Task<Cart> GetOrCreateCartAsync()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                throw new Exception("User not authenticated");
            }

            // Try to find the user's cart
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == UserId);

            // If cart doesn't exist, create a new one
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = UserId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        #endregion
    }
}