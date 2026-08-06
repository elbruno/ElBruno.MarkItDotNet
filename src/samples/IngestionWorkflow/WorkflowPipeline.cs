using ElBruno.MarkItDotNet.AzureSearch;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;

namespace IngestionWorkflowSample;

public static class WorkflowPipeline
{
    public static Document BuildDocument(string documentId, string title, string markdown, string sourcePath)
    {
        return new Document
        {
            Id = documentId,
            Metadata = new DocumentMetadata
            {
                Title = title,
                SourceFormat = ".md",
                WordCount = markdown.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                Custom = new Dictionary<string, object>
                {
                    ["sourcePath"] = sourcePath
                }
            },
            Source = new SourceReference { FilePath = sourcePath },
            Sections =
            [
                new DocumentSection
                {
                    Id = $"{documentId}-section-1",
                    Heading = new HeadingBlock
                    {
                        Id = $"{documentId}-heading-1",
                        Text = title,
                        Level = 1
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = $"{documentId}-block-1",
                            Text = markdown,
                            Source = new SourceReference { FilePath = sourcePath, HeadingPath = title }
                        }
                    ]
                }
            ]
        };
    }

    public static IChunkingStrategy ResolveChunker(string strategy)
    {
        return strategy switch
        {
            "heading" => new HeadingBasedChunker(),
            "token" => new TokenAwareChunker(),
            _ => new ParagraphBasedChunker(),
        };
    }

    public static IReadOnlyList<ChunkInfo> ToChunkInfos(IReadOnlyList<ChunkResult> chunks)
    {
        return chunks
            .Select(chunk => new ChunkInfo
            {
                ChunkId = chunk.Id,
                Content = chunk.Content,
                HeadingPath = chunk.HeadingPath,
                Sources = chunk.Sources
            })
            .ToList();
    }

    public static IReadOnlyList<SearchDocument> MapSearchDocuments(
        IReadOnlyList<ChunkResult> chunks,
        IReadOnlyList<CitationSet> citationSets,
        Document document,
        ISearchDocumentMapper mapper)
    {
        var citationByChunkId = citationSets.ToDictionary(item => item.ChunkId, StringComparer.OrdinalIgnoreCase);
        var searchDocuments = new List<SearchDocument>(chunks.Count);

        foreach (var chunk in chunks)
        {
            citationByChunkId.TryGetValue(chunk.Id, out var citationSet);
            var primaryCitation = citationSet?.Citations.FirstOrDefault();
            searchDocuments.Add(mapper.Map(chunk, document, primaryCitation));
        }

        return searchDocuments;
    }
}
