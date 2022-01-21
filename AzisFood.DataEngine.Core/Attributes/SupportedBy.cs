using System;

namespace AzisFood.DataEngine.Core.Attributes;

/// <summary>
/// Defines which database should be used
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SupportedBy : Attribute
{
    public DatabaseType Type { get; set; }

    public SupportedBy(DatabaseType type)
    {
        Type = type;
    }
}