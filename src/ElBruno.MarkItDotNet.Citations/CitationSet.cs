namespace ElBruno.MarkItDotNet.Citations;

/// <summary>
/// A collection of citations associated with a single chunk or answer.
/// </summary>
public record CitationSet
{
    /// <summary>Gets the identifier of the chunk or answer these citations belong to.</summary>
    public string ChunkId { get; init; } = string.Empty;

    /// <summary>Gets the list of citation references for this chunk.</summary>
    public IReadOnlyList<CitationReference> Citations { get; init; } = [];
}
