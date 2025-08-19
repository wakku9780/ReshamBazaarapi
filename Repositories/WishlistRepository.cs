using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _ctx;
    public WishlistRepository(AppDbContext ctx) { _ctx = ctx; }

    public async Task<IEnumerable<WishlistItem>> GetForUserAsync(string userId, CancellationToken ct = default)
    {
        return await _ctx.WishlistItems
            .AsNoTracking()
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<WishlistItem?> GetAsync(string userId, int productId, CancellationToken ct = default)
    {
        return _ctx.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);
    }

    public async Task<WishlistItem> AddAsync(WishlistItem item, CancellationToken ct = default)
    {
        _ctx.WishlistItems.Add(item);
        await _ctx.SaveChangesAsync(ct);
        return item;
    }

    public async Task DeleteAsync(WishlistItem item, CancellationToken ct = default)
    {
        _ctx.WishlistItems.Remove(item);
        await _ctx.SaveChangesAsync(ct);
    }
}
