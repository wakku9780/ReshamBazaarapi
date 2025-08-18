// WARNING: This file was accidentally created in Controllers/. The real DbContext is in Data/AppDbContext.cs.
// Keeping this stub with a different class name to avoid build conflicts. Do NOT use this type.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Models;

namespace ReshamBazaar.Api.Data;

public class ObsoleteAppDbContext : IdentityDbContext<ApplicationUser>
{
    public ObsoleteAppDbContext(DbContextOptions<ObsoleteAppDbContext> options) : base(options) { }
}
