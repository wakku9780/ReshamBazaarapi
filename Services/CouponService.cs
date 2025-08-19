using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using ReshamBazaar.Api.Repositories;

namespace ReshamBazaar.Api.Services;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _repo;
    public CouponService(ICouponRepository repo)
    {
        _repo = repo;
    }

    public async Task<CouponValidateResponse> ValidateAsync(string? code, decimal subtotal, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new CouponValidateResponse(false, "No coupon code provided", 0, subtotal);

        var coupon = await _repo.GetByCodeAsync(code.Trim().ToUpperInvariant(), ct);
        if (coupon is null || !coupon.IsActive)
            return new CouponValidateResponse(false, "Invalid coupon", 0, subtotal);
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
            return new CouponValidateResponse(false, "Coupon expired", 0, subtotal);
        if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
            return new CouponValidateResponse(false, $"Minimum order amount is {coupon.MinOrderAmount.Value}", 0, subtotal);

        decimal discount = 0;
        if (coupon.Type == DiscountType.Percent)
        {
            discount = Math.Round(subtotal * (coupon.Amount / 100m), 2);
            if (coupon.MaxDiscount.HasValue)
                discount = Math.Min(discount, coupon.MaxDiscount.Value);
        }
        else // Fixed
        {
            discount = coupon.Amount;
        }

        discount = Math.Clamp(discount, 0, subtotal);
        var finalTotal = subtotal - discount;
        return new CouponValidateResponse(true, "Coupon applied", discount, finalTotal);
    }
}
