using System;
using System.ComponentModel.DataAnnotations;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;

namespace AzisFood.DataEngine.Postgres.Models;

[SupportedBy(DatabaseType.Postgres)]
public abstract class PgRepoEntity : IRepoEntity
{
    /// <summary>
    ///     Identifier
    /// </summary>
    [Key]
    public Guid Id { get; set; }
}