using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedDefaultUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            // 1. Ensure Roles exist
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == Role.AdminRole);
            if (adminRole == null)
            {
                adminRole = new Role(1, Role.AdminRole);
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
            }

            var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == Role.CustomerRole);
            if (customerRole == null)
            {
                customerRole = new Role(2, Role.CustomerRole);
                context.Roles.Add(customerRole);
                await context.SaveChangesAsync();
            }

            // 2. Ensure Admin User exists
            var adminEmail = "admin@shopsphere.com";
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                var adminPassHash = passwordHasher.HashPassword("Admin@123");
                adminUser = new User("System Administrator", adminEmail, adminPassHash, adminRole.Id);
                context.Users.Add(adminUser);
            }

            // 3. Ensure Demo Customer exists
            var customerEmail = "customer@shopsphere.com";
            var customerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == customerEmail);
            if (customerUser == null)
            {
                var customerPassHash = passwordHasher.HashPassword("Customer@123");
                customerUser = new User("John Customer", customerEmail, customerPassHash, customerRole.Id);
                context.Users.Add(customerUser);
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding initial database users.");
        }
    }
}
