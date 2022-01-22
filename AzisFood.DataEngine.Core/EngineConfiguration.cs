namespace AzisFood.DataEngine.Core;

/// <summary>
///     Additional configuration of postgres
/// </summary>
public class EngineConfiguration
{
    public EngineConfiguration(bool contextAutoRegister = false)
    {
        ContextContextAutoRegister = contextAutoRegister;
    }

    /// <summary>
    ///     Automatically register all available db contexts
    /// </summary>
    public bool ContextContextAutoRegister { get; }
}