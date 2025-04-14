using System;
using System.Collections.Generic;
using AlpineNeeds.Models;
using AlpineNeeds.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages.Checkout
{
    public class ConfirmationModel : PageModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime EstimatedDelivery { get; set; }
        public int OrderId { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }

        public IActionResult OnGet(int? orderId)
        {
            if (orderId == null)
            {
                // Prevent refresh resubmission or direct access
                return RedirectToPage("/Index");
            }
            OrderId = orderId.Value;
            OrderNumber = $"{orderId:000000}";
            EstimatedDelivery = DateTime.UtcNow.AddDays(5);
            // TODO: Populate Items, ShippingAddress, OrderTotal from order
            return Page();
        }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
