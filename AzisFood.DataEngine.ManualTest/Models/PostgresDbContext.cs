using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.ManualTest.Models;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Order> Orders { get; set; } = null!;
}