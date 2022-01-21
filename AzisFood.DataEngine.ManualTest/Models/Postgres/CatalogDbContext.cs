using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.ManualTest.Models.Postgres;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;
}