using System;

namespace AzisFood.DataEngine.Core.Attributes;

/// <summary>
/// Which context should be used
/// </summary>
public class UseContext : Attribute
{
    public string ContextName { get; set; }
    
    public UseContext(string contextName)
    {
        ContextName = contextName;
    }
}