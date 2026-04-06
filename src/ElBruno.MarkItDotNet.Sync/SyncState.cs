// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Represents the persisted synchronization state for a single document,
/// including its source hash, chunk hashes, version, and metadata.
/// </summary>
public record SyncState
{
    /// <summary>
    /// Gets the unique identifier of the document.
    /// </summary>
    public string DocumentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SHA-256 hash of the source file content.
    /// </summary>
    public string SourceHash { get; init; } = string.Empty;

    /// <summary>
    /// Gets the mapping of chunk ID to chunk content hash.
    /// </summary>
    public IReadOnlyDictionary<string, string> ChunkHashes { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the version number, incremented on each sync.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Gets the timestamp of the last successful sync.
    /// </summary>
    public DateTimeOffset LastSyncedAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether the document has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets additional metadata associated with this sync state (e.g., source file path, format).
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
