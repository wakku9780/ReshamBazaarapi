using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public CartController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemReadDto>>> Get()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var items = await _ctx.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        var result = items.Select(ci => new CartItemReadDto(
            ci.Id,
            ci.ProductId,
            ci.Product?.Name ?? string.Empty,
            ci.Product?.Price ?? 0,
            ci.Quantity,
            (ci.Product?.Price ?? 0) * ci.Quantity
        ));
        return Ok(result);
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
