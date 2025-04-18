using AlpineNeeds.Models;
using AlpineNeeds.Services;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AlpineNeeds.Utilities;

namespace AlpineNeeds.Pages
{
    public class CartModel : BasePageModel
    {
        private readonly ICartService _cartService;

        public CartModel(ICartService cartService)
        {
            _cartService = cartService;
        }

        public List<CartItemDTO> CartItems { get; set; } = new();
        public decimal CartTotal { get; set; }
        public decimal ShippingCost { get; set; } = 4.99M;

        public async Task<IActionResult> OnGetAsync()
        {
            CartItems = await _cartService.GetCartAsync();
            CartTotal = await _cartService.GetCartTotalAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity = 1, string? color = null, string? size = null)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            try
            {
                var itemCount = await _cartService.AddToCartAsync(productId, quantity, color, size);
                
                // For AJAX requests, return JSON response
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true, count = itemCount, message = "Item added to cart" });
                }
                
                // Store success message in TempData (will be available for the next request)
                TempData["CartMessage"] = "Item added to cart";
                TempData["LastAddedProductId"] = productId;
                
                // Return to the same page the request came from, or to cart if referrer is not available
                if (!string.IsNullOrEmpty(Request.Headers["Referer"]))
                {
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // For AJAX requests, return JSON error
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = ex.Message });
                }
                
                // Store error message in TempData
                TempData["ErrorMessage"] = ex.Message;
                
                // Return to the same page the request came from
                if (!string.IsNullOrEmpty(Request.Headers["Referer"]))
                {
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, int quantity, string? color = null, string? size = null)
        {
            try
            {
                await _cartService.UpdateQuantityAsync(productId, quantity, color, size);
                
                // For AJAX requests, return JSON response
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cartTotal = await _cartService.GetCartTotalAsync();
                    var cartItemCount = await _cartService.GetCartItemCountAsync();
                    
                    return new JsonResult(new { 
                        success = true, 
                        total = cartTotal.ToBgCurrency(),
                        count = cartItemCount,
                        message = "Cart updated" 
                    });
                }
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // For AJAX requests, return JSON error
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = ex.Message });
                }
                
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int productId, string? color = null, string? size = null)
        {
            try
            {
                await _cartService.RemoveFromCartAsync(productId, color, size);
                
                AddPageSuccess("Item removed from cart successfully");
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                AddPageError(ex.Message);
                return RedirectToPage();
            }
        }
        
        public async Task<IActionResult> OnGetCartCountAsync()
        {
            var count = await _cartService.GetCartItemCountAsync();
            return new JsonResult(count);
        }
    }
}