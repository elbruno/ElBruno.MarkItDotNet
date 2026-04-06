// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;

namespace ElBruno.MarkItDotNet.VectorData;

/// <summary>
/// Helper for batching vector records for upload to vector stores.
/// </summary>
public static class VectorBatchProcessor
{
    /// <summary>
    /// Batches a sequence of vector records into groups of the specified size.
    /// </summary>
    /// <param name="records">The records to batch.</param>
    /// <param name="batchSize">The maximum number of records per batch. Defaults to 100.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An async enumerable of record batches.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="batchSize"/> is less than 1.</exception>
    public static async IAsyncEnumerable<IReadOnlyList<VectorRecord>> BatchAsync(
        IEnumerable<VectorRecord> records,
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        var batch = new List<VectorRecord>(batchSize);

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            batch.Add(record);

            if (batch.Count >= batchSize)
            {
                yield return batch.AsReadOnly();
                batch = new List<VectorRecord>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch.AsReadOnly();
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
