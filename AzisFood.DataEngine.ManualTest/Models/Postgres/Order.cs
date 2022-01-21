using System.ComponentModel.DataAnnotations;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Postgres.Models;

namespace AzisFood.DataEngine.ManualTest.Models.Postgres;

[SupportedBy(DatabaseType.Postgres)]
[UseContext(nameof(PostgresDbContext))]
public class Order : PgRepoEntity
{
    /// <summary>
    /// Date of order
    /// </summary>
    [Required]
    public DateTimeOffset OrderDate { get; set; }
    
    /// <summary>
    /// Total price
    /// </summary>
    [Required]
    public decimal  Price { get; set; }
    
    /// <summary>
    /// Quantity of items
    /// </summary>
    [Required]
    public int  Qty { get; set; }
    
    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
}