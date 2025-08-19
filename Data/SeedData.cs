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
                new() { Name = "Kanchipuram Silk Saree", Category = "Kanjivaram", Description = "Handwoven Kanchipuram silk with zari border.", Color = "Maroon", Price = 12999, Stock = 10, ImageUrl = "https://example.com/images/kanchi-maroon.jpg" },
                new() { Name = "Banarasi Silk Saree", Category = "Banarasi", Description = "Traditional Banarasi silk with rich motifs.", Color = "Royal Blue", Price = 14999, Stock = 8, ImageUrl = "https://example.com/images/banarasi-blue.jpg" },
                new() { Name = "Mysore Silk Saree", Category = "Mysore", Description = "Soft Mysore silk with gold border.", Color = "Emerald Green", Price = 9999, Stock = 12, ImageUrl = "https://example.com/images/mysore-green.jpg" },
                new() { Name = "Tussar Silk Saree", Category = "Tussar", Description = "Elegant Tussar silk with natural texture.", Color = "Beige", Price = 8999, Stock = 15, ImageUrl = "https://example.com/images/tussar-beige.jpg" },
                new() { Name = "Chanderi Silk Saree", Category = "Chanderi", Description = "Lightweight Chanderi silk with delicate work.", Color = "Peach", Price = 7999, Stock = 20, ImageUrl = "https://example.com/images/chanderi-peach.jpg" },
                new() { Name = "Patola Silk Saree", Category = "Patola", Description = "Intricate Patola double ikat.", Color = "Red", Price = 19999, Stock = 5, ImageUrl = "https://example.com/images/patola-red.jpg" },
                new() { Name = "Paithani Silk Saree", Category = "Paithani", Description = "Classic Paithani with peacock pallu.", Color = "Purple", Price = 17999, Stock = 7, ImageUrl = "https://example.com/images/paithani-purple.jpg" },
                new() { Name = "Kota Silk Saree", Category = "Kota", Description = "Feather-light Kota silk checks.", Color = "Yellow", Price = 6999, Stock = 18, ImageUrl = "https://example.com/images/kota-yellow.jpg" }
            };
            ctx.Products.AddRange(products);
        }

        // Seed roles and users
        const string defaultEmail = "demo@reshambazaar.com";
        const string defaultPassword = "Password1!";
        const string adminEmail = "admin@reshambazaar.com";
        const string adminPassword = "AdminPass1!";

        if (!await roleMgr.RoleExistsAsync("Admin"))
        {
            await roleMgr.CreateAsync(new IdentityRole("Admin"));
        }

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

        var adminUser = await userMgr.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Admin User"
            };
            await userMgr.CreateAsync(adminUser, adminPassword);
        }
        if (!await userMgr.IsInRoleAsync(adminUser, "Admin"))
        {
            await userMgr.AddToRoleAsync(adminUser, "Admin");
        }

        // Seed coupons if empty
        if (!ctx.Coupons.Any())
        {
            var now = DateTime.UtcNow;
            ctx.Coupons.AddRange(
                new Coupon { Code = "WELCOME10", Type = DiscountType.Percent, Amount = 10, ExpiresAt = now.AddMonths(6), IsActive = true, MinOrderAmount = 1000, MaxDiscount = 1500 },
                new Coupon { Code = "FEST500", Type = DiscountType.Fixed, Amount = 500, ExpiresAt = now.AddMonths(3), IsActive = true, MinOrderAmount = 3000 },
                new Coupon { Code = "BIGSALE20", Type = DiscountType.Percent, Amount = 20, ExpiresAt = now.AddMonths(1), IsActive = true, MinOrderAmount = 5000, MaxDiscount = 3000 }
            );
        }

        await ctx.SaveChangesAsync();
    }
}

