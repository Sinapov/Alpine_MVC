using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using AlpineNeeds.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AlpineNeeds.Pages.Account.Orders;

[Authorize]
public class DetailsModel : BasePageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;

    public DetailsModel(ApplicationDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public Order? Order { get; set; }
    public bool AllItemsAvailableForReorder { get; set; }
    public string? TrackingNumber { get; set; } = "ABC123456789"; // Example tracking number
    public DateTime? ShippingDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; } = 4.99M;
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public List<Order> UserOrders { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Get the current user ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Retrieve all orders for the current user (ordered by date desc)
        UserOrders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        // Retrieve the order with all related data
        Order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (Order == null)
        {
            return NotFound();
        }

        // Check if all items in the order are in stock for reordering
        AllItemsAvailableForReorder = true;
        foreach (var item in Order.OrderProducts)
        {
            if (item.Product.StockQuantity < item.Quantity)
            {
                AllItemsAvailableForReorder = false;
                break;
            }
        }

        // Set additional order information
        ShippingDate = Order.Status >= OrderStatus.Preparing ? Order.OrderDate.AddDays(1) : null;
        
        // Calculate order totals
        SubTotal = Order.OrderProducts.Sum(op => op.Price * op.Quantity);
        Tax = Math.Round(SubTotal * 0.2m, 2); // Example: 20% tax
        Total = Order.TotalPrice;
        Discount = SubTotal + ShippingCost + Tax - Total;

        return Page();
    }

    public async Task<IActionResult> OnPostReorderAsync(int id)
    {
        // Get the current user ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Retrieve the order with products
        var order = await _context.Orders
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        // Add each item to cart
        foreach (var item in order.OrderProducts)
        {
            // Skip items that are out of stock
            if (item.Product.StockQuantity < item.Quantity)
            {
                continue;
            }

            await _cartService.AddToCartAsync(
                item.ProductId,
                item.Quantity,
                item.Color,
                item.Size
            );
        }

        // Redirect to cart page
        return RedirectToPage("/Cart");
    }
}
