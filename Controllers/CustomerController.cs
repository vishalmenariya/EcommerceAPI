using EcommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers
{
    [Route("ecommerce/customers")]
    [ApiController]
    [EnableRateLimiting("StrictPolicy")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public CustomerController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            List<Customer> result = await this._dbContext.Customers.AsNoTracking().ToListAsync();
                
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById([FromRoute] int id)
        {
            Customer result = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(customer => customer.CustomerId == id);

            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]  
        public async Task<IActionResult> AddNewCustomer([FromBody] Customer customer)
        {

            if (await _dbContext.Customers.AnyAsync(c => c.CustomerPhone == customer.CustomerPhone))
            {
                return Conflict(new { message = "A customer with this Phone Number already exists!" });
            }

            if (await _dbContext.Customers.AnyAsync(c => c.CustomerEmail == customer.CustomerEmail))
            {
                return Conflict(new { message = "A customer with this email already exists!" });
            }

            try
            {
                await _dbContext.Customers.AddAsync(customer);

                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
                   (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    return Conflict(new { message = "Race condition caught: This email was just registered by another request." });
                }

                return StatusCode(500, new { message = "An unexpected database error occurred. Please try again later." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer([FromRoute] int id, [FromBody] Customer updatedCustomer)
        {
            if (id != updatedCustomer.CustomerId)
            {
                return BadRequest(new { message = "The ID in the URL does not match the ID in the data." });
            }

            Customer existingCustomer = await _dbContext.Customers.FindAsync(id);
            if(existingCustomer == null)
            {
                return NotFound();
            }

            if (existingCustomer.CustomerPhone != updatedCustomer.CustomerPhone &&
                await _dbContext.Customers.AnyAsync(c => c.CustomerPhone == updatedCustomer.CustomerPhone))
            {
                return Conflict(new { message = "This phone number is already registered to another account." });
            }

            if (existingCustomer.CustomerEmail != updatedCustomer.CustomerEmail &&
                await _dbContext.Customers.AnyAsync(c => c.CustomerEmail == updatedCustomer.CustomerEmail))
            {
                return Conflict(new { message = "This email is already registered to another account." });
            }

            _dbContext.Entry(existingCustomer).CurrentValues.SetValues(updatedCustomer);

            try
            {
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "An unexpected database error occurred." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer([FromRoute] int id)
        {
            Customer customerToDelete = await _dbContext.Customers.FindAsync(id);

            if(customerToDelete ==  null)
            {
                return NotFound();
            }

            bool hasOrders = await _dbContext.Orders.AnyAsync(o => o.CustomerId == id);
            if (hasOrders)
            {
                return Conflict(new { message = "Cannot delete this customer because they have existing order history." });
            }

            _dbContext.Customers.Remove(customerToDelete);

            try
            {
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected database error occurred." });
            }
        }
    }
}
