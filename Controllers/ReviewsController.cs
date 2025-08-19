using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Services;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/products/{productId:int}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _svc;
    public ReviewsController(IReviewService svc)
    {
        _svc = svc;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ReviewReadDto>>> Get(int productId)
    {
        var items = await _svc.GetAsync(productId);
        return Ok(items);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upsert(int productId, ReviewCreateDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            await _svc.UpsertAsync(userId, productId, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Product not found" });
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteMine(int productId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        await _svc.DeleteAsync(userId, productId);
        return NoContent();
    }
}
