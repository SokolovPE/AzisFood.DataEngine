using System;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.Postgres.Attributes;

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