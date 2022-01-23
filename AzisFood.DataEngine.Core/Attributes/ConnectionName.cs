using System;

namespace AzisFood.DataEngine.Core.Attributes;

/// <summary>
///     Define connection settings
/// </summary>
public class ConnectionAlias : Attribute
{
    public ConnectionAlias(string alias)
    {
        Alias = alias;
    }

    /// <summary>
    ///     Alias of connection defined in settings
    /// </summary>
    public string Alias { get; set; }
}