using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Services;

public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
}
