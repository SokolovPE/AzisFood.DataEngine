using System;

namespace AzisFood.DataEngine.Core.Attributes;

/// <summary>
///     Define connection settings
/// </summary>
public class ConnectionSettings : Attribute
{
    public ConnectionSettings(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Name of connection defined in settings
    /// </summary>
    public string Name { get; set; }
}