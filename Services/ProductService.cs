using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using ReshamBazaar.Api.Repositories;

namespace ReshamBazaar.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<ProductReadDto>> GetAsync(ProductQuery query, CancellationToken ct = default)
    {
        // Translate friendly price token into min/max if provided
        if (!string.IsNullOrWhiteSpace(query.Price))
        {
            var token = query.Price.Trim().ToLower();
            if (token.StartsWith("under-"))
            {
                if (decimal.TryParse(token.Replace("under-", string.Empty), out var maxTok))
                    query.MaxPrice ??= maxTok;
            }
            else if (token.StartsWith("above-"))
            {
                if (decimal.TryParse(token.Replace("above-", string.Empty), out var minTok))
                    query.MinPrice ??= minTok;
            }
            else if (token.Contains('-'))
            {
                var parts = token.Split('-', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && decimal.TryParse(parts[0], out var a) && decimal.TryParse(parts[1], out var b))
                {
                    if (a > b) (a, b) = (b, a);
                    query.MinPrice ??= a;
                    query.MaxPrice ??= b;
                }
            }
        }

        var (items, total) = await _repo.SearchAsync(query, ct);
        var mapped = items.Select(p => new ProductReadDto(p.Id, p.Name, p.Description, p.Category, p.Color, p.Price, p.Stock, p.ImageUrl));
        return new PagedResult<ProductReadDto>
        {
            Items = mapped,
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100),
            TotalCount = total
        };
    }

    public async Task<ProductReadDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new ProductReadDto(p.Id, p.Name, p.Description, p.Category, p.Color, p.Price, p.Stock, p.ImageUrl);
    }

    public async Task<ProductReadDto> CreateAsync(ProductCreateDto dto, CancellationToken ct = default)
    {
        var p = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Color = dto.Color,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl
        };
        var created = await _repo.AddAsync(p, ct);
        return new ProductReadDto(created.Id, created.Name, created.Description, created.Category, created.Color, created.Price, created.Stock, created.ImageUrl);
    }

    public async Task UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Product not found");
        p.Name = dto.Name;
        p.Description = dto.Description;
        p.Category = dto.Category;
        p.Color = dto.Color;
        p.Price = dto.Price;
        p.Stock = dto.Stock;
        p.ImageUrl = dto.ImageUrl;
        await _repo.UpdateAsync(p, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Product not found");
        await _repo.DeleteAsync(p, ct);
    }
}
