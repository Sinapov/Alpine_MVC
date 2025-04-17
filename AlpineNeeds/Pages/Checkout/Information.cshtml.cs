using System.Linq;
using System.Threading.Tasks;
using AlpineNeeds.Models;
using AlpineNeeds.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages.Checkout
{
    [Authorize]
    public class InformationModel : PageModel
    {
        private readonly ICheckoutSessionService _checkoutSessionService;
        private readonly ICartService _cartService;

        [BindProperty]
        public CheckoutAddressViewModel ShippingAddress { get; set; } = new();

        [BindProperty]
        public CheckoutAddressViewModel? BillingAddress { get; set; } = new();

        [BindProperty]
        public bool SameAsShipping { get; set; } = true;

        public OrderSummaryViewModel OrderSummary { get; set; } = new();

        public InformationModel(ICheckoutSessionService checkoutSessionService, ICartService cartService)
        {
            _checkoutSessionService = checkoutSessionService;
            _cartService = cartService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var savedShipping = await _checkoutSessionService.GetShippingAddressAsync();
            if (savedShipping != null)
            {
                ShippingAddress = savedShipping;
            }
            var savedBilling = await _checkoutSessionService.GetBillingAddressAsync();
            if (savedBilling != null)
            {
                BillingAddress = savedBilling;
                SameAsShipping = false;
            }
            // Load order summary from cart
            var cartItems = await _cartService.GetCartAsync();
            OrderSummary = new OrderSummaryViewModel
            {
                Subtotal = cartItems.Sum(i => i.Price * i.Quantity),
                Shipping = cartItems.Count > 0 ? 10 : 0, // Example flat shipping
                Tax = cartItems.Sum(i => i.Price * i.Quantity) * 0.2m, // Example 20% tax
            };
            OrderSummary.Total = OrderSummary.Subtotal + OrderSummary.Shipping + OrderSummary.Tax;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload order summary
                var cartItems = await _cartService.GetCartAsync();
                OrderSummary = new OrderSummaryViewModel
                {
                    Subtotal = cartItems.Sum(i => i.Price * i.Quantity),
                    Shipping = cartItems.Count > 0 ? 10 : 0,
                    Tax = cartItems.Sum(i => i.Price * i.Quantity) * 0.2m,
                };
                OrderSummary.Total = OrderSummary.Subtotal + OrderSummary.Shipping + OrderSummary.Tax;
                return Page();
            }
            await _checkoutSessionService.SaveShippingAddressAsync(ShippingAddress);
            if (!SameAsShipping)
            {
                await _checkoutSessionService.SaveBillingAddressAsync(BillingAddress!);
            }
            else
            {
                await _checkoutSessionService.SaveBillingAddressAsync(ShippingAddress);
            }
            return RedirectToPage("/Checkout/Payment");
        }
    }
}
