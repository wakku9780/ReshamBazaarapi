using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public AdminOrdersController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> List(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? userId)
    {
        var q = _ctx.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var st))
        {
            q = q.Where(o => o.Status == st);
        }
        if (from.HasValue) q = q.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(o => o.CreatedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(userId)) q = q.Where(o => o.UserId == userId);

        var list = await q.OrderByDescending(o => o.Id).Take(500).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var o = await _ctx.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        return o is null ? NotFound() : Ok(o);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto dto)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
        {
            return BadRequest(new { message = "Invalid status. Allowed: Pending, Shipped, Delivered, Cancelled" });
        }
        var o = await _ctx.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (o is null) return NotFound();
        o.Status = newStatus;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
