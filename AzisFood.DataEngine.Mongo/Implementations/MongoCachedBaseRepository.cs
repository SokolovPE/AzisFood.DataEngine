using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.CacheService.Redis.Interfaces;
using AzisFood.DataEngine.Interfaces;
using AzisFood.DataEngine.Mongo.Models;
using AzisFood.MQ.Abstractions.Interfaces;
using AzisFood.MQ.Abstractions.Models;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Tag;
using StackExchange.Redis;

namespace AzisFood.DataEngine.Mongo.Implementations
{
    public class MongoCachedBaseRepository<TRepoEntity> : ICachedBaseRepository<TRepoEntity> where TRepoEntity: MongoRepoEntity
    {
        public string RepoEntityName { get; init; }
        private readonly ILogger<MongoCachedBaseRepository<TRepoEntity>> _logger;
        private readonly IRedisCacheService _cacheService;
        private readonly IProducerService<TRepoEntity> _producerService;
        private readonly ITracer _tracer;
        
        private readonly IBaseRepository<TRepoEntity> _base;

        public MongoCachedBaseRepository(IBaseRepository<TRepoEntity> @base, ILogger<MongoCachedBaseRepository<TRepoEntity>> logger,
            IRedisCacheService cacheService,
            ITracer tracer, IProducerService<TRepoEntity> producerService)
        {
            _logger = logger;
            _cacheService = cacheService;
            _tracer = tracer;
            _producerService = producerService;
            _base = @base;
            RepoEntityName = _base.RepoEntityName;
        }

        public async Task<IEnumerable<TRepoEntity>> GetAsync(CancellationToken token = default) =>
            await Get(false, token);

        public async Task<TRepoEntity> GetAsync(string id, CancellationToken token = default) =>
            await Get(id, false, token);

        public async Task<IEnumerable<TRepoEntity>> GetAsync(Expression<Func<TRepoEntity, bool>> filter,
            CancellationToken token = default) =>
            await GetFiltered(filter, false, token);

        public async Task<IEnumerable<TRepoEntity>> GetHashAsync(CancellationToken token = default) =>
            await Get(token: token);

        public async Task<TRepoEntity> GetHashAsync(string id, CancellationToken token = default) =>
            await Get(id, token: token);

        public async Task<IEnumerable<TRepoEntity>> GetHashAsync(Expression<Func<TRepoEntity, bool>> filter,
            CancellationToken token = default) =>
            await GetFiltered(filter, token: token);

        public async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.create").StartActive();
            try
            {
                var insertSpan = _tracer.BuildSpan("insertion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await _base.CreateAsync(item, token);
                insertSpan.Finish();
                return item;
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public async Task UpdateAsync(string id, TRepoEntity itemIn, CancellationToken token = default)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.update").StartActive();
            try
            {
                var replaceSpan = _tracer.BuildSpan("replace").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await _base.UpdateAsync(id, itemIn, token);
                replaceSpan.Finish();
                await _producerService.SendEvent(token: token);
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public async Task RemoveAsync(TRepoEntity itemIn, CancellationToken token = default)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.delete").StartActive();
            try
            {
                var deleteSpan = _tracer.BuildSpan("deletion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await _base.RemoveAsync(itemIn, token);
                deleteSpan.Finish();
                await _producerService.SendEvent(token: token);
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public async Task RemoveAsync(string id, CancellationToken token = default)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.delete").StartActive();
            try
            {
                var deleteSpan = _tracer.BuildSpan("deletion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await _base.RemoveAsync(id, token);
                deleteSpan.Finish();
                await _producerService.SendEvent(eventType: EventType.Deleted, payload: id, token: token);
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public async Task RemoveAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.delete-filtered").StartActive();
            try
            {
                var deleteSpan = _tracer.BuildSpan("deletion-filtered").WithTag(Tags.DbType, "Mongo")
                    .AsChildOf(mainSpan.Span)
                    .Start();
                await _base.RemoveAsync(filter, token);
                deleteSpan.Finish();
                await _producerService.SendEvent(eventType: EventType.Deleted, token: token);
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public async Task RemoveManyAsync(string[] ids, CancellationToken token = default)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.delete").StartActive();
            try
            {
                var deleteSpan = _tracer.BuildSpan("deletion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await _base.RemoveManyAsync(ids, token);
                deleteSpan.Finish();
                await _producerService.SendEvent(token: token);
                // TODO: On deletion no need fully recache - just remove hash entry!
                await _producerService.SendEvent(eventType: EventType.Deleted, payload: ids, token: token);
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }
        
        /// <summary>
        /// Get items from cache
        /// </summary>
        /// <param name="hashMode">Get from HashSet</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Entries of entity</returns>
        private async Task<IEnumerable<TRepoEntity>> Get(bool hashMode = true, CancellationToken token = default)
        {
            _logger.LogInformation($"Requested all {RepoEntityName} items");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.get-all").StartActive();
            try
            {
                token.ThrowIfCancellationRequested();
                var redisResult = hashMode
                    ? await _cacheService.HashGetAllAsync<TRepoEntity>(CommandFlags.None)
                    : await _cacheService.GetAsync<List<TRepoEntity>>(RepoEntityName);

                if (redisResult != null)
                {
                    var mongoRepoEntities = redisResult.ToArray();
                    if (mongoRepoEntities.Length > 0)
                    {
                        _logger.LogInformation(
                            $"Request of all {RepoEntityName} items returned {mongoRepoEntities.Length} items");
                        return mongoRepoEntities;
                    }
                }

                _logger.LogWarning($"Items of type {RepoEntityName} are not presented in cache");
                var dbSpan = _tracer.BuildSpan("mongo-cached-repo.get.db").WithTag(Tags.DbType, "Mongo")
                    .AsChildOf(mainSpan.Span).Start();
                var dbResult = await _base.GetAsync(token);
                dbSpan.Finish();
                await _producerService.SendEvent(token: token);

                var repoEntities = dbResult as TRepoEntity[] ?? dbResult.ToArray();
                _logger.LogInformation($"Request of all {RepoEntityName} items returned {repoEntities.Length} items");
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
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        /// <summary>
        /// Get items from cache
        /// </summary>
        /// <param name="hashMode">Get from HashSet</param>
        /// <param name="filter">Condition to filter</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Entries of entity</returns>
        private async Task<IEnumerable<TRepoEntity>> GetFiltered(Expression<Func<TRepoEntity, bool>> filter,
            bool hashMode = true, CancellationToken token = default)
        {
            _logger.LogInformation($"Requested filtered {RepoEntityName} items, filter: {filter}");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.get-filtered").StartActive();
            try
            {
                token.ThrowIfCancellationRequested();
                var redisResult = hashMode
                    ? await _cacheService.HashGetAllAsync<TRepoEntity>(CommandFlags.None)
                    : await _cacheService.GetAsync<List<TRepoEntity>>(RepoEntityName);

                if (redisResult != null)
                {
                    var mongoRepoEntities = redisResult.Where(filter.Compile()).ToArray();
                    if (mongoRepoEntities.Length > 0)
                    {
                        _logger.LogInformation(
                            $"Request of filtered {RepoEntityName} items returned {mongoRepoEntities.Length} items, filter: {filter}");
                        return mongoRepoEntities;
                    }
                }

                _logger.LogWarning($"Items of type {RepoEntityName} are not presented in cache");
                var dbSpan = _tracer.BuildSpan("mongo-cached-repo.get-filtered.db").WithTag(Tags.DbType, "Mongo")
                    .AsChildOf(mainSpan.Span).Start();
                var dbResult = await _base.GetAsync(filter, token);
                dbSpan.Finish();
                await _producerService.SendEvent(token: token);

                var repoEntities = dbResult as TRepoEntity[] ?? dbResult.ToArray();
                _logger.LogInformation(
                    $"Request of filtered {RepoEntityName} items returned {repoEntities.Length} items, filter: {filter}");
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
                    $"There was an error during attempt to return filtered {RepoEntityName} items, filter: {filter}");
                return default;
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        /// <summary>
        /// Get item from cache
        /// </summary>
        /// <param name="id">Identifier of entry</param>
        /// <param name="hashMode">Get from HashSet</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Entry of entity</returns>
        private async Task<TRepoEntity> Get(string id, bool hashMode = true, CancellationToken token = default)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.get").StartActive();
            try
            {
                var redisResult = hashMode
                    ? await _cacheService.HashGetAsync<TRepoEntity>(id, CommandFlags.None)
                    : (await _cacheService.GetAsync<List<TRepoEntity>>(RepoEntityName))?.FirstOrDefault(
                        x => x.Id == id);

                if (redisResult != null)
                {
                    _logger.LogInformation($"Request of {RepoEntityName} with id: {id} succeeded");
                    return redisResult;
                }

                _logger.LogWarning($"Item of type {RepoEntityName}  with id: {id} is not presented in cache");
                var dbSpan = _tracer.BuildSpan("mongo-cached-repo.get.db").WithTag(Tags.DbType, "Mongo")
                    .AsChildOf(mainSpan.Span).Start();
                var dbResult = await _base.GetAsync(id, token);
                dbSpan.Finish();
                await _producerService.SendEvent(token: token);
                _logger.LogInformation($"Request of {RepoEntityName} with id: {id} succeeded");
                return dbResult;
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
            finally
            {
                mainSpan.Span.Finish();
            }
        }
    }
}