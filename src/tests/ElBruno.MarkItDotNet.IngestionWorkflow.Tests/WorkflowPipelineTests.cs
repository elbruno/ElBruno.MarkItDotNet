using ElBruno.MarkItDotNet.AzureSearch;
using ElBruno.MarkItDotNet.Chunking;
using FluentAssertions;
using IngestionWorkflowSample;
using Xunit;

namespace ElBruno.MarkItDotNet.IngestionWorkflow.Tests;

public class WorkflowPipelineTests
{
    [Fact]
    public void BuildDocument_ThenChunkAndMap_ProducesSearchDocuments()
    {
        var document = WorkflowPipeline.BuildDocument(
            documentId: "doc-1",
            title: "Demo",
            markdown: "Line 1\nLine 2\nLine 3",
            sourcePath: "sample.txt");

        var chunker = WorkflowPipeline.ResolveChunker("paragraph");
        var chunks = chunker.Chunk(
            document,
            new ChunkingOptions { MaxChunkSize = 20, OverlapSize = 0 });

        chunks.Should().NotBeEmpty();

        var chunkInfos = WorkflowPipeline.ToChunkInfos(chunks);
        var citationSets = ElBruno.MarkItDotNet.Citations.CitationPropagator.PropagateToChunks(document, chunkInfos);
        var mapper = new DefaultSearchDocumentMapper();

        var searchDocuments = WorkflowPipeline.MapSearchDocuments(chunks, citationSets, document, mapper);

        searchDocuments.Should().HaveCount(chunks.Count);
        searchDocuments[0].Id.Should().Be(chunks[0].Id);
        searchDocuments[0].DocumentId.Should().Be("doc-1");
        searchDocuments[0].CitationText.Should().NotBeNullOrWhiteSpace();
    }
}
