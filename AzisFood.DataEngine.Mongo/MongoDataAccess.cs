#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Mongo.Models;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo
{
    /// <inheritdoc />
    public class MongoDataAccess<TRepoEntity> : IDataAccess<TRepoEntity> where TRepoEntity: MongoRepoEntity
    {
        private readonly IMongoCollection<TRepoEntity> _items;
        public MongoDataAccess(IMongoOptions mongoOptions)
        {
            var client = new MongoClient(mongoOptions.ConnectionString);
            var database = client.GetDatabase(mongoOptions.DatabaseName);
            
            _items = database.GetCollection<TRepoEntity>(typeof(TRepoEntity).Name);
        }
        
        // For tests
        public MongoDataAccess(IMongoOptions mongoOptions, IMongoCollection<TRepoEntity> collection)
        {
            _items = collection;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TRepoEntity>> GetAllAsync(CancellationToken token = default) =>
            (await _items.FindAsync(FilterDefinition<TRepoEntity>.Empty, cancellationToken: token))
            .ToEnumerable(token);

        /// <inheritdoc />
        public async Task<TRepoEntity?> GetAsync(Guid id, CancellationToken token = default) =>
            await (await _items.FindAsync(item => item.Id == id, null, token))
                .FirstOrDefaultAsync(token);

        /// <inheritdoc />
        public async Task<IEnumerable<TRepoEntity>> GetAsync(Expression<Func<TRepoEntity, bool>> filter,
            CancellationToken token = default) =>
            await (await _items.FindAsync(filter, cancellationToken: token)).ToListAsync(token);

        /// <inheritdoc />
        public async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
        {
            item.Id = Guid.NewGuid();
            await _items.InsertOneAsync(item, cancellationToken: token);
            return item;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Guid id, TRepoEntity itemIn, CancellationToken token = default)
        {
            itemIn.Id = id;
            await _items.ReplaceOneAsync(item => item.Id == id, itemIn, cancellationToken: token);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(TRepoEntity itemIn, CancellationToken token = default) =>
            await _items.DeleteOneAsync(item => item.Id == itemIn.Id, token);

        /// <inheritdoc />
        public async Task RemoveAsync(Guid id, CancellationToken token = default) =>
            await _items.DeleteOneAsync(item => item.Id == id, token);

        /// <inheritdoc />
        public async Task RemoveAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default) =>
            await _items.DeleteManyAsync(filter, token);

        /// <inheritdoc />
        public async Task RemoveManyAsync(Guid[] ids, CancellationToken token = default) =>
            await _items.DeleteManyAsync(item => ((IList) ids).Contains(item.Id), token);
    }
}