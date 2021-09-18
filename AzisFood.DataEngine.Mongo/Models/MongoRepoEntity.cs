using AzisFood.CacheService.Abstractions.Models;
using AzisFood.DataEngine.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AzisFood.DataEngine.Mongo.Models
{
    public abstract class MongoRepoEntity : IRepoEntity
    {
        /// <summary>
        /// Identifier
        /// </summary>
        [BsonId]
        [HashEntryKey]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        protected MongoRepoEntity()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }
    }
}