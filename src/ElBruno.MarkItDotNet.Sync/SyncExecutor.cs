// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Orchestrates the synchronization workflow: computing plans, committing state, and marking deletions.
/// </summary>
public class SyncExecutor
{
    private readonly ISyncStateStore _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncExecutor"/> class.
    /// </summary>
    /// <param name="store">The sync state store for persisting state.</param>
    public SyncExecutor(ISyncStateStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    /// <summary>
    /// Computes hashes for the source content and chunks, then produces a sync plan.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <param name="sourceContent">The source content stream.</param>
    /// <param name="chunks">The chunked results of the document.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="SyncPlan"/> describing the actions to take.</returns>
    public async Task<SyncPlan> PlanAsync(
        string documentId,
        Stream sourceContent,
        IReadOnlyList<ChunkResult> chunks,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(sourceContent);
        ArgumentNullException.ThrowIfNull(chunks);

        var sourceHash = ContentHasher.ComputeSourceHash(sourceContent);
        var chunkHashes = ContentHasher.ComputeChunkHashes(chunks);
        var previousState = await _store.GetStateAsync(documentId, ct).ConfigureAwait(false);

        return SyncPlanner.ComputePlan(documentId, sourceHash, chunkHashes, previousState);
    }

    /// <summary>
    /// Commits the sync plan by saving the new state after successful indexing.
    /// </summary>
    /// <param name="plan">The sync plan that was executed.</param>
    /// <param name="chunkHashes">The chunk hashes for the current version.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task CommitAsync(
        SyncPlan plan,
        IReadOnlyDictionary<string, string> chunkHashes,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(chunkHashes);

        var previousState = await _store.GetStateAsync(plan.DocumentId, ct).ConfigureAwait(false);

        // Build the new source hash from the existing state or compute a placeholder
        var sourceHash = previousState?.SourceHash ?? string.Empty;

        var newState = new SyncState
        {
            DocumentId = plan.DocumentId,
            SourceHash = sourceHash,
            ChunkHashes = new Dictionary<string, string>(chunkHashes),
            Version = plan.NewVersion,
            LastSyncedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            Metadata = previousState?.Metadata ?? new Dictionary<string, string>()
        };

        await _store.SaveStateAsync(newState, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Marks a document as soft-deleted, producing a Delete sync plan and updating state.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="SyncPlan"/> with a Delete action.</returns>
    public async Task<SyncPlan> MarkDeletedAsync(string documentId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(documentId);

        var previousState = await _store.GetStateAsync(documentId, ct).ConfigureAwait(false);

        var plan = new SyncPlan
        {
            DocumentId = documentId,
            Action = SyncAction.Delete,
            ChunksToAdd = [],
            ChunksToUpdate = [],
            ChunksToDelete = previousState?.ChunkHashes.Keys.ToList() ?? [],
            ChunksUnchanged = [],
            PreviousVersion = previousState?.Version,
            NewVersion = (previousState?.Version ?? 0) + 1
        };

        var deletedState = new SyncState
        {
            DocumentId = documentId,
            SourceHash = previousState?.SourceHash ?? string.Empty,
            ChunkHashes = new Dictionary<string, string>(),
            Version = plan.NewVersion,
            LastSyncedAt = DateTimeOffset.UtcNow,
            IsDeleted = true,
            Metadata = previousState?.Metadata ?? new Dictionary<string, string>()
        };

        await _store.SaveStateAsync(deletedState, ct).ConfigureAwait(false);

        return plan;
    }
}
