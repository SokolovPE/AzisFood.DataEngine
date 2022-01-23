using System;
using System.ComponentModel.DataAnnotations;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.Postgres.Models;

[SupportedBy(DatabaseType.Postgres)]
public abstract class PgRepoEntity<TDbContext> : IRepoEntity where TDbContext : DbContext
{
    /// <summary>
    ///     Identifier
    /// </summary>
    [Key]
    public Guid Id { get; set; }
}