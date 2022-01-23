using AzisFood.DataEngine.Core.Attributes;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.ManualTest.Models.Postgres;

[ConnectionAlias("postgres")]
public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; } = null!;
}