using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Repositories;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken ct = default);
}
