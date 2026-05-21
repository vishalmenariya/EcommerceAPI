using EcommerceAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Identity.Client;

namespace EcommerceAPI.Controllers
{
    [Route("ecommerce/reports")]
    [ApiController]
    [EnableRateLimiting("StrictPolicy")]
    public class ReportController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet("popular-products")]
        public async Task<IActionResult> GetMostPopularProducts([FromQuery] int limit = 5)
        {
            if (limit <= 0)
            {
                return BadRequest(new { message = "The limit must be a number greater than 0." });
            }

            var topProductStats = await dbContext.OrderItems
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                TotalSold = group.Sum(item => item.Quantity)
            })
            .OrderByDescending(result => result.TotalSold)
            .Take(limit)
            .ToListAsync();

            if (!topProductStats.Any()) return NotFound("No order data available.");

            var topProductIds = topProductStats.Select(p => p.ProductId).ToList();

            var productNames = await dbContext.Products
                .Where(product => topProductIds.Contains(product.ProductId))
                .ToDictionaryAsync(product => product.ProductId, product => product.ProductName);

            var finalResult = topProductStats.Select(stat => new
            {
                ProductId = stat.ProductId,
                ProductName = productNames.GetValueOrDefault(stat.ProductId, "Unknown Product"),
                TotalQuantitySold = stat.TotalSold
            });

            return Ok(finalResult);
        }

        [HttpGet("top-customers")]
        public async Task<IActionResult> GetMostOrdersCustomer([FromQuery] int limit = 5)
        {
            if (limit <= 0)
            {
                return BadRequest(new { message = "The limit must be a number greater than 0." });
            }

            var topCustomerStats = await dbContext.Orders
                .GroupBy(order => order.CustomerId)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    TotalOrders = group.Count()
                })
                .OrderByDescending(result => result.TotalOrders)
                .Take(limit)
                .ToListAsync();

            if (!topCustomerStats.Any())
            {
                return NotFound(new { message = "No order data available." });
            }

            var topCustomerIds = topCustomerStats.Select(c => c.CustomerId).ToList();

            var customerNames = await dbContext.Customers
                .Where(customer => topCustomerIds.Contains(customer.CustomerId))
                .ToDictionaryAsync(customer => customer.CustomerId, customer => customer.CustomerName);

            var finalResult = topCustomerStats.Select(stat => new
            {
                CustomerId = stat.CustomerId,
                CustomerName = customerNames.GetValueOrDefault(stat.CustomerId, "Unknown Customer"),
                TotalOrdersPlaced = stat.TotalOrders
            });

            return Ok(finalResult);
        }

        [HttpGet("total-sales")]
        public async Task<IActionResult> GetTotalSales([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date cannot be after the end date." });
            }

            var result = await dbContext.Orders
                .Where(order => order.OrderDate >= startDate && order.OrderDate <= endDate)
                .SelectMany(order => order.Items)
                .SumAsync(item => item.Quantity * item.UnitPrice);

            return Ok(new
            {
                PeriodStart = startDate.ToString("yyyy-MM-dd"),
                PeriodEnd = endDate.ToString("yyyy-MM-dd"),
                TotalRevenue = result
            });
        }

        [HttpGet("get-country-wise-count")]
        public async Task<IActionResult> GetCountryWiseCount()
        {
            var result = await dbContext.Customers
                .GroupBy(customer => customer.CustomerCountry)
                .Select(group => new
                {
                    CountryName = group.Key,
                    CustomerCount = group.Count()
                })
                .OrderBy(result => result.CountryName)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("get-customers-by-country-name")]
        public async Task<IActionResult> GetCustomerByCountryName([FromQuery] string countryName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {

            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "Page number and page size must be greater than 0." });
            }

            if (pageSize > 100) pageSize = 100;

            int itemsToSkip = (pageNumber - 1) * pageSize;

            var result = await dbContext.Customers
                .AsNoTracking()
                .Where(customer => customer.CustomerCountry == countryName)
                .Skip(itemsToSkip)
                .Take(pageSize)
                .ToListAsync();

            if (!result.Any())
            {
                return NotFound(new { message = $"No customer for the country {countryName} is there!" });
            }

            return Ok(new
            {
                Page = pageNumber,
                ItemsReturned = result.Count,
                Data = result
            });
        }

        [HttpGet("get-category-wise-count")]
        public async Task<IActionResult> GetCategoryWiseCount()
        {
            var result = await dbContext.Products
                .GroupBy(product => product.ProductCategory)
                .Select(group => new
                {
                    CategoryName = group.Key,
                    CategoryCount = group.Count()
                })
                .OrderBy(result => result.CategoryName)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("get-products-by-category-name")]
        public async Task<IActionResult> GetProductsByCategoryName([FromQuery] string categoryName)
        {
            var result = await dbContext.Products
                .Where(product => product.ProductCategory == categoryName)
                .AsNoTracking()
                .ToListAsync();

            if (!result.Any())
            {
                return NotFound(new { message = $"No product for the category {categoryName} is there!" });
            }

            return Ok(result);
        }

        [HttpGet("sales-location-wise")]
        public async Task<IActionResult> GetSalesLocationWise()
        {
            var result = await dbContext.OrderItems
                .GroupBy(item => item.Order.Customer.CustomerCountry)
                .Select(group => new
                {
                    CountryName = group.Key,
                    TotalRevenue = group.Sum(item => item.Quantity * item.UnitPrice)
                })
                .OrderByDescending(result => result.TotalRevenue)
                .AsNoTracking()
                .ToListAsync();

            if (!result.Any())
            {
                return NotFound(new { message = "No sales data available to calculate." });
            }

            return Ok(result);
        }

        [HttpGet("get-customer-orders")]
        public async Task<IActionResult> GetCustomerOrders([FromQuery] int customerId)
        {

            if (customerId <= 0)
            {
                return BadRequest(new { message = "Invalid customer ID." });
            }

            var purchasedProducts = await dbContext.OrderItems
                .AsNoTracking()
                .Where(item => item.Order.CustomerId == customerId)
                .Select(item => new
                {
                    ProductName = item.Product.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice
                })  
                .ToListAsync();

            if (!purchasedProducts.Any())
            {
                return NotFound(new { message = $"No products found for the customer ID '{customerId}'." });
            }

            return Ok(purchasedProducts);
        }
    }
}