using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Models;
using TiendaPromElec.Repositories;
using System.Linq;

namespace TiendaPromElec.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Rm Db
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Memory Db
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestingDb");
                });

                // Mock Data Seed
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();

                    if (!db.Products.Any())
                    {
                        db.Products.Add(new Product 
                        { 
                            Name = "Integration Test Item", 
                            Description = "Test Desc", 
                            Brand = "Test Brand", 
                            Price = 100 
                        });
                        db.SaveChanges();
                    }
                }
            });
        }
    }
}