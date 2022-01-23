using System;
using AzisFood.CacheService.Abstractions.Models;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;

namespace AzisFood.DataEngine.Mongo.Models;

[SupportedBy(DatabaseType.Mongo)]
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