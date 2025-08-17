using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Services;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}
