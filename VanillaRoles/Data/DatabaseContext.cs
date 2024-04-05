using Microsoft.EntityFrameworkCore;

namespace VanillaRoles.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Link> Links { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
}
