using ReshamBazaar.Api.DTOs;

namespace ReshamBazaar.Api.Services;

public interface IProductService
{
    Task<PagedResult<ProductReadDto>> GetAsync(ProductQuery query, CancellationToken ct = default);
    Task<ProductReadDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductReadDto> CreateAsync(ProductCreateDto dto, CancellationToken ct = default);
    Task UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
