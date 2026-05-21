using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Data
{
    public class DbInitializer
    {
        public static void Initialize(AppDbContext dbContext)
        {
            if(dbContext.Customers.Any())
            {
                return;
            }

            List<Customer> customers = new List<Customer>
            {
                new Customer { CustomerName = "Bruce Wayne", CustomerPhone = "555-0199", CustomerEmail = "bruce@wayneenterprises.com", CustomerCountry = "USA" },
                new Customer { CustomerName = "Clark Kent", CustomerPhone = "555-0188", CustomerEmail = "clark@dailyplanet.com", CustomerCountry = "UK" },
                new Customer { CustomerName = "Diana Prince", CustomerPhone = "555-0177", CustomerEmail = "diana@themyscira.com", CustomerCountry = "IRELAND" }
            };

            dbContext.Customers.AddRange(customers);
            dbContext.SaveChanges();


            List<Product> products = new List<Product>()
            {
                new Product { ProductName = "High-End Gaming Laptop", ProductStock = 50, ProductPrice = 1499.99m, ProductCategory="Laptop" },
                new Product { ProductName = "Mechanical Keyboard", ProductStock = 120, ProductPrice = 129.50m, ProductCategory="Accessories"  },
                new Product { ProductName = "Wireless Noise-Canceling Headphones", ProductStock = 200, ProductPrice = 249.99m, ProductCategory="Sound"  },
                new Product { ProductName = "USB-C Hub", ProductStock = 500, ProductPrice = 45.00m, ProductCategory="Connectors"  }
            };

            dbContext.Products.AddRange(products);
            dbContext.SaveChanges();


            List<Order> orders = new List<Order>
            {
                new Order { OrderDate = DateTime.UtcNow.AddDays(-5), CustomerId = customers[0].CustomerId }, 
                new Order { OrderDate = DateTime.UtcNow, CustomerId = customers[2].CustomerId }
            };

            dbContext.Orders.AddRange(orders);
            dbContext.SaveChanges();


            List<OrderItems> orderItems = new List<OrderItems>
            {
                new OrderItems { OrderId = orders[0].OrderId, ProductId = products[0].ProductId, Quantity = 1, UnitPrice = products[0].ProductPrice },
                new OrderItems { OrderId = orders[0].OrderId, ProductId = products[1].ProductId, Quantity = 1, UnitPrice = products[1].ProductPrice },
                new OrderItems { OrderId = orders[1].OrderId, ProductId = products[2].ProductId, Quantity = 2, UnitPrice = products[2].ProductPrice }
            };
            dbContext.OrderItems.AddRange(orderItems);

            dbContext.SaveChanges();
        }
    }
}
