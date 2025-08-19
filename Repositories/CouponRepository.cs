using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _ctx;
    public CouponRepository(AppDbContext ctx) { _ctx = ctx; }

    public Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var norm = (code ?? string.Empty).Trim().ToUpperInvariant();
        return _ctx.Coupons.FirstOrDefaultAsync(c => c.Code == norm, ct);
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken ct = default)
    {
        return await _ctx.Coupons.AsNoTracking().OrderBy(c => c.Code).ToListAsync(ct);
    }
}
