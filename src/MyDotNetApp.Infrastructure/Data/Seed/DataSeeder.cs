using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyDotNetApp.Domain.Entities;
using MyDotNetApp.Infrastructure.Identity;

namespace MyDotNetApp.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedProductsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@app.com";
        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task SeedProductsAsync(AppDbContext context)
    {
        if (await context.Products.IgnoreQueryFilters().AnyAsync())
            return;

        var products = new List<Product>
        {
            new() { Name = "Laptop Pro 15", Description = "High-performance laptop for developers", Price = 1299.99m, SKU = "LAP-001", Stock = 50 },
            new() { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 49.99m, SKU = "MOU-001", Stock = 200 },
            new() { Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard, Cherry MX Blue", Price = 129.99m, SKU = "KEY-001", Stock = 100 },
            new() { Name = "4K Monitor", Description = "27-inch 4K IPS display", Price = 599.99m, SKU = "MON-001", Stock = 30 },
            new() { Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI", Price = 39.99m, SKU = "HUB-001", Stock = 150 }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
