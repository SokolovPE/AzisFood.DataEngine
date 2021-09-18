using AzisFood.DataEngine.Interfaces;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo.Implementations
{
    public class MongoService : IMongoService
    {
        public IMongoCollection<TRepoEntity> GetCollection<TRepoEntity>(IMongoOptions options)
        {
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DatabaseName);

            return database.GetCollection<TRepoEntity>(typeof(TRepoEntity).Name);
        }
    }
}