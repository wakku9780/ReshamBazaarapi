using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Services;

public class CheckoutService : ICheckoutService
{
    private readonly AppDbContext _ctx;
    private readonly ICouponService _couponSvc;
    public CheckoutService(AppDbContext ctx, ICouponService couponSvc)
    {
        _ctx = ctx;
        _couponSvc = couponSvc;
    }

    public async Task<OrderReadDto> CheckoutAsync(string userId, CheckoutRequestDto request, CancellationToken ct = default)
    {
        Console.WriteLine($"CouponCode received: '{request.CouponCode}'");

        var items = await _ctx.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync(ct);
        if (items.Count == 0) throw new InvalidOperationException("Cart is empty");

        decimal subtotal = 0;
        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = request.Address != null
    ? $"{request.Address.FullName}, {request.Address.Line1}" +
      $"{(!string.IsNullOrWhiteSpace(request.Address.Line2) ? ", " + request.Address.Line2 : "")}, " +
      $"{request.Address.City} - {request.Address.Pincode}, {request.Address.Phone}"
    : null

        };

        foreach (var ci in items)
        {
            if (ci.Product == null) continue;
            var lineTotal = ci.Product.Price * ci.Quantity;
            subtotal += lineTotal;
            order.Items.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                UnitPrice = ci.Product.Price,
                Quantity = ci.Quantity
            });
        }

        order.Total = subtotal;

        // Apply coupon if any
        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var validation = await _couponSvc.ValidateAsync(request.CouponCode!, subtotal, ct);
            if (validation.IsValid)
            {
                discount = validation.DiscountAmount;
                order.CouponCode = request.CouponCode!.Trim().ToUpperInvariant();
            }
        }
        order.Discount = discount;
        order.FinalTotal = subtotal - discount;

        _ctx.Orders.Add(order);
        _ctx.CartItems.RemoveRange(items);
        await _ctx.SaveChangesAsync(ct);

        return new OrderReadDto(
            order.Id,
            order.CreatedAt,
            order.Total,
            order.Discount,
            order.FinalTotal,
            order.CouponCode,
            order.Status,
            order.Items.Select(i => new OrderItemReadDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()
        );
    }
}
