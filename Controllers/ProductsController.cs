using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ProductsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ProductReadDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? price,
        [FromQuery] string? sortBy = "latest",
        [FromQuery] string? sortDir = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _ctx.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term) || p.Color.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim().ToLower();
            // More forgiving: allow minor spelling differences by using Contains
            query = query.Where(p => p.Category.ToLower().Contains(cat));
        }

        // Price token parser: supports formats like "under-1499", "above-5000", or "1500-4999"
        if (!string.IsNullOrWhiteSpace(price))
        {
            var token = price.Trim().ToLower();
            if (token.StartsWith("under-"))
            {
                if (decimal.TryParse(token.Replace("under-", string.Empty), out var maxTok))
                {
                    maxPrice ??= maxTok;
                }
            }
            else if (token.StartsWith("above-"))
            {
                if (decimal.TryParse(token.Replace("above-", string.Empty), out var minTok))
                {
                    minPrice ??= minTok;
                }
            }
            else if (token.Contains('-'))
            {
                var parts = token.Split('-', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && decimal.TryParse(parts[0], out var a) && decimal.TryParse(parts[1], out var b))
                {
                    if (a > b) (a, b) = (b, a);
                    minPrice ??= a;
                    maxPrice ??= b;
                }
            }
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Sorting
        var by = (sortBy ?? "latest").ToLower();
        var dirDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (by) switch
        {
            "price" => dirDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "name" => dirDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.Id)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var resultItems = items.Select(p => new ProductReadDto(p.Id, p.Name, p.Description, p.Category, p.Color, p.Price, p.Stock, p.ImageUrl));
        var result = new PagedResult<ProductReadDto>
        {
            Items = resultItems,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductReadDto>> GetById(int id)
    {
        var p = await _ctx.Products.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProductReadDto(p.Id, p.Name, p.Description, p.Category, p.Color, p.Price, p.Stock, p.ImageUrl));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductReadDto>> Create(ProductCreateDto dto)
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
        _ctx.Products.Add(p);
        await _ctx.SaveChangesAsync();
        var read = new ProductReadDto(p.Id, p.Name, p.Description, p.Category, p.Color, p.Price, p.Stock, p.ImageUrl);
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, read);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        var p = await _ctx.Products.FindAsync(id);
        if (p == null) return NotFound();
        p.Name = dto.Name;
        p.Description = dto.Description;
        p.Category = dto.Category;
        p.Color = dto.Color;
        p.Price = dto.Price;
        p.Stock = dto.Stock;
        p.ImageUrl = dto.ImageUrl;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _ctx.Products.FindAsync(id);
        if (p == null) return NotFound();
        _ctx.Products.Remove(p);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
