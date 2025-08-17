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
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll()
    {
        var items = await _ctx.Products.AsNoTracking().ToListAsync();
        var result = items.Select(p => new ProductReadDto(p.Id, p.Name, p.Description, p.Color, p.Price, p.Stock, p.ImageUrl));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductReadDto>> GetById(int id)
    {
        var p = await _ctx.Products.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProductReadDto(p.Id, p.Name, p.Description, p.Color, p.Price, p.Stock, p.ImageUrl));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductReadDto>> Create(ProductCreateDto dto)
    {
        var p = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl
        };
        _ctx.Products.Add(p);
        await _ctx.SaveChangesAsync();
        var read = new ProductReadDto(p.Id, p.Name, p.Description, p.Color, p.Price, p.Stock, p.ImageUrl);
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
