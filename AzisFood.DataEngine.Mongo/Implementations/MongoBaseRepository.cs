using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzisFood.DataEngine.Interfaces;
using AzisFood.DataEngine.Mongo.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo.Implementations
{
    // TODO: Exception handling!!!
    public class MongoBaseRepository<TRepoEntity> : IBaseRepository<TRepoEntity> where TRepoEntity: MongoRepoEntity
    {
        private readonly ILogger<MongoBaseRepository<TRepoEntity>> _logger;
        protected readonly string RepoEntityName;
        protected IMongoCollection<TRepoEntity> Items;
        
        public MongoBaseRepository(ILogger<MongoBaseRepository<TRepoEntity>> logger, IMongoOptions mongoOptions)
        {
            _logger = logger;
            var client = new MongoClient(mongoOptions.ConnectionString);
            var database = client.GetDatabase(mongoOptions.DatabaseName);

            // Fill constants
            RepoEntityName = typeof(TRepoEntity).Name;

            Items = database.GetCollection<TRepoEntity>(RepoEntityName);
        }
        
        public virtual async Task<IEnumerable<TRepoEntity>> GetAsync() => 
            (await Items.FindAsync(item => true)).ToEnumerable();

        public virtual async Task<TRepoEntity> GetAsync(string id) =>
            await (await Items.FindAsync(item => item.Id == id)).FirstOrDefaultAsync();

        public virtual async Task<TRepoEntity> CreateAsync(TRepoEntity item)
        {
            // Assign unique id
            item.Id = ObjectId.GenerateNewId().ToString();
            await Items.InsertOneAsync(item);
            return item;
        }
        
        public virtual async Task UpdateAsync(string id, TRepoEntity itemIn) =>
            await Items.ReplaceOneAsync(item => item.Id == id, itemIn);

        public virtual async Task RemoveAsync(TRepoEntity itemIn) =>
            await Items.DeleteOneAsync(item => item.Id == itemIn.Id);

        public virtual async Task RemoveAsync(string id) =>
            await Items.DeleteOneAsync(item => item.Id == id);

        public virtual async Task RemoveManyAsync(string[] ids) =>
            await Items.DeleteManyAsync(item => ids.Contains(item.Id));
    }
}