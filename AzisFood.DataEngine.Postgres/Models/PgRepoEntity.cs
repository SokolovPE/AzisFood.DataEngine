using System;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.Postgres.Models;

[SupportedBy(DatabaseType.Postgres)]
[MessagePackObject]
public abstract class PgRepoEntity<TDbContext> : IRepoEntity where TDbContext : DbContext
{
    protected PgRepoEntity()
    {
        Id = Guid.NewGuid();
    }
    
    /// <summary>
    ///     Identifier
    /// </summary>
    [System.ComponentModel.DataAnnotations.Key]
    [Key(0)]
    public Guid Id { get; set; }
}