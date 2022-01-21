using System;
using System.ComponentModel.DataAnnotations;
using AzisFood.DataEngine.Abstractions.Interfaces;

namespace AzisFood.DataEngine.Postgres.Models;

public abstract class PgRepoEntity : IRepoEntity
{
    /// <summary>
    /// Identifier
    /// </summary>
    [Key]
    public Guid Id { get; set; }
}