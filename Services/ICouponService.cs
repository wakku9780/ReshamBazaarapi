using ReshamBazaar.Api.DTOs;

namespace ReshamBazaar.Api.Services;

public interface ICouponService
{
    Task<CouponValidateResponse> ValidateAsync(string? code, decimal subtotal, CancellationToken ct = default);
}
