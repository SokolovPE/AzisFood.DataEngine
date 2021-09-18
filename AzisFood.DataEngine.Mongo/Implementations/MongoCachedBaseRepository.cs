using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzisFood.CacheService.Redis.Interfaces;
using AzisFood.DataEngine.Interfaces;
using AzisFood.DataEngine.Mongo.Models;
using AzisFood.MQ.Abstractions.Interfaces;
using AzisFood.MQ.Abstractions.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using OpenTracing;
using OpenTracing.Tag;
using StackExchange.Redis;

namespace AzisFood.DataEngine.Mongo.Implementations
{
    public class MongoCachedBaseRepository<TRepoEntity> : MongoBaseRepository<TRepoEntity>, ICachedBaseRepository<TRepoEntity>
        where TRepoEntity : MongoRepoEntity
    {
        private readonly ILogger<MongoCachedBaseRepository<TRepoEntity>> _logger;
        private readonly IRedisCacheService _cacheService;
        private readonly IProducerService<TRepoEntity> _producerService;
        private readonly ITracer _tracer;

        public MongoCachedBaseRepository(ILogger<MongoCachedBaseRepository<TRepoEntity>> logger,
            IMongoOptions mongoOptions,
            IRedisCacheService cacheService,
            ITracer tracer, IProducerService<TRepoEntity> producerService) : base(logger, mongoOptions)
        {
            _logger = logger;
            _cacheService = cacheService;
            _tracer = tracer;
            _producerService = producerService;
        }

        public override async Task<IEnumerable<TRepoEntity>> GetAsync() =>
            await Get(false);

        public async Task<IEnumerable<TRepoEntity>> GetHashAsync() =>
            await Get();

        public override async Task<TRepoEntity> GetAsync(string id) =>
            await Get(id, false);

        public async Task<TRepoEntity> GetHashAsync(string id) =>
            await Get(id);

        public override async Task<TRepoEntity> CreateAsync(TRepoEntity item)
        {
            _logger.LogInformation($"Requested creation of {RepoEntityName}: {JsonConvert.SerializeObject(item)}");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.create").StartActive();
            try
            {
                // Assign unique id
                item.Id = ObjectId.GenerateNewId().ToString();
                var insertSpan = _tracer.BuildSpan("insertion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await Items.InsertOneAsync(item);
                insertSpan.Finish();
                await _producerService.SendEvent();
                _logger.LogInformation($"Requested creation of {RepoEntityName} succeeded");
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to create {RepoEntityName}");
                return default;
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public override async Task UpdateAsync(string id, TRepoEntity itemIn)
        {
            _logger.LogInformation(
                $"Requested update of {RepoEntityName} with id {id} with new value: {JsonConvert.SerializeObject(itemIn)}");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.update").StartActive();
            try
            {
                var replaceSpan = _tracer.BuildSpan("replace").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await Items.ReplaceOneAsync(item => item.Id == id, itemIn);
                replaceSpan.Finish();
                await _producerService.SendEvent();
                _logger.LogInformation($"Requested update of {RepoEntityName} succeeded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to update {RepoEntityName}");
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public override async Task RemoveAsync(TRepoEntity itemIn)
        {
            _logger.LogInformation($"Requested delete of {RepoEntityName}: {JsonConvert.SerializeObject(itemIn)}");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.delete").StartActive();
            try
            {
                var deleteSpan = _tracer.BuildSpan("deletion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await Items.DeleteOneAsync(item => item.Id == itemIn.Id);
                deleteSpan.Finish();
                await _producerService.SendEvent();
                _logger.LogInformation($"Requested delete of {RepoEntityName} succeeded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to delete {RepoEntityName}");
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public override async Task RemoveAsync(string id)
        {
            _logger.LogInformation($"Requested delete of {RepoEntityName} with id {id}");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.delete").StartActive();
            try
            {
                var deleteSpan = _tracer.BuildSpan("deletion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await Items.DeleteOneAsync(item => item.Id == id);
                deleteSpan.Finish();
                await _producerService.SendEvent();
                // TODO: On deletion no need fully recache - just remove hash entry!
                await _producerService.SendEvent(eventType: EventType.Deleted, payload: id);
                _logger.LogInformation($"Requested delete of {RepoEntityName} succeeded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to delete {RepoEntityName}");
            }
            finally
            {
                mainSpan.Span.Finish();
            }
        }

        public override async Task RemoveManyAsync(string[] ids)
        {
            _logger.LogInformation(
                $"Requested delete of multiple {RepoEntityName} with ids {JsonConvert.SerializeObject(ids)}");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.delete").StartActive();
            try
            {
                var deleteSpan = _tracer.BuildSpan("deletion").WithTag(Tags.DbType, "Mongo").AsChildOf(mainSpan.Span)
                    .Start();
                await Items.DeleteManyAsync(item => ids.Contains(item.Id));
                deleteSpan.Finish();
                await _producerService.SendEvent();
                // TODO: On deletion no need fully recache - just remove hash entry!
                await _producerService.SendEvent(eventType: EventType.Deleted, payload: ids);
                _logger.LogInformation($"Requested delete of multiple {RepoEntityName} succeeded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error during attempt to delete multiple {RepoEntityName}");
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
        /// <returns>Entries of entity</returns>
        private async Task<IEnumerable<TRepoEntity>> Get(bool hashMode = true)
        {
            _logger.LogInformation($"Requested all {RepoEntityName} items");
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.get-all").StartActive();
            try
            {
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
                var dbResult = (await Items.FindAsync(item => true)).ToEnumerable();
                dbSpan.Finish();
                await _producerService.SendEvent();

                var repoEntities = dbResult as TRepoEntity[] ?? dbResult.ToArray();
                _logger.LogInformation($"Request of all {RepoEntityName} items returned {repoEntities.Length} items");
                return repoEntities;
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
        /// Get item from cache
        /// </summary>
        /// <param name="id">Identifier of entry</param>
        /// <param name="hashMode">Get from HashSet</param>
        /// <returns>Entry of entity</returns>
        private async Task<TRepoEntity> Get(string id, bool hashMode = true)
        {
            var mainSpan = _tracer.BuildSpan("mongo-cached-repo.get").StartActive();
            _logger.LogInformation($"Requested {RepoEntityName} with id: {id}");
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
                var dbResult = await (await Items.FindAsync(item => item.Id == id)).FirstOrDefaultAsync();
                dbSpan.Finish();
                await _producerService.SendEvent();
                _logger.LogInformation($"Request of {RepoEntityName} with id: {id} succeeded");
                return dbResult;
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