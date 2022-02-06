namespace AzisFood.DataEngine.Postgres.Models;

/// <summary>
///     Postgres options
/// </summary>
public class PgConfiguration
{
    /// <summary>
    ///     Automatically register all configured connections
    /// </summary>
    public bool AutoRegistration { get; set; } = true;

    /// <summary>
    ///     Connection configurations
    /// </summary>
    public PgConnectOptions[] Connections { get; set; }
}