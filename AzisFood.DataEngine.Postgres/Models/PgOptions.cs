namespace AzisFood.DataEngine.Postgres.Models;

/// <summary>
///     Postgres options
/// </summary>
public class PgOptions
{
    public PgConnect[] Connections { get; set; }
}