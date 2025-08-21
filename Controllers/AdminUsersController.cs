using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSummaryDto>>> List([FromQuery] string? email, [FromQuery] string? role, [FromQuery] bool? blocked)
    {
        var q = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var e = email.Trim().ToLower();
            q = q.Where(u => u.Email != null && u.Email.ToLower().Contains(e));
        }
        if (blocked.HasValue)
        {
            q = q.Where(u => u.IsBlocked == blocked.Value);
        }
        var users = await q.Take(500).ToListAsync();
        var result = new List<UserSummaryDto>(users.Count);
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role)) continue;
            result.Add(new UserSummaryDto(u.Id, u.Email ?? string.Empty, u.FullName, u.EmailConfirmed, u.IsBlocked, roles));
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserSummaryDto>> GetById(string id)
    {
        var u = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        var roles = await _userManager.GetRolesAsync(u);
        return Ok(new UserSummaryDto(u.Id, u.Email ?? string.Empty, u.FullName, u.EmailConfirmed, u.IsBlocked, roles));
    }

    [HttpPatch("{id}/block")]
    public async Task<ActionResult> Block(string id)
    {
        var u = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        u.IsBlocked = true;
        var result = await _userManager.UpdateAsync(u);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        var roles = await _userManager.GetRolesAsync(u);
        var dto = new UserSummaryDto(u.Id, u.Email ?? string.Empty, u.FullName, u.EmailConfirmed, u.IsBlocked, roles);
        return Ok(dto);
    }

    [HttpPatch("{id}/unblock")]
    public async Task<ActionResult> Unblock(string id)
    {
        var u = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        u.IsBlocked = false;
        var result = await _userManager.UpdateAsync(u);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        var roles = await _userManager.GetRolesAsync(u);
        var dto = new UserSummaryDto(u.Id, u.Email ?? string.Empty, u.FullName, u.EmailConfirmed, u.IsBlocked, roles);
        return Ok(dto);
    }
}
