using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetForProductAsync(int productId, CancellationToken ct = default);
    Task<Review?> GetByUserAsync(int productId, string userId, CancellationToken ct = default);
    Task<Review> AddAsync(Review review, CancellationToken ct = default);
    Task UpdateAsync(Review review, CancellationToken ct = default);
    Task DeleteAsync(Review review, CancellationToken ct = default);
}
