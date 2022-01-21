using System;
using AzisFood.CacheService.Abstractions.Models;
using AzisFood.DataEngine.Abstractions.Interfaces;

namespace AzisFood.DataEngine.Mongo.Models;

public abstract class MongoRepoEntity : IRepoEntity
{
    protected MongoRepoEntity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    ///     Identifier
    /// </summary>
    [HashEntryKey]
    public Guid Id { get; set; }
}