using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Citations;

/// <summary>
/// Propagates citation references from source documents through to chunks,
/// creating <see cref="CitationSet"/> instances that link each chunk back to its source.
/// </summary>
public static class CitationPropagator
{
    /// <summary>
    /// Matches chunks to their source blocks in the document and creates citation sets.
    /// </summary>
    /// <param name="document">The source document containing blocks with source references.</param>
    /// <param name="chunks">The chunks to create citations for.</param>
    /// <returns>A list of citation sets, one per chunk.</returns>
    public static IReadOnlyList<CitationSet> PropagateToChunks(Document document, IReadOnlyList<ChunkInfo> chunks)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(chunks);

        var filePath = document.Source?.FilePath;
        var results = new List<CitationSet>(chunks.Count);

        foreach (var chunk in chunks)
        {
            var citations = new List<CitationReference>();

            if (chunk.Sources.Count > 0)
            {
                foreach (var source in chunk.Sources)
                {
                    citations.Add(new CitationReference
                    {
                        FilePath = source.FilePath ?? filePath,
                        PageNumber = source.PageNumber,
                        HeadingPath = source.HeadingPath ?? chunk.HeadingPath,
                        Span = source.Span,
                        Mode = CitationMode.Exact
                    });
                }
            }
            else
            {
                // No source references on the chunk — create a coarse citation
                citations.Add(new CitationReference
                {
                    FilePath = filePath,
                    HeadingPath = chunk.HeadingPath,
                    Mode = CitationMode.Coarse
                });
            }

            results.Add(new CitationSet
            {
                ChunkId = chunk.ChunkId,
                Citations = citations
            });
        }

        return results;
    }
}
