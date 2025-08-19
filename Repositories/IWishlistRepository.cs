using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public interface IWishlistRepository
{
    Task<IEnumerable<WishlistItem>> GetForUserAsync(string userId, CancellationToken ct = default);
    Task<WishlistItem?> GetAsync(string userId, int productId, CancellationToken ct = default);
    Task<WishlistItem> AddAsync(WishlistItem item, CancellationToken ct = default);
    Task DeleteAsync(WishlistItem item, CancellationToken ct = default);
}
