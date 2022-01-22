using AzisFood.DataEngine.Core.Attributes;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.ManualTest.Models.Postgres;

[ConnectionSettings("catalog")]
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;
}