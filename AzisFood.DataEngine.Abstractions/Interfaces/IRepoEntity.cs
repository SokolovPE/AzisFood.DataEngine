using System;

namespace AzisFood.DataEngine.Abstractions.Interfaces;

/// <summary>
///     Abstract repository entity
/// </summary>
public interface IRepoEntity
{
    public Guid Id { get; set; }
}