using EcommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers
{
    [Route("ecommerce/orders")]
    [ApiController]
    [EnableRateLimiting("StrictPolicy")]
    public class OrderController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await dbContext.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById([FromRoute] int id)
        {
            var result = await dbContext.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            var requestedProductIds = order.Items.Select(i => i.ProductId).ToList();

            var databaseProducts = await dbContext.Products
                .Where(p => requestedProductIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            foreach (var item in order.Items)
            {
                if (!databaseProducts.TryGetValue(item.ProductId, out var realProduct))
                {
                    return BadRequest(new { message = $"Product with ID {item.ProductId} does not exist." });
                }

                if (realProduct.ProductStock < item.Quantity)
                {
                    return Conflict(new { message = $"Insufficient stock for {realProduct.ProductName}. Only {realProduct.ProductStock} left." });
                }

                realProduct.ProductStock -= item.Quantity;
                
                item.UnitPrice = realProduct.ProductPrice;
            }

            try
            {
                await dbContext.Orders.AddAsync(order);
                await dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected database error occurred." });
            }
        }
    }
}