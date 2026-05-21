using EcommerceAPI.Data;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace EcommerceAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("StrictPolicy", policy =>
                {
                    policy.Window = TimeSpan.FromSeconds(20);
                    policy.PermitLimit = 10;
                    policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    policy.QueueLimit = 0;
                });
            });

            //builder.Services.AddDbContext<AppDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDb")));

            // Change .UseSqlServer to .UseNpgsql
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<AppDbContext>();
                        DbInitializer.Initialize(context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
                    }
                }

                app.MapOpenApi();

                app.MapScalarApiReference();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}