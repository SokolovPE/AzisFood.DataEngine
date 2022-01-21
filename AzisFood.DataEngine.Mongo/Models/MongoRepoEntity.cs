using System;
using AzisFood.CacheService.Abstractions.Models;
using AzisFood.DataEngine.Abstractions.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace AzisFood.DataEngine.Mongo.Models
{
    public abstract class MongoRepoEntity : IRepoEntity
    {
        /// <summary>
        /// Identifier
        /// </summary>
        [HashEntryKey]
        public Guid Id { get; set; }
        
        protected MongoRepoEntity()
        {
            Id = Guid.NewGuid();
        }
    }
}