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
public class CartController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly ICouponService _couponService;
    public CartController(AppDbContext ctx, ICouponService couponService)
    {
        _ctx = ctx;
        _couponService = couponService;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;

    [HttpGet]
    public async Task<ActionResult<CartSummaryDto>> Get([FromQuery] string? code)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var itemsQ = await _ctx.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        var items = itemsQ.Select(ci => new CartItemReadDto(
            ci.Id,
            ci.ProductId,
            ci.Product?.Name ?? string.Empty,
            ci.Product?.Price ?? 0,
            ci.Quantity,
            (ci.Product?.Price ?? 0) * ci.Quantity
        )).ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var validation = await _couponService.ValidateAsync(code, subtotal);
        var summary = new CartSummaryDto(
            Subtotal: subtotal,
            Discount: validation.DiscountAmount,
            FinalTotal: validation.FinalTotal,
            Items: items
        );
        return Ok(summary);
    }

    // Compatibility endpoint to return only items if some clients still expect array
    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<CartItemReadDto>>> GetItems()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var items = await _ctx.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .Select(ci => new CartItemReadDto(
                ci.Id,
                ci.ProductId,
                ci.Product!.Name,
                ci.Product.Price,
                ci.Quantity,
                ci.Product.Price * ci.Quantity
            ))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var count = await _ctx.CartItems.Where(ci => ci.UserId == userId).SumAsync(ci => ci.Quantity);
        return Ok(count);
    }

    [HttpPost("add")]
    public async Task<ActionResult<CartItemReadDto>> Add(CartItemRequestDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var product = await _ctx.Products.FindAsync(dto.ProductId);
        if (product == null) return NotFound(new { message = "Product not found" });

        var existing = await _ctx.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);
        if (existing != null)
        {
            existing.Quantity += Math.Max(1, dto.Quantity);
        }
        else
        {
            var ci = new CartItem { UserId = userId, ProductId = dto.ProductId, Quantity = Math.Max(1, dto.Quantity) };
            _ctx.CartItems.Add(ci);
        }
        await _ctx.SaveChangesAsync();

        return await GetItem(dto.ProductId);
    }

    // Base route support: POST /api/cart with { productId, quantity }
    [HttpPost]
    public Task<ActionResult<CartItemReadDto>> AddAtBase([FromBody] CartItemRequestDto dto)
        => Add(dto);

    // Compatibility route for clients calling POST /api/cart/{productId} with optional { quantity } body
    public record QuantityOnly(int Quantity);

    [HttpPost("{productId:int}")]
    public Task<ActionResult<CartItemReadDto>> AddByProductId(int productId, [FromBody] QuantityOnly? body)
    {
        var qty = body?.Quantity ?? 1;
        return Add(new CartItemRequestDto(productId, qty));
    }

    // Compatibility: PATCH /api/cart/{productId} to update quantity
    [HttpPatch("{productId:int}")]
    public async Task<ActionResult> PatchQuantity(int productId, [FromBody] QuantityOnly? body)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var ci = await _ctx.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        if (ci == null) return NotFound();

        if (body is null)
        {
            // No body => increment by 1
            ci.Quantity += 1;
        }
        else
        {
            ci.Quantity = body.Quantity;
        }

        if (ci.Quantity <= 0)
        {
            _ctx.CartItems.Remove(ci);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        await _ctx.SaveChangesAsync();
        // return updated item
        var item = await _ctx.CartItems.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == ci.Id);
        if (item == null) return NotFound();
        var read = new CartItemReadDto(item.Id, item.ProductId, item.Product?.Name ?? string.Empty, item.Product?.Price ?? 0, item.Quantity, (item.Product?.Price ?? 0) * item.Quantity);
        return Ok(read);
    }

    // Apply coupon and return full cart summary. If user is authenticated, subtotal/items are computed from DB cart; otherwise uses provided subtotal.
    // POST /api/cart/apply-coupon
    [HttpPost("apply-coupon")]
    [AllowAnonymous]
    public async Task<ActionResult<CartSummaryDto>> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        if (request is null) return BadRequest(new { message = "Invalid request" });

        // Try to compute from current user's cart
        var userId = GetUserId();
        decimal subtotal;
        List<CartItemReadDto> items = new();
        if (!string.IsNullOrEmpty(userId))
        {
            var cartItems = await _ctx.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
            foreach (var ci in cartItems)
            {
                var unit = ci.Product?.Price ?? 0;
                var line = unit * ci.Quantity;
                items.Add(new CartItemReadDto(ci.Id, ci.ProductId, ci.Product?.Name ?? string.Empty, unit, ci.Quantity, line));
            }
            subtotal = items.Sum(i => i.LineTotal);
        }
        else
        {
            subtotal = Math.Max(0, request.Subtotal);
        }

        var validation = await _couponService.ValidateAsync(request.Code, subtotal);
        var summary = new CartSummaryDto(
            Subtotal: subtotal,
            Discount: validation.DiscountAmount,
            FinalTotal: validation.FinalTotal,
            Items: items
        );
        return Ok(summary);
    }

    // GET /api/cart/summary?code=COUPON (optional convenience endpoint)
    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<ActionResult<CartSummaryDto>> GetSummary([FromQuery] string? code)
    {
        var userId = GetUserId();
        decimal subtotal = 0;
        List<CartItemReadDto> items = new();
        if (!string.IsNullOrEmpty(userId))
        {
            var cartItems = await _ctx.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
            foreach (var ci in cartItems)
            {
                var unit = ci.Product?.Price ?? 0;
                var line = unit * ci.Quantity;
                items.Add(new CartItemReadDto(ci.Id, ci.ProductId, ci.Product?.Name ?? string.Empty, unit, ci.Quantity, line));
            }
            subtotal = items.Sum(i => i.LineTotal);
        }

        var validation = await _couponService.ValidateAsync(code, subtotal);
        var summary = new CartSummaryDto(
            Subtotal: subtotal,
            Discount: validation.DiscountAmount,
            FinalTotal: validation.FinalTotal,
            Items: items
        );
        return Ok(summary);
    }

    private async Task<ActionResult<CartItemReadDto>> GetItem(int productId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var ci = await _ctx.CartItems.Include(x => x.Product).FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        if (ci == null) return NotFound();
        var read = new CartItemReadDto(ci.Id, ci.ProductId, ci.Product?.Name ?? string.Empty, ci.Product?.Price ?? 0, ci.Quantity, (ci.Product?.Price ?? 0) * ci.Quantity);
        return Ok(read);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateQuantity(CartItemRequestDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var ci = await _ctx.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == dto.ProductId);
        if (ci == null) return NotFound();
        if (dto.Quantity <= 0)
        {
            _ctx.CartItems.Remove(ci);
        }
        else
        {
            ci.Quantity = dto.Quantity;
        }
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var ci = await _ctx.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        if (ci == null) return NotFound();
        _ctx.CartItems.Remove(ci);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var items = _ctx.CartItems.Where(x => x.UserId == userId);
        _ctx.CartItems.RemoveRange(items);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
