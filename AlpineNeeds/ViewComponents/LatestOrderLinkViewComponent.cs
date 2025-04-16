using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AlpineNeeds.ViewComponents
{
    public class LatestOrderLinkViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public LatestOrderLinkViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string? userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // Not logged in, show default link
                return View("Default");
            }

            Order? latestOrder = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            return View(latestOrder);
        }
    }
}
