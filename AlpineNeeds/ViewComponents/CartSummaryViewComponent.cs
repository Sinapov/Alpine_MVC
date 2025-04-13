using AlpineNeeds.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlpineNeeds.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartSummaryViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartItemCount = await _cartService.GetCartItemCountAsync();
            return View(cartItemCount);
        }
    }
}