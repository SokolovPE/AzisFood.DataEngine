using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.ManualTest.Models;

public class PgDbContext : DbContext
{
    public PgDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    public DbSet<Category> Categories { get; set; }
}