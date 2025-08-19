using ReshamBazaar.Api.DTOs;

namespace ReshamBazaar.Api.Services;

public interface IReviewService
{
    Task<IEnumerable<ReviewReadDto>> GetAsync(int productId, CancellationToken ct = default);
    Task UpsertAsync(string userId, int productId, ReviewCreateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string userId, int productId, CancellationToken ct = default);
}
