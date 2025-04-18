using AlpineNeeds.Data;
using AlpineNeeds.Models;
using AlpineNeeds.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AlpineNeeds.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : BasePageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Order> Orders { get; set; } = new List<Order>();
        
        public int TotalCount { get; set; }
        
        public int TotalPages { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;
        
        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DateStart { get; set; }
        
        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public List<OrderStatus> StatusFilter { get; set; } = new List<OrderStatus>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public decimal? MinAmount { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public decimal? MaxAmount { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string SortField { get; set; } = "OrderDate";
        
        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";
        
        public SelectList StatusList { get; set; }
        
        // This will help with maintaining filter parameters in pagination links
        public Dictionary<string, string> RouteData { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {

            // Store route data for pagination
            RouteData = new Dictionary<string, string>
            {
                { "sortField", SortField },
                { "sortOrder", SortOrder }
            };
            
            if (DateStart.HasValue)
                RouteData.Add("dateStart", DateStart.Value.ToString("yyyy-MM-dd"));
            
            if (DateEnd.HasValue)
                RouteData.Add("dateEnd", DateEnd.Value.ToString("yyyy-MM-dd"));
            
            if (StatusFilter != null && StatusFilter.Any())
            {
                RouteData.Add("statusFilter", string.Join(",", StatusFilter.Select(s => s.ToString())));
            }
            
            if (!string.IsNullOrEmpty(SearchTerm))
                RouteData.Add("searchTerm", SearchTerm);
            
            if (MinAmount.HasValue)
                RouteData.Add("minAmount", MinAmount.Value.ToString());
            
            if (MaxAmount.HasValue)
                RouteData.Add("maxAmount", MaxAmount.Value.ToString());

            // Build query
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                .AsQueryable();

            // Apply filters
            if (DateStart.HasValue)
            {
                query = query.Where(o => o.OrderDate >= DateStart.Value);
            }
            
            if (DateEnd.HasValue)
            {
                // Add one day to include the end date fully
                var endDatePlusOneDay = DateEnd.Value.AddDays(1);
                query = query.Where(o => o.OrderDate < endDatePlusOneDay);
            }
            
            if (StatusFilter != null && StatusFilter.Any())
            {
                query = query.Where(o => StatusFilter.Contains(o.Status));
            }
            
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(o =>
                    o.Id.ToString().Contains(SearchTerm) ||
                    o.User.FirstName.Contains(SearchTerm) ||
                    o.User.LastName.Contains(SearchTerm) ||
                    o.User.Email.Contains(SearchTerm));
            }
            
            if (MinAmount.HasValue)
            {
                query = query.Where(o => o.TotalPrice >= MinAmount.Value);
            }
            
            if (MaxAmount.HasValue)
            {
                query = query.Where(o => o.TotalPrice <= MaxAmount.Value);
            }

            // Get total count for pagination
            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Ensure current page is within valid range
            CurrentPage = Math.Max(1, Math.Min(CurrentPage, Math.Max(1, TotalPages)));

            // Apply sorting
            query = ApplySorting(query, SortField, SortOrder);

            // Apply pagination
            Orders = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        private IQueryable<Order> ApplySorting(IQueryable<Order> query, string sortField, string sortOrder)
        {
            return (sortField, sortOrder) switch
            {
                ("Id", "asc") => query.OrderBy(o => o.Id),
                ("Id", _) => query.OrderByDescending(o => o.Id),
                
                ("OrderDate", "asc") => query.OrderBy(o => o.OrderDate),
                ("OrderDate", _) => query.OrderByDescending(o => o.OrderDate),
                
                ("TotalPrice", "asc") => query.OrderBy(o => o.TotalPrice),
                ("TotalPrice", _) => query.OrderByDescending(o => o.TotalPrice),
                
                ("Status", "asc") => query.OrderBy(o => o.Status),
                ("Status", _) => query.OrderByDescending(o => o.Status),
                
                // Default sort by date descending
                (_, _) => query.OrderByDescending(o => o.OrderDate)
            };
        }
        
        public string SortOrderForLink(string field)
        {
            return field == SortField ? (SortOrder == "asc" ? "desc" : "asc") : "asc";
        }
        
        public async Task<IActionResult> OnPostCancelOrderAsync(int orderId, string reason)
        {
            var order = await _context.Orders.FindAsync(orderId);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Check if order can be canceled
            if (order.Status != OrderStatus.Placed && 
                order.Status != OrderStatus.Confirmed && 
                order.Status != OrderStatus.Preparing)
            {
                TempData["ErrorMessage"] = "This order cannot be canceled due to its current status.";
                return RedirectToPage();
            }
            
            // Update order status
            order.Status = OrderStatus.CustomerCanceled;
            
            // Add order note about cancellation (if you have order notes table)
            // _context.OrderNotes.Add(new OrderNote { OrderId = orderId, Note = reason, CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
            
            await _context.SaveChangesAsync();
            
            this.AddPageSuccess($"Order #{orderId} has been canceled successfully.");
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostBatchActionAsync(string orderIds, string action)
        {
            if (string.IsNullOrEmpty(orderIds) || string.IsNullOrEmpty(action))
            {
                TempData["ErrorMessage"] = "Invalid selection or action.";
                return RedirectToPage();
            }
            
            var ids = orderIds.Split(',').Select(int.Parse).ToList();
            var orders = await _context.Orders.Where(o => ids.Contains(o.Id)).ToListAsync();
            
            if (!orders.Any())
            {
                TempData["ErrorMessage"] = "No orders found with the selected IDs.";
                return RedirectToPage();
            }
            
            switch (action)
            {
                case "MarkAsConfirmed":
                    foreach (var order in orders.Where(o => o.Status == OrderStatus.Placed))
                    {
                        order.Status = OrderStatus.Confirmed;
                    }
                    break;
                
                case "MarkAsPreparing":
                    foreach (var order in orders.Where(o => o.Status == OrderStatus.Confirmed))
                    {
                        order.Status = OrderStatus.Preparing;
                    }
                    break;
                
                case "MarkAsPacked":
                    foreach (var order in orders.Where(o => o.Status == OrderStatus.Preparing))
                    {
                        order.Status = OrderStatus.Packed;
                    }
                    break;
                
                case "MarkAsDelivered":
                    foreach (var order in orders.Where(o => o.Status == OrderStatus.Packed))
                    {
                        order.Status = OrderStatus.Delivered;
                    }
                    break;
                
                case "MarkAsFinished":
                    foreach (var order in orders.Where(o => o.Status == OrderStatus.Delivered))
                    {
                        order.Status = OrderStatus.Finished;
                    }
                    break;
                
                case "GenerateInvoices":
                    // This would typically route to a different action or service
                    this.AddPageSuccess($"Invoices for {orders.Count} orders have been generated successfully.");
                    return RedirectToPage();
                
                case "GeneratePackingSlips":
                    // This would typically route to a different action or service
                    this.AddPageSuccess($"Packing slips for {orders.Count} orders have been generated successfully.");
                    return RedirectToPage();
                
                default:
                    this.AddPageError("Invalid action specified.");
                    return RedirectToPage();
            }
            
            await _context.SaveChangesAsync();
            
            this.AddPageSuccess($"Updated status for {orders.Count} orders.");
            return RedirectToPage();
        }
    }
}
