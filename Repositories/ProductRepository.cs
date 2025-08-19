using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _ctx;
    public ProductRepository(AppDbContext ctx) { _ctx = ctx; }

    public async Task<(IEnumerable<Product> Items, int Total)> SearchAsync(ProductQuery query, CancellationToken ct = default)
    {
        var q = _ctx.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(term)
                          || p.Description.ToLower().Contains(term)
                          || p.Color.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var cat = query.Category.Trim().ToLower();
            q = q.Where(p => p.Category.ToLower().Contains(cat));
        }
        if (query.MinPrice.HasValue)
            q = q.Where(p => p.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue)
            q = q.Where(p => p.Price <= query.MaxPrice.Value);

        var by = (query.SortBy ?? "latest").ToLower();
        var dirDesc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        q = by switch
        {
            "price" => dirDesc ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price),
            "name"  => dirDesc ? q.OrderByDescending(p => p.Name)  : q.OrderBy(p => p.Name),
            _       => q.OrderByDescending(p => p.Id)
        };

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _ctx.Products.FindAsync([id], ct).AsTask();

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _ctx.Products.Add(product);
        await _ctx.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _ctx.Products.Remove(product);
        await _ctx.SaveChangesAsync(ct);
    }
}
