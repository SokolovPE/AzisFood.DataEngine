namespace AzisFood.DataEngine.Postgres.Models;

/// <summary>
///     Postgres options
/// </summary>
public class PgOptions
{
    public PgConnectOptions[] Connections { get; set; }
}