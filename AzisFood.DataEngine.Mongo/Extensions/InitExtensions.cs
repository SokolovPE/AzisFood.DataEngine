using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Implementations;
using AzisFood.DataEngine.Mongo.Implementations;
using AzisFood.DataEngine.Mongo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AzisFood.DataEngine.Mongo.Extensions
{
    public static class InitExtensions
    {
        public static void AddMongoDBSupport(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<MongoOptions>(configuration.GetSection(nameof(MongoOptions)));
            serviceCollection.AddSingleton<IMongoOptions>(sp =>
                sp.GetRequiredService<IOptions<MongoOptions>>().Value);
            serviceCollection.AddTransient<IDataAccess,MongoDataAccess>();
            serviceCollection.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            serviceCollection.AddTransient(typeof(ICachedBaseRepository<>), typeof(CachedBaseRepository<>));
            serviceCollection.AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
            
            // Register mapping of Guid to string of MongoDb
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        }
    }
}