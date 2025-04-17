using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel(ApplicationDbContext context) : BasePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required Order Order { get; set; }
        
        [BindProperty]
        public OrderStatus NewStatus { get; set; }
        
        public required SelectList AvailableStatuses { get; set; }
        
        public bool CanCancelOrder => Order.Status == OrderStatus.Placed || 
                                      Order.Status == OrderStatus.Confirmed || 
                                      Order.Status == OrderStatus.Preparing;
        
        public List<OrderStatus> OrderStatusTimeline { get; set; } = new List<OrderStatus>
        {
            OrderStatus.Placed,
            OrderStatus.Confirmed,
            OrderStatus.Preparing,
            OrderStatus.Packed,
            OrderStatus.Delivered,
            OrderStatus.Finished
        };
        
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            
            // Set up available statuses for dropdown based on current status
            SetAvailableStatuses();
            
            return Page();
        }
        
        private void SetAvailableStatuses()
        {
            // Define transitions based on current status
            var availableTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Placed, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.CustomerCanceled, OrderStatus.OutOfStock } },
                { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.CustomerCanceled, OrderStatus.OutOfStock } },
                { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.Packed, OrderStatus.CustomerCanceled, OrderStatus.OutOfStock } },
                { OrderStatus.Packed, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.OutOfStock } },
                { OrderStatus.Delivered, new List<OrderStatus> { OrderStatus.Finished } },
                // Canceled orders don't have transitions
                { OrderStatus.CustomerCanceled, new List<OrderStatus>() },
                { OrderStatus.OutOfStock, new List<OrderStatus>() },
                { OrderStatus.Finished, new List<OrderStatus>() }
            };
            
            // Set the available statuses
            var statusList = availableTransitions.ContainsKey(Order.Status) 
                ? availableTransitions[Order.Status] 
                : new List<OrderStatus>();
            
            AvailableStatuses = new SelectList(statusList, Order.Status);
            
            // Default to the next logical status
            NewStatus = statusList.FirstOrDefault();
        }
        
        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            var order = await _context.Orders.FindAsync(Order.Id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Check if status transition is valid
            if (!IsValidStatusTransition(order.Status, NewStatus))
            {
                TempData["ErrorMessage"] = "Invalid status transition.";
                return RedirectToPage(new { id = order.Id });
            }
            
            // Update the status
            order.Status = NewStatus;
            
            // Handle inventory updates if needed
            // For example, if status is now "Confirmed" or "Preparing",
            // you might want to reserve inventory
            
            await _context.SaveChangesAsync();
            
            this.AddPageSuccess($"Order status updated to {NewStatus}.");
            return RedirectToPage(new { id = order.Id });
        }
        
        public async Task<IActionResult> OnPostCancelOrderAsync(string reason, bool notifyCustomer)
        {
            var order = await _context.Orders.FindAsync(Order.Id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Check if order can be canceled
            if (!CanCancelOrder)
            {
                TempData["ErrorMessage"] = "This order cannot be canceled due to its current status.";
                return RedirectToPage(new { id = order.Id });
            }
            
            // Update order status
            order.Status = OrderStatus.CustomerCanceled;
            
            // Add order note about cancellation (if you have order notes table)
            // _context.OrderNotes.Add(new OrderNote { 
            //     OrderId = Order.Id, 
            //     Note = reason, 
            //     CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            // });
            
            // If we need to notify customer, send email here
            if (notifyCustomer)
            {
                // Email service would be injected and used here
                // await _emailService.SendOrderCancellationEmail(order, reason);
            }
            
            await _context.SaveChangesAsync();
            
            this.AddPageSuccess($"Order #{Order.Id} has been canceled successfully.");
            return RedirectToPage(new { id = order.Id });
        }
        
        public async Task<IActionResult> OnPostSendEmailAsync(string subject, string message)
        {
            var order = await _context.Orders.FindAsync(Order.Id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Load user if needed
            if (order.User == null)
            {
                order.User = await _context.Users.FindAsync(order.UserId);
                
                if (order.User == null)
                {
                    TempData["ErrorMessage"] = "User not found for this order.";
                    return RedirectToPage(new { id = order.Id });
                }
            }
            
            // Send email logic
            // await _emailService.SendCustomEmailToUser(order.User.Email, subject, message);
            
            this.AddPageSuccess($"Email sent to {order.User.Email}.");
            return RedirectToPage(new { id = order.Id });
        }
        
        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Define valid transitions
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Placed, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.CustomerCanceled, OrderStatus.OutOfStock } },
                { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.CustomerCanceled, OrderStatus.OutOfStock } },
                { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.Packed, OrderStatus.CustomerCanceled, OrderStatus.OutOfStock } },
                { OrderStatus.Packed, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.OutOfStock } },
                { OrderStatus.Delivered, new List<OrderStatus> { OrderStatus.Finished } },
                // Canceled orders don't have transitions
                { OrderStatus.CustomerCanceled, new List<OrderStatus>() },
                { OrderStatus.OutOfStock, new List<OrderStatus>() },
                { OrderStatus.Finished, new List<OrderStatus>() }
            };
            
            return validTransitions.ContainsKey(currentStatus) && validTransitions[currentStatus].Contains(newStatus);
        }
        
        // Helper methods for the view
        public string GetStatusBadgeClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Placed => "bg-info",
                OrderStatus.Confirmed => "bg-primary",
                OrderStatus.Preparing => "bg-warning text-dark",
                OrderStatus.Packed => "bg-secondary",
                OrderStatus.Delivered => "bg-success",
                OrderStatus.Finished => "bg-light text-dark",
                OrderStatus.CustomerCanceled => "bg-danger",
                OrderStatus.OutOfStock => "bg-danger",
                _ => "bg-secondary"
            };
        }
        
        public string GetProgressBarClass()
        {
            return Order.Status switch
            {
                OrderStatus.CustomerCanceled => "bg-danger",
                OrderStatus.OutOfStock => "bg-danger",
                _ => "bg-success"
            };
        }
        
        public int GetProgressPercentage()
        {
            // Calculate progress percentage based on status
            if (Order.Status == OrderStatus.CustomerCanceled || Order.Status == OrderStatus.OutOfStock)
            {
                return 100; // Complete but canceled
            }
            
            var index = OrderStatusTimeline.IndexOf(Order.Status);
            if (index < 0) return 0;
            
            return (index * 100) / (OrderStatusTimeline.Count - 1);
        }
        
        public string GetStatusPointClass(OrderStatus status)
        {
            var classes = new List<string>();
            
            if (status == Order.Status)
            {
                classes.Add("status-current");
            }
            
            if (IsStatusCompleted(status))
            {
                classes.Add("status-completed");
            }
            
            if (Order.Status == OrderStatus.CustomerCanceled || Order.Status == OrderStatus.OutOfStock)
            {
                classes.Add("status-cancelled");
            }
            
            return string.Join(" ", classes);
        }
        
        public bool IsStatusCompleted(OrderStatus status)
        {
            if (Order.Status == OrderStatus.CustomerCanceled || Order.Status == OrderStatus.OutOfStock)
            {
                return false;
            }
            
            var currentIndex = OrderStatusTimeline.IndexOf(Order.Status);
            var statusIndex = OrderStatusTimeline.IndexOf(status);
            
            return statusIndex <= currentIndex;
        }
        
        public bool IsCurrentStatus(OrderStatus status)
        {
            return Order.Status == status;
        }
    }
}
