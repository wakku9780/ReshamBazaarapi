using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using ReshamBazaar.Api.Services;
using System.Text.RegularExpressions;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc)
    {
        _svc = svc;
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
        var result = await _svc.GetAsync(new ProductQuery
        {
            Q = q,
            Category = category,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Price = price,
            SortBy = sortBy,
            SortDir = sortDir,
            Page = page,
            PageSize = pageSize
        });
        // Normalize image URLs without mutating init-only record properties
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        if (result?.Items != null)
        {
            result.Items = result.Items.Select(item =>
            {
                if (!string.IsNullOrWhiteSpace(item.ImageUrl) && item.ImageUrl.StartsWith("/"))
                {
                    return item with { ImageUrl = baseUrl + item.ImageUrl };
                }
                return item;
            }).ToList();
        }
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductReadDto>> GetById(int id)
    {
        var p = await _svc.GetByIdAsync(id);
        if (p == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(p.ImageUrl) && p.ImageUrl.StartsWith("/"))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var p2 = p with { ImageUrl = baseUrl + p.ImageUrl };
            return Ok(p2);
        }
        return Ok(p);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductReadDto>> Create(ProductCreateDto dto)
    {
        var created = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        try
        {
            await _svc.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
