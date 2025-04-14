using System;
using System.Linq;
using System.Threading.Tasks;
using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AlpineNeeds.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(ApplicationDbContext db, ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> PlaceOrderAsync(string userId, CheckoutAddressViewModel shipping, CheckoutAddressViewModel billing)
        {
            var cart = await _cartService.GetCartAsync();
            if (cart == null || cart.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            var order = new Order
            {
                UserId = userId,
                TotalPrice = cart.Sum(i => i.Price * i.Quantity),
                Status = OrderStatus.Placed,
                OrderDate = DateTime.UtcNow,
                OrderProducts = cart.Select(i => new OrderProduct
                {
                    ProductId = i.ProductId,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Color = i.Color ?? string.Empty,
                    Size = i.Size ?? string.Empty
                }).ToList()
            };

            // Update inventory
            foreach (var item in order.OrderProducts)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Clear cart
            await _cartService.ClearCartAsync();

            return order.Id;
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
