using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public interface IProductRepository
{
    Task<(IEnumerable<Product> Items, int Total)> SearchAsync(ProductQuery query, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Product> AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Product product, CancellationToken ct = default);
}
