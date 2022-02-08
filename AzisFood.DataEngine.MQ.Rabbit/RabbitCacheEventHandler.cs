using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.MQ.Abstractions.Interfaces;
using AzisFood.MQ.Abstractions.Models;

namespace AzisFood.DataEngine.MQ.Rabbit;

/// <summary>
///     Cache event handler for AzisFood.MQ.Rabbit
/// </summary>
public class RabbitCacheEventHandler<TRepoEntity> : ICacheEventHandler<TRepoEntity>
    where TRepoEntity : class, IRepoEntity, new()
{
    private readonly IProducerService<TRepoEntity> _producerService;

    public RabbitCacheEventHandler(IProducerService<TRepoEntity> producerService)
    {
        _producerService = producerService;
    }

    /// <inheritdoc />
    public async Task NotifyCreate(TRepoEntity payload, CancellationToken token = default)
    {
        // For now ignore payload...
        await _producerService.SendEvent(token: token);
    }

    /// <inheritdoc />
    public async Task NotifyUpdate(Guid id, TRepoEntity payload, CancellationToken token = default)
    {
        // For now ignore payload...
        await _producerService.SendEvent(token: token);
    }

    /// <inheritdoc />
    public async Task NotifyRemove(CancellationToken token = default)
    {
        await _producerService.SendEvent(eventType: EventType.Deleted, token: token);
    }

    /// <inheritdoc />
    public async Task NotifyRemove(Guid payload, CancellationToken token = default)
    {
        await _producerService.SendEvent(eventType: EventType.Deleted, payload: payload, token: token);
    }

    /// <inheritdoc />
    public async Task NotifyRemove(Guid[] payload, CancellationToken token = default)
    {
        await _producerService.SendEvent(eventType: EventType.Deleted, payload: payload, token: token);
    }

    /// <inheritdoc />
    public async Task NotifyMissing(CancellationToken token = default)
    {
        await _producerService.SendEvent(token: token);
    }

    /// <inheritdoc />
    public async Task NotifyMissing(Guid payload, CancellationToken token = default)
    {
        // For now ignore payload...
        await _producerService.SendEvent(token: token);
    }
}