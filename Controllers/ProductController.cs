using EcommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers
{
    [Route("ecommerce/products")]
    [ApiController]
    [EnableRateLimiting("StrictPolicy")]
    public class ProductController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            List<Product> result = await dbContext.Products.AsNoTracking().ToListAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById([FromRoute] int id)
        {
            var result = await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(product => product.ProductId == id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            try
            {
                await dbContext.Products.AddAsync(product);

                await dbContext.SaveChangesAsync();

                return CreatedAtAction("GetProductById", new { id = product.ProductId }, product);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "An unexpected database error occurred. Please try again later." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] Product updatedProduct)
        {
            if(id != updatedProduct.ProductId)
            {
                return BadRequest(new { message = "The ID in the URL does not match the ID in the data." });
            }

            Product existingProduct = await dbContext.Products.FindAsync(id);

            if(existingProduct == null)
            {
                return NotFound();
            }

            dbContext.Entry(existingProduct).CurrentValues.SetValues(updatedProduct);

            try
            {
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "An unexpected database error occurred." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            Product productToDelete = await dbContext.Products.FindAsync(id);

            if (productToDelete == null)
            {
                return NotFound();
            }

            bool hasOrders = await dbContext.OrderItems.AnyAsync(o => o.ProductId == id);
            if (hasOrders)
            {
                return Conflict(new { message = "Cannot delete this product because it has existing order history." });
            }

            dbContext.Products.Remove(productToDelete);

            try
            {
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected database error occurred." });
            }
        }

    }
}
