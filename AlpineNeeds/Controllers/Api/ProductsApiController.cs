using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlpineNeeds.Controllers.Api
{
    [Route("api/products")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Return simplified product data for the modal
            return new
            {
                id = product.Id,
                name = product.Name,
                price = product.Price,
                stockQuantity = product.StockQuantity,
                colors = product.Colors,
                sizes = product.Sizes,
                hasOptions = product.Colors.Any() || product.Sizes.Any()
            };
        }
    }
}