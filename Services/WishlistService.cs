using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using ReshamBazaar.Api.Repositories;

namespace ReshamBazaar.Api.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _repo;
    private readonly IProductRepository _productRepo;
    public WishlistService(IWishlistRepository repo, IProductRepository productRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
    }

    public async Task<IEnumerable<WishlistItemReadDto>> GetAsync(string userId, CancellationToken ct = default)
    {
        var items = await _repo.GetForUserAsync(userId, ct);
        return items.Select(w => new WishlistItemReadDto(
            w.Id,
            w.ProductId,
            w.Product?.Name ?? string.Empty,
            w.Product?.Price ?? 0,
            w.Product?.ImageUrl ?? string.Empty,
            w.CreatedAt
        ));
    }

    public async Task AddAsync(string userId, int productId, CancellationToken ct = default)
    {
        var existing = await _repo.GetAsync(userId, productId, ct);
        if (existing != null) return; // idempotent

        // Optional: ensure product exists (light check)
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null) throw new KeyNotFoundException("Product not found");

        await _repo.AddAsync(new WishlistItem { UserId = userId, ProductId = productId }, ct);
    }

    public async Task RemoveAsync(string userId, int productId, CancellationToken ct = default)
    {
        var existing = await _repo.GetAsync(userId, productId, ct);
        if (existing == null) return; // idempotent
        await _repo.DeleteAsync(existing, ct);
    }
}
