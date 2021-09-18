using AzisFood.DataEngine.Interfaces;

namespace AzisFood.DataEngine.Mongo.Implementations
{
    public class MongoOptions : IMongoOptions
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}