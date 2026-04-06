using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Citations;

/// <summary>
/// Lightweight representation of a chunk for citation propagation.
/// Avoids a dependency on the Chunking package.
/// </summary>
public record ChunkInfo
{
    /// <summary>Gets the unique identifier of the chunk.</summary>
    public string ChunkId { get; init; } = string.Empty;

    /// <summary>Gets the text content of the chunk.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Gets the source references associated with the chunk's blocks.</summary>
    public IReadOnlyList<SourceReference> Sources { get; init; } = [];

    /// <summary>Gets the heading path for the chunk's location in the document hierarchy.</summary>
    public string? HeadingPath { get; init; }
}
