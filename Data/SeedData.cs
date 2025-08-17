using Microsoft.AspNetCore.Identity;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext ctx, UserManager<ApplicationUser> userMgr, RoleManager<IdentityRole> roleMgr)
    {
        // Seed products if empty
        if (!ctx.Products.Any())
        {
            var products = new List<Product>
            {
                new() { Name = "Kanchipuram Silk Saree", Description = "Handwoven Kanchipuram silk with zari border.", Color = "Maroon", Price = 12999, Stock = 10, ImageUrl = "https://example.com/images/kanchi-maroon.jpg" },
                new() { Name = "Banarasi Silk Saree", Description = "Traditional Banarasi silk with rich motifs.", Color = "Royal Blue", Price = 14999, Stock = 8, ImageUrl = "https://example.com/images/banarasi-blue.jpg" },
                new() { Name = "Mysore Silk Saree", Description = "Soft Mysore silk with gold border.", Color = "Emerald Green", Price = 9999, Stock = 12, ImageUrl = "https://example.com/images/mysore-green.jpg" },
                new() { Name = "Tussar Silk Saree", Description = "Elegant Tussar silk with natural texture.", Color = "Beige", Price = 8999, Stock = 15, ImageUrl = "https://example.com/images/tussar-beige.jpg" },
                new() { Name = "Chanderi Silk Saree", Description = "Lightweight Chanderi silk with delicate work.", Color = "Peach", Price = 7999, Stock = 20, ImageUrl = "https://example.com/images/chanderi-peach.jpg" },
                new() { Name = "Patola Silk Saree", Description = "Intricate Patola double ikat.", Color = "Red", Price = 19999, Stock = 5, ImageUrl = "https://example.com/images/patola-red.jpg" },
                new() { Name = "Paithani Silk Saree", Description = "Classic Paithani with peacock pallu.", Color = "Purple", Price = 17999, Stock = 7, ImageUrl = "https://example.com/images/paithani-purple.jpg" },
                new() { Name = "Kota Silk Saree", Description = "Feather-light Kota silk checks.", Color = "Yellow", Price = 6999, Stock = 18, ImageUrl = "https://example.com/images/kota-yellow.jpg" }
            };
            ctx.Products.AddRange(products);
        }

        // Seed default user
        const string defaultEmail = "demo@reshambazaar.com";
        const string defaultPassword = "Password1!";
        if (await userMgr.FindByEmailAsync(defaultEmail) is null)
        {
            var user = new ApplicationUser
            {
                UserName = defaultEmail,
                Email = defaultEmail,
                EmailConfirmed = true,
                FullName = "Demo User"
            };
            await userMgr.CreateAsync(user, defaultPassword);
        }

        await ctx.SaveChangesAsync();
    }
}
