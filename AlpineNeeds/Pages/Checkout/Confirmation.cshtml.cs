using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AlpineNeeds.Pages.Checkout
{
    public class ConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ConfirmationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string OrderNumber { get; set; } = string.Empty;
        public DateTime EstimatedDelivery { get; set; }
        public Order Order { get; set; }
        public string FormattedShippingAddress { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? orderId)
        {
            if (orderId == null)
            {
                // Prevent refresh resubmission or direct access
                return RedirectToPage("/Index");
            }

            // Load the order with all related data
            Order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (Order == null)
            {
                return NotFound();
            }

            // Format order info
            OrderNumber = $"{orderId:000000}";
            EstimatedDelivery = Order.OrderDate.AddDays(5);

            // Format shipping address
            if (Order.ShippingAddress != null)
            {
                var addressBuilder = new StringBuilder();
                addressBuilder.AppendLine(Order.ShippingAddress.FullName);
                addressBuilder.AppendLine(Order.ShippingAddress.AddressLine1);
                
                if (!string.IsNullOrEmpty(Order.ShippingAddress.AddressLine2))
                {
                    addressBuilder.AppendLine(Order.ShippingAddress.AddressLine2);
                }
                
                addressBuilder.AppendLine($"{Order.ShippingAddress.City}, {Order.ShippingAddress.State} {Order.ShippingAddress.ZipCode}");
                addressBuilder.AppendLine(Order.ShippingAddress.Country);
                
                if (!string.IsNullOrEmpty(Order.ShippingAddress.PhoneNumber))
                {
                    addressBuilder.AppendLine($"Phone: {Order.ShippingAddress.PhoneNumber}");
                }
                
                FormattedShippingAddress = addressBuilder.ToString();
            }

            return Page();
        }
    }
}
