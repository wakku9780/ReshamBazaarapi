using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using ReshamBazaar.Api.Repositories;

namespace ReshamBazaar.Api.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repo;
    private readonly IProductRepository _productRepo;
    public ReviewService(IReviewRepository repo, IProductRepository productRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
    }

    public async Task<IEnumerable<ReviewReadDto>> GetAsync(int productId, CancellationToken ct = default)
    {
        var items = await _repo.GetForProductAsync(productId, ct);
        return items.Select(r => new ReviewReadDto(r.Id, r.ProductId, r.UserId, r.Rating, r.Comment, r.CreatedAt));
    }

    public async Task UpsertAsync(string userId, int productId, ReviewCreateDto dto, CancellationToken ct = default)
    {
        // Ensure product exists
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product is null) throw new KeyNotFoundException("Product not found");

        var existing = await _repo.GetByUserAsync(productId, userId, ct);
        if (existing is null)
        {
            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };
            await _repo.AddAsync(review, ct);
        }
        else
        {
            existing.Rating = dto.Rating;
            existing.Comment = dto.Comment;
            await _repo.UpdateAsync(existing, ct);
        }
    }

    public async Task DeleteAsync(string userId, int productId, CancellationToken ct = default)
    {
        var existing = await _repo.GetByUserAsync(productId, userId, ct);
        if (existing is null) return;
        await _repo.DeleteAsync(existing, ct);
    }
}
