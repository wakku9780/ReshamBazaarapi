using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Services;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _svc;
    public WishlistController(IWishlistService svc)
    {
        _svc = svc;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WishlistItemReadDto>>> Get()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var items = await _svc.GetAsync(userId);
        return Ok(items);
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(int productId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            await _svc.AddAsync(userId, productId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Product not found" });
        }
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        await _svc.RemoveAsync(userId, productId);
        return NoContent();
    }
}
