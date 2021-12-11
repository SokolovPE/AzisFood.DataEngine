using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Interfaces;
using AzisFood.DataEngine.Mongo.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace AzisFood.DataEngine.Mongo.Implementations
{
    public class MongoBaseRepository<TRepoEntity> : IBaseRepository<TRepoEntity> where TRepoEntity: MongoRepoEntity
    {
        public string RepoEntityName { get; init; }
        private readonly ILogger<MongoBaseRepository<TRepoEntity>> _logger;
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

        // constructor for tests
        public MongoBaseRepository(ILogger<MongoBaseRepository<TRepoEntity>> logger, IMongoOptions mongoOptions,
            IMongoClient mongoClient)
        {
            _logger = logger;
            var database = mongoClient.GetDatabase(mongoOptions.DatabaseName);

            // Fill constants
            RepoEntityName = typeof(TRepoEntity).Name;

            Items = database.GetCollection<TRepoEntity>(RepoEntityName);
        }

        public virtual async Task<IEnumerable<TRepoEntity>> GetAsync(CancellationToken token = default)
        {
            _logger.LogInformation($"Requested all {RepoEntityName} items");
            try
            {
                token.ThrowIfCancellationRequested();
                var repoEntities =
                    (await Items.FindAsync(filter: FilterDefinition<TRepoEntity>.Empty, cancellationToken: token))
                    .ToEnumerable(token);
                _logger.LogInformation(
                    $"Request of all {RepoEntityName} items succeeded");
                return repoEntities;
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
            _logger.LogInformation($"Requested {RepoEntityName} with id: {id}");
            try
            {
                token.ThrowIfCancellationRequested();
                var repoEntity = await (await Items.FindAsync(item => item.Id == id, null, token))
                    .FirstOrDefaultAsync(token);
                _logger.LogInformation($"Request of {RepoEntityName} with id: {id} succeeded");
                return repoEntity;
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

        public virtual async Task<IEnumerable<TRepoEntity>> GetAsync(Expression<Func<TRepoEntity, bool>> filter,
            CancellationToken token = default)
        {
            _logger.LogInformation($"Requested filtered {RepoEntityName} items, filter: {filter}");
            try
            {
                token.ThrowIfCancellationRequested();
                var repoEntities = await (await Items.FindAsync(filter, cancellationToken: token)).ToListAsync(token);
                _logger.LogInformation(
                    $"Request of filtered {RepoEntityName} items returned {repoEntities.Count} items, filter: {filter}");
                return repoEntities;
            }
            catch (OperationCanceledException)
            {
                // Throw cancelled operation, do not catch
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"There was an error during attempt to return {RepoEntityName} items with filter {filter}");
                return default;
            }
        }

        public virtual async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
        {
            _logger.LogInformation($"Requested creation of {RepoEntityName}: {JsonConvert.SerializeObject(item)}");
            try
            {
                token.ThrowIfCancellationRequested();
                // Assign unique id
                item.Id = ObjectId.GenerateNewId().ToString();
                await Items.InsertOneAsync(item, cancellationToken: token);
                _logger.LogInformation($"Requested creation of {RepoEntityName} succeeded");
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
            _logger.LogInformation(
                $"Requested update of {RepoEntityName} with id {id} with new value: {JsonConvert.SerializeObject(itemIn)}");
            try
            {
                await Items.ReplaceOneAsync(item => item.Id == id, itemIn, cancellationToken: token);
                _logger.LogInformation($"Requested update of {RepoEntityName} succeeded");
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
            _logger.LogInformation($"Requested delete of {RepoEntityName}: {JsonConvert.SerializeObject(itemIn)}");
            try
            {
                await Items.DeleteOneAsync(item => item.Id == itemIn.Id, token);
                _logger.LogInformation($"Requested delete of {RepoEntityName} succeeded");
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
            _logger.LogInformation($"Requested delete of {RepoEntityName} with id {id}");
            try
            {
                await Items.DeleteOneAsync(item => item.Id == id, token);
                _logger.LogInformation($"Requested delete of {RepoEntityName} succeeded");
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
        
        public virtual async Task RemoveAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
        {
            _logger.LogInformation($"Requested delete of {RepoEntityName} with filter {filter}");
            try
            {
                await Items.DeleteManyAsync(filter, token);
                _logger.LogInformation($"Requested delete of {RepoEntityName} with filter {filter} succeeded");
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
            _logger.LogInformation(
                $"Requested delete of multiple {RepoEntityName} with ids {JsonConvert.SerializeObject(ids)}");
            try
            {
                await Items.DeleteManyAsync(item => ids.Contains(item.Id), token);
                _logger.LogInformation($"Requested delete of multiple {RepoEntityName} succeeded");
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