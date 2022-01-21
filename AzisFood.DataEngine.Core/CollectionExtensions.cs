using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Core;

/// <summary>
///     Custom collection extensions
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    ///     Process collection by fixed chunks
    /// </summary>
    /// <param name="input">Input collection</param>
    /// <param name="chunkSize">Chunk size</param>
    /// <param name="process">Action to process chunks</param>
    public static void ChunkedProcess<T>(this IEnumerable<T> input, int chunkSize,
        Action<IEnumerable<T>> process)
    {
        var collection = input as T[] ?? input.ToArray();
        var chunkCount = collection.Length / chunkSize;
        for (var i = 0; i < chunkCount + 1; i++)
        {
            var rng = collection.Skip(i * chunkSize).Take(chunkSize);
            process(rng);
        }
    }

    /// <summary>
    ///     Process collection by fixed chunks async
    /// </summary>
    /// <param name="input">Input collection</param>
    /// <param name="chunkSize">Chunk size</param>
    /// <param name="process">Action to process chunks</param>
    public static async Task ChunkedProcessAsync<T>(this IEnumerable<T> input, int chunkSize,
        Func<IEnumerable<T>, Task> process)
    {
        var collection = input as T[] ?? input.ToArray();
        var chunkCount = collection.Length / chunkSize;
        for (var i = 0; i < chunkCount + 1; i++)
        {
            var rng = collection.Skip(i * chunkSize).Take(chunkSize);
            await process(rng);
        }
    }
}