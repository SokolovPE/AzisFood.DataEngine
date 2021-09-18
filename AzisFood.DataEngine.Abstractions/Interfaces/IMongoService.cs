using MongoDB.Driver;

namespace AzisFood.DataEngine.Interfaces
{
    /// <summary>
    /// Service to operate mongo entities
    /// </summary>
    public interface IMongoService
    {
        public IMongoCollection<TRepoEntity> GetCollection<TRepoEntity>(IMongoOptions options);
    }
}