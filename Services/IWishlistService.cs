using ReshamBazaar.Api.DTOs;

namespace ReshamBazaar.Api.Services;

public interface IWishlistService
{
    Task<IEnumerable<WishlistItemReadDto>> GetAsync(string userId, CancellationToken ct = default);
    Task AddAsync(string userId, int productId, CancellationToken ct = default);
    Task RemoveAsync(string userId, int productId, CancellationToken ct = default);
}
