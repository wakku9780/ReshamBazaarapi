using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using ReshamBazaar.Api.Services;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly ICheckoutService _checkout;
    public OrdersController(AppDbContext ctx, ICheckoutService checkout)
    {
        _ctx = ctx;
        _checkout = checkout;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;

    [HttpPost("create-from-cart")]
    public async Task<ActionResult<OrderReadDto>> CreateFromCart()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var cartItems = await _ctx.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
        if (cartItems.Count == 0) return BadRequest(new { message = "Cart is empty" });

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        decimal total = 0;
        foreach (var ci in cartItems)
        {
            if (ci.Product == null) continue;
            var lineTotal = ci.Product.Price * ci.Quantity;
            total += lineTotal;
            order.Items.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                UnitPrice = ci.Product.Price,
                Quantity = ci.Quantity
            });
        }
        order.Total = total;

        _ctx.Orders.Add(order);
        _ctx.CartItems.RemoveRange(cartItems);
        await _ctx.SaveChangesAsync();

        var read = new OrderReadDto(
            order.Id,
            order.CreatedAt,
            order.Total,
            0,
            order.Total,
            null,
            order.Items.Select(i => new OrderItemReadDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()
        );
        return CreatedAtAction(nameof(GetMyOrders), new { }, read);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderReadDto>> Checkout(CheckoutRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var order = await _checkout.CheckoutAsync(userId, request);
            return CreatedAtAction(nameof(GetMyOrders), new { }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetMyOrders()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var orders = await _ctx.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(o => new OrderReadDto(
            o.Id,
            o.CreatedAt,
            o.Total,
            o.Discount,
            o.FinalTotal == 0 ? o.Total - o.Discount : o.FinalTotal,
            o.CouponCode,
            o.Items.Select(i => new OrderItemReadDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()
        ));
        return Ok(result);
    }
}
