using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzisFood.DataEngine.Core.Implementations
{
    public class BaseRepository<TRepoEntity> : IBaseRepository<TRepoEntity> where TRepoEntity: IRepoEntity, new()
    {
        public string RepoEntityName { get; init; }
        private readonly ILogger<BaseRepository<TRepoEntity>> _logger;
        protected readonly IDataAccess<TRepoEntity> DataAccess;

        public BaseRepository(ILogger<BaseRepository<TRepoEntity>> logger, IDataAccess<TRepoEntity> dataAccess)
        {
            _logger = logger;
            DataAccess = dataAccess;

            // Fill constants
            RepoEntityName = typeof(TRepoEntity).Name;
        }

        public virtual async Task<IEnumerable<TRepoEntity>> GetAsync(CancellationToken token = default)
        {
            _logger.LogInformation($"Requested all {RepoEntityName} items");
            try
            {
                token.ThrowIfCancellationRequested();
                var repoEntities = await DataAccess.GetAllAsync(token);
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

        public virtual async Task<TRepoEntity> GetAsync(Guid id, CancellationToken token = default)
        {
            _logger.LogInformation($"Requested {RepoEntityName} with id: {id}");
            try
            {
                token.ThrowIfCancellationRequested();
                var repoEntity = await DataAccess.GetAsync(id, token);
                if (repoEntity == null)
                {
                    throw new InvalidOperationException($"{RepoEntityName} with id {id} was not found");
                }
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
                
                var repoEntities = await DataAccess.GetAsync(filter, token);
                _logger.LogInformation(
                    $"Request of filtered {RepoEntityName} items succeeded, filter: {filter}");
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
                await DataAccess.CreateAsync(item, token);
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

        public virtual async Task UpdateAsync(Guid id, TRepoEntity itemIn, CancellationToken token = default)
        {
            _logger.LogInformation(
                $"Requested update of {RepoEntityName} with id {id} with new value: {JsonConvert.SerializeObject(itemIn)}");
            try
            {
                await DataAccess.UpdateAsync(id, itemIn, token);
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
                await DataAccess.RemoveAsync(itemIn, token);
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

        public virtual async Task RemoveAsync(Guid id, CancellationToken token = default)
        {
            _logger.LogInformation($"Requested delete of {RepoEntityName} with id {id}");
            try
            {
                await DataAccess.RemoveAsync(id, token);
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
                await DataAccess.RemoveAsync(filter, token);
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

        public virtual async Task RemoveManyAsync(Guid[] ids, CancellationToken token = default)
        {
            _logger.LogInformation(
                $"Requested delete of multiple {RepoEntityName} with ids {JsonConvert.SerializeObject(ids)}");
            try
            {
                await DataAccess.RemoveManyAsync(ids, token);
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