using System.ComponentModel.DataAnnotations;
using AzisFood.DataEngine.Postgres.Models;

namespace AzisFood.DataEngine.ManualTest.Models.Postgres;

public class Category : PgRepoEntity<CatalogDbContext>
{
    [Required] public string Title { get; set; } = null!;

    [Required] public int Order { get; set; }
}