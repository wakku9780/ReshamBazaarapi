using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Services;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;

    // Compatibility endpoint expected by frontend: POST /api/checkout
    [HttpPost]
    public async Task<ActionResult<OrderReadDto>> Post([FromBody] CheckoutRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var order = await _checkoutService.CheckoutAsync(userId, request);
            return CreatedAtAction(nameof(OrdersController.GetMyOrders), "Orders", new { }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
