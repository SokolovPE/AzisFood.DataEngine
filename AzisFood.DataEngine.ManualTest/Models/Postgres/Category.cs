using System.ComponentModel.DataAnnotations;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Postgres.Models;

namespace AzisFood.DataEngine.ManualTest.Models.Postgres;

[SupportedBy(DatabaseType.Postgres)]
[UseContext(nameof(CatalogDbContext))]
public class Category : PgRepoEntity
{
    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public int Order { get; set; }
}