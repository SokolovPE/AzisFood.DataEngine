namespace AzisFood.DataEngine.Postgres.Models;

/// <summary>
///     Additional configuration of postgres
/// </summary>
public class PgConfiguration
{
    public PgConfiguration(bool contextAutoRegister = false)
    {
        ContextContextAutoRegister = contextAutoRegister;
    }

    /// <summary>
    ///     Automatically register all available db contexts
    /// </summary>
    public bool ContextContextAutoRegister { get; set; }
}