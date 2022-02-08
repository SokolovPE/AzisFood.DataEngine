using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Abstractions.Interfaces;

/// <summary>
///     Contract of service which handles database updates in cached repository
/// </summary>
public interface ICacheEventHandler<in TRepoEntity> where TRepoEntity : class, IRepoEntity, new()
{
    /// <summary>
    ///     Create notification
    /// </summary>
    /// <param name="payload">Created entity</param>
    /// <param name="token">Cancellation token</param>
    Task NotifyCreate(TRepoEntity payload, CancellationToken token = default);

    /// <summary>
    ///     Update notification
    /// </summary>
    /// <param name="id">Identifier of updated entity</param>
    /// <param name="payload">New value</param>
    /// <param name="token">Cancellation token</param>
    Task NotifyUpdate(Guid id, TRepoEntity payload, CancellationToken token = default);

    /// <summary>
    ///     Remove notification after filtered delete
    ///     Removed ids unknown
    /// </summary>
    /// <param name="token">Cancellation token</param>
    Task NotifyRemove(CancellationToken token = default);

    /// <summary>
    ///     Remove of single record notification
    /// </summary>
    /// <param name="payload">Identifier of deleted entity</param>
    /// <param name="token">Cancellation token</param>
    Task NotifyRemove(Guid payload, CancellationToken token = default);

    /// <summary>
    ///     Remove of multiple record notification
    /// </summary>
    /// <param name="payload">Identifier of deleted entity</param>
    /// <param name="token">Cancellation token</param>
    Task NotifyRemove(Guid[] payload, CancellationToken token = default);

    /// <summary>
    ///     Entity missing in cache Notification
    /// </summary>
    /// <param name="token">Cancellation token</param>
    Task NotifyMissing(CancellationToken token = default);

    /// <summary>
    ///     Entity missing in cache Notification
    /// </summary>
    /// <param name="payload">Missing record id</param>
    /// <param name="token">Cancellation token</param>
    Task NotifyMissing(Guid payload, CancellationToken token = default);
}