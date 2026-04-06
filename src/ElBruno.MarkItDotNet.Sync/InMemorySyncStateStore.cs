// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// An in-memory implementation of <see cref="ISyncStateStore"/> using a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// Suitable for testing and transient scenarios.
/// </summary>
public class InMemorySyncStateStore : ISyncStateStore
{
    private readonly ConcurrentDictionary<string, SyncState> _states = new();

    /// <inheritdoc />
    public Task<SyncState?> GetStateAsync(string documentId, CancellationToken ct = default)
    {
        _states.TryGetValue(documentId, out var state);
        return Task.FromResult(state);
    }

    /// <inheritdoc />
    public Task SaveStateAsync(SyncState state, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        _states[state.DocumentId] = state;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteStateAsync(string documentId, CancellationToken ct = default)
    {
        _states.TryRemove(documentId, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SyncState>> GetAllStatesAsync(CancellationToken ct = default)
    {
        IReadOnlyList<SyncState> result = _states.Values.ToList();
        return Task.FromResult(result);
    }
}
