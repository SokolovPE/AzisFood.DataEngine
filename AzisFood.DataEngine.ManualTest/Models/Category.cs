using System.ComponentModel.DataAnnotations;
using AzisFood.DataEngine.Postgres.Models;

namespace AzisFood.DataEngine.ManualTest.Models;

public class Category : PgRepoEntity
{
    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public int Order { get; set; }
}