using System;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Abstractions.Interfaces;

public interface ICacheOperator<T>
{
    /// <summary>
    ///     Fully refresh cache of entity
    /// </summary>
    /// <param name="expiry">Key expiration time</param>
    /// <param name="asHash">Save as hashset</param>
    Task FullRecache(TimeSpan expiry, bool asHash = true);
}