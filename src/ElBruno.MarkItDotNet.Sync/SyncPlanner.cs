// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Computes synchronization plans by comparing current content hashes against previously stored state.
/// </summary>
public static class SyncPlanner
{
    /// <summary>
    /// Computes a sync plan by comparing the current source hash and chunk hashes against a previous state.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <param name="sourceHash">The current SHA-256 hash of the source content.</param>
    /// <param name="currentChunkHashes">The current mapping of chunk ID to chunk content hash.</param>
    /// <param name="previousState">The previously stored sync state, or null if this is a new document.</param>
    /// <returns>A <see cref="SyncPlan"/> describing the actions to take.</returns>
    public static SyncPlan ComputePlan(
        string documentId,
        string sourceHash,
        IReadOnlyDictionary<string, string> currentChunkHashes,
        SyncState? previousState)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(sourceHash);
        ArgumentNullException.ThrowIfNull(currentChunkHashes);

        // New document — add all chunks
        if (previousState is null)
        {
            return new SyncPlan
            {
                DocumentId = documentId,
                Action = SyncAction.Add,
                ChunksToAdd = currentChunkHashes.Keys.ToList(),
                ChunksToUpdate = [],
                ChunksToDelete = [],
                ChunksUnchanged = [],
                PreviousVersion = null,
                NewVersion = 1
            };
        }

        // Source hash unchanged — skip
        if (previousState.SourceHash == sourceHash)
        {
            return new SyncPlan
            {
                DocumentId = documentId,
                Action = SyncAction.Skip,
                ChunksToAdd = [],
                ChunksToUpdate = [],
                ChunksToDelete = [],
                ChunksUnchanged = currentChunkHashes.Keys.ToList(),
                PreviousVersion = previousState.Version,
                NewVersion = previousState.Version
            };
        }

        // Source hash changed — diff chunk hashes
        var chunksToAdd = new List<string>();
        var chunksToUpdate = new List<string>();
        var chunksUnchanged = new List<string>();

        foreach (var (chunkId, hash) in currentChunkHashes)
        {
            if (!previousState.ChunkHashes.TryGetValue(chunkId, out var previousHash))
            {
                chunksToAdd.Add(chunkId);
            }
            else if (previousHash != hash)
            {
                chunksToUpdate.Add(chunkId);
            }
            else
            {
                chunksUnchanged.Add(chunkId);
            }
        }

        var chunksToDelete = previousState.ChunkHashes.Keys
            .Where(id => !currentChunkHashes.ContainsKey(id))
            .ToList();

        return new SyncPlan
        {
            DocumentId = documentId,
            Action = SyncAction.Update,
            ChunksToAdd = chunksToAdd,
            ChunksToUpdate = chunksToUpdate,
            ChunksToDelete = chunksToDelete,
            ChunksUnchanged = chunksUnchanged,
            PreviousVersion = previousState.Version,
            NewVersion = previousState.Version + 1
        };
    }
}
