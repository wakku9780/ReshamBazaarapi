using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Services;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;
    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // Anonymous validation allowed so cart page can pre-validate before login
    [HttpGet("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<CouponValidateResponse>> Validate([FromQuery] string code, [FromQuery] decimal subtotal)
    {
        var result = await _couponService.ValidateAsync(code, subtotal);
        return Ok(result);
    }
}
