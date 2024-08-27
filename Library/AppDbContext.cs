using Microsoft.EntityFrameworkCore;

namespace Library;

public class AppDbContext : DbContext
{
    public DbSet<NodeEntity> Nodes { get; init; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
