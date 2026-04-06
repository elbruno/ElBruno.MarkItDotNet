// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Abstraction for persisting document synchronization state.
/// </summary>
public interface ISyncStateStore
{
    /// <summary>
    /// Retrieves the sync state for a given document, or null if no state exists.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The sync state, or null if not found.</returns>
    Task<SyncState?> GetStateAsync(string documentId, CancellationToken ct = default);

    /// <summary>
    /// Saves or updates the sync state for a document.
    /// </summary>
    /// <param name="state">The sync state to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveStateAsync(SyncState state, CancellationToken ct = default);

    /// <summary>
    /// Deletes the sync state for a document.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteStateAsync(string documentId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all stored sync states.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of all sync states.</returns>
    Task<IReadOnlyList<SyncState>> GetAllStatesAsync(CancellationToken ct = default);
}
