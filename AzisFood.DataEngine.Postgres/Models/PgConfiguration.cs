namespace AzisFood.DataEngine.Postgres.Models;

/// <summary>
///     Postgres options
/// </summary>
public class PgConfiguration
{
    public PgConnectOptions[] Connections { get; set; }
}