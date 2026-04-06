// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Describes the synchronization plan for a single document, including which chunks
/// to add, update, delete, or leave unchanged.
/// </summary>
public record SyncPlan
{
    /// <summary>
    /// Gets the unique identifier of the document.
    /// </summary>
    public string DocumentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the overall sync action for this document.
    /// </summary>
    public SyncAction Action { get; init; }

    /// <summary>
    /// Gets the chunk IDs that should be added (new chunks).
    /// </summary>
    public IReadOnlyList<string> ChunksToAdd { get; init; } = [];

    /// <summary>
    /// Gets the chunk IDs that have changed and should be updated.
    /// </summary>
    public IReadOnlyList<string> ChunksToUpdate { get; init; } = [];

    /// <summary>
    /// Gets the chunk IDs that should be removed.
    /// </summary>
    public IReadOnlyList<string> ChunksToDelete { get; init; } = [];

    /// <summary>
    /// Gets the chunk IDs that are unchanged.
    /// </summary>
    public IReadOnlyList<string> ChunksUnchanged { get; init; } = [];

    /// <summary>
    /// Gets the previous version number, or null if this is a new document.
    /// </summary>
    public int? PreviousVersion { get; init; }

    /// <summary>
    /// Gets the new version number after this sync.
    /// </summary>
    public int NewVersion { get; init; }
}
