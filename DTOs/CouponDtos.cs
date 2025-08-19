namespace ReshamBazaar.Api.DTOs;

public record CouponReadDto(string Code, string Type, decimal Amount, DateTime? ExpiresAt, bool IsActive, decimal? MinOrderAmount, decimal? MaxDiscount);
public record CouponValidateResponse(bool IsValid, string Message, decimal DiscountAmount, decimal FinalTotal);
