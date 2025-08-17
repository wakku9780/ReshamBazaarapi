using Microsoft.AspNetCore.Identity;

namespace ReshamBazaar.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
