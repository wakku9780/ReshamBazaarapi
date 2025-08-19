using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using Microsoft.Extensions.Logging;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminProductsController> _logger;
    public AdminProductsController(AppDbContext ctx, IWebHostEnvironment env, ILogger<AdminProductsController> logger)
    {
        _ctx = ctx;
        _env = env;
        _logger = logger;
    }

    [HttpPost("json")]
    public async Task<ActionResult<Product>> CreateJson([FromBody] ProductCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var product = new Product
        {
            Name = dto.Name,
            Category = dto.Category ?? string.Empty,
            Color = dto.Color ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = string.Empty
        };
        try
        {
            _ctx.Products.Add(product);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (DbUpdateException dbx)
        {
            _logger.LogError(dbx, "DB error creating product (json)");
            return Problem(detail: dbx.InnerException?.Message ?? dbx.Message, statusCode: 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating product (json)");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> List([FromQuery] ProductListQuery q)
    {
        var query = _ctx.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(q.Category))
        {
            var c = q.Category.Trim().ToLower();
            query = query.Where(p => p.Category != null && p.Category.ToLower() == c);
        }
        if (q.MinPrice.HasValue) query = query.Where(p => p.Price >= q.MinPrice.Value);
        if (q.MaxPrice.HasValue) query = query.Where(p => p.Price <= q.MaxPrice.Value);

        var skip = Math.Max(0, (q.Page - 1) * q.PageSize);
        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip(skip)
            .Take(q.PageSize)
            .ToListAsync();
        // Normalize ImageUrl to absolute
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        foreach (var p in items)
        {
            if (!string.IsNullOrEmpty(p.ImageUrl) && p.ImageUrl.StartsWith("/"))
            {
                p.ImageUrl = baseUrl + p.ImageUrl;
            }
        }
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var p = await _ctx.Products.FindAsync(id);
        if (p is null) return NotFound();
        if (!string.IsNullOrEmpty(p.ImageUrl) && p.ImageUrl.StartsWith("/"))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            p.ImageUrl = baseUrl + p.ImageUrl;
        }
        return Ok(p);
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<Product>> Create([FromForm] ProductCreateUpdateDto dto, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var product = new Product
        {
            Name = dto.Name,
            Category = dto.Category ?? string.Empty,
            Color = dto.Color ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Price = dto.Price,
            Stock = dto.Stock
        };
        try
        {
            if (image != null)
            {
                product.ImageUrl = await SaveImage(image);
            }
            _ctx.Products.Add(product);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Product create validation failed");
            return BadRequest(new { message = vex.Message });
        }
        catch (DbUpdateException dbx)
        {
            _logger.LogError(dbx, "DB error creating product");
            return Problem(detail: dbx.InnerException?.Message ?? dbx.Message, statusCode: 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating product");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    [HttpPut("{id:int}")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult> Update(int id, [FromForm] ProductCreateUpdateDto dto, IFormFile? image)
    {
        var p = await _ctx.Products.FindAsync(id);
        if (p is null) return NotFound();
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        p.Name = dto.Name;
        p.Category = dto.Category ?? string.Empty;
        p.Color = dto.Color ?? string.Empty;
        p.Description = dto.Description ?? string.Empty;
        p.Price = dto.Price;
        p.Stock = dto.Stock;
        try
        {
            if (image != null)
            {
                p.ImageUrl = await SaveImage(image);
            }
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Product update validation failed");
            return BadRequest(new { message = vex.Message });
        }
        catch (DbUpdateException dbx)
        {
            _logger.LogError(dbx, "DB error updating product");
            return Problem(detail: dbx.InnerException?.Message ?? dbx.Message, statusCode: 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating product");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var p = await _ctx.Products.FindAsync(id);
        if (p is null) return NotFound();
        _ctx.Products.Remove(p);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> SaveImage(IFormFile file)
    {
        var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadsRoot);
        var ext = Path.GetExtension(file.FileName);
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext.ToLower())) throw new ValidationException("Invalid image type");
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(uploadsRoot, fileName);
        using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return $"{baseUrl}/uploads/{fileName}";
    }
}
