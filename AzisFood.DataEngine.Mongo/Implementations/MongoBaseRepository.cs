using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Interfaces;
using AzisFood.DataEngine.Mongo.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo.Implementations
{
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

        public virtual async Task<IEnumerable<TRepoEntity>> GetAsync(CancellationToken token = default)
        {
            try
            {
                return (await Items.FindAsync(item => true, cancellationToken: token)).ToEnumerable(token);
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to return all {RepoEntityName} items");
                return default;
            }
        }

        public virtual async Task<TRepoEntity> GetAsync(string id, CancellationToken token = default)
        {
            try
            {
                return await (await Items.FindAsync(item => item.Id == id, cancellationToken: token))
                    .FirstOrDefaultAsync(token);
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to return {RepoEntityName} item");
                return default;
            }
        }

        public virtual async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
        {
            try
            {
                // Assign unique id
                item.Id = ObjectId.GenerateNewId().ToString();
                await Items.InsertOneAsync(item, cancellationToken: token);
                return item;
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to create {RepoEntityName}");
                return default;
            }
        }

        public virtual async Task UpdateAsync(string id, TRepoEntity itemIn, CancellationToken token = default)
        {
            try
            {
                await Items.ReplaceOneAsync(item => item.Id == id, itemIn, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to update {RepoEntityName}");
            }
        }

        public virtual async Task RemoveAsync(TRepoEntity itemIn, CancellationToken token = default)
        {
            try
            {
                await Items.DeleteOneAsync(item => item.Id == itemIn.Id, token);
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to delete {RepoEntityName}");
            }
        }

        public virtual async Task RemoveAsync(string id, CancellationToken token = default)
        {
            try
            {
                await Items.DeleteOneAsync(item => item.Id == id, token);
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to delete {RepoEntityName}");
            }
        }

        public virtual async Task RemoveManyAsync(string[] ids, CancellationToken token = default)
        {
            try
            {
                await Items.DeleteManyAsync(item => ids.Contains(item.Id), token);
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to delete multiple {RepoEntityName}");
            }
        }
    }
}