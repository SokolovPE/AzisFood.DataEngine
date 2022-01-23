using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo.Models;

public class MongoConnectConfiguration
{
    public string ConnectionString { get; set; }
    public string Alias { get; set; }

    public MongoUrl GetMongoUrl => MongoUrl.Create(ConnectionString);
}