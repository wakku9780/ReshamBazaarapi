using ReshamBazaar.Api.DTOs;

namespace ReshamBazaar.Api.Services;

public interface ICheckoutService
{
    Task<OrderReadDto> CheckoutAsync(string userId, CheckoutRequestDto request, CancellationToken ct = default);
}
