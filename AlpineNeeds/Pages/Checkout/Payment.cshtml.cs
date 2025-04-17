using System.Security.Claims;
using System.Threading.Tasks;
using AlpineNeeds.Models;
using AlpineNeeds.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages.Checkout
{
    [Authorize]
    public class PaymentModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICheckoutSessionService _checkoutSessionService;

        public PaymentModel(IOrderService orderService, ICheckoutSessionService checkoutSessionService)
        {
            _orderService = orderService;
            _checkoutSessionService = checkoutSessionService;
        }

        public void OnGet()
        {
            // Render payment form (not implemented here)
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get user ID
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Require login
            }

            // Get addresses from session
            var shipping = await _checkoutSessionService.GetShippingAddressAsync();
            var billing = await _checkoutSessionService.GetBillingAddressAsync();
            if (shipping == null || billing == null)
            {
                ModelState.AddModelError(string.Empty, "Shipping and billing information is required.");
                return Page();
            }

            // Place order
            int orderId = await _orderService.PlaceOrderAsync(userId, shipping, billing);

            // TODO: Send confirmation email (can be added here)

            // Redirect to confirmation page with orderId
            return RedirectToPage("/Checkout/Confirmation", new { orderId });
        }
    }
}
