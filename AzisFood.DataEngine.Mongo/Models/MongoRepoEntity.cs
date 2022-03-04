using System;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using MessagePack;

namespace AzisFood.DataEngine.Mongo.Models;

[SupportedBy(DatabaseType.Mongo)]
[MessagePackObject]
public abstract class MongoRepoEntity : IRepoEntity
{
    protected MongoRepoEntity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    ///     Identifier
    /// </summary>
    [Key(0)]
    public Guid Id { get; set; }
}