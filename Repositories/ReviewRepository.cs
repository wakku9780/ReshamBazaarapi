using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _ctx;
    public ReviewRepository(AppDbContext ctx) { _ctx = ctx; }

    public async Task<IEnumerable<Review>> GetForProductAsync(int productId, CancellationToken ct = default)
    {
        return await _ctx.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<Review?> GetByUserAsync(int productId, string userId, CancellationToken ct = default)
    {
        return _ctx.Reviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, ct);
    }

    public async Task<Review> AddAsync(Review review, CancellationToken ct = default)
    {
        _ctx.Reviews.Add(review);
        await _ctx.SaveChangesAsync(ct);
        return review;
    }

    public async Task UpdateAsync(Review review, CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Review review, CancellationToken ct = default)
    {
        _ctx.Reviews.Remove(review);
        await _ctx.SaveChangesAsync(ct);
    }
}
