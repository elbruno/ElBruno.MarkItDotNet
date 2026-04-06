// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.Metadata;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// Tests the Metadata extraction → Document enrichment → Chunking → Chunk enrichment pipeline.
/// </summary>
public class MetadataEnrichmentPipelineTests
{
    [Fact]
    public void ExtractMetadata_ThenAttachToDocument_EnrichesDocument()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var extractor = new DocumentMetadataExtractor();

        var metadata = extractor.Extract(document);

        metadata.Title.Should().Be("Annual Performance Report");
        metadata.Author.Should().Be("Jane Smith");
        metadata.DocumentType.Should().Be(DocumentType.Report);
        metadata.WordCount.Should().BeGreaterThan(0);
        metadata.NormalizedHeadings.Should().NotBeEmpty();

        var enriched = MetadataAttacher.AttachToDocument(document, metadata);

        enriched.Metadata.Title.Should().Be("Annual Performance Report");
        enriched.Metadata.Author.Should().Be("Jane Smith");
        enriched.Metadata.WordCount.Should().Be(metadata.WordCount);
        enriched.Metadata.Custom.Should().ContainKey("Language");
        enriched.Metadata.Custom.Should().ContainKey("DocumentType");
        enriched.Metadata.Custom["DocumentType"].Should().Be("Report");
    }

    [Fact]
    public void EnrichedDocument_ChunkedAndMetadataAttachedToChunks()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var extractor = new DocumentMetadataExtractor();
        var metadata = extractor.Extract(document);
        var enriched = MetadataAttacher.AttachToDocument(document, metadata);

        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(enriched);

        chunks.Should().NotBeEmpty();

        var enrichedChunks = MetadataAttacher.AttachToChunks(chunks, metadata);

        enrichedChunks.Should().HaveSameCount(chunks);
        enrichedChunks.Should().AllSatisfy(c =>
        {
            c.Metadata.Should().ContainKey("DocumentTitle");
            c.Metadata["DocumentTitle"].Should().Be("Annual Performance Report");
            c.Metadata.Should().ContainKey("DocumentAuthor");
            c.Metadata["DocumentAuthor"].Should().Be("Jane Smith");
            c.Metadata.Should().ContainKey("DocumentLanguage");
            c.Metadata.Should().ContainKey("DocumentType");
            c.Metadata["DocumentType"].Should().Be("Report");
        });
    }

    [Fact]
    public void ChunksRetainOriginalContentAfterMetadataEnrichment()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var extractor = new DocumentMetadataExtractor();
        var metadata = extractor.Extract(document);

        var chunker = new HeadingBasedChunker();
        var originalChunks = chunker.Chunk(document);
        var enrichedChunks = MetadataAttacher.AttachToChunks(originalChunks, metadata);

        for (int i = 0; i < originalChunks.Count; i++)
        {
            enrichedChunks[i].Id.Should().Be(originalChunks[i].Id);
            enrichedChunks[i].Content.Should().Be(originalChunks[i].Content);
            enrichedChunks[i].Index.Should().Be(originalChunks[i].Index);
            enrichedChunks[i].HeadingPath.Should().Be(originalChunks[i].HeadingPath);
            enrichedChunks[i].Sources.Should().BeEquivalentTo(originalChunks[i].Sources);
        }
    }

    [Fact]
    public void SimpleDocument_MetadataExtraction_InfersDocumentType()
    {
        var document = TestDocumentFactory.CreateSimpleDocument();
        var extractor = new DocumentMetadataExtractor();

        var metadata = extractor.Extract(document);

        metadata.Title.Should().Be("Quick Note");
        metadata.Author.Should().Be("Test Author");
        metadata.SectionCount.Should().Be(1);
        metadata.DocumentType.Should().Be(DocumentType.Article);
    }

    [Fact]
    public void MetadataExtraction_NormalizedHeadingsMatchDocument()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var extractor = new DocumentMetadataExtractor();

        var metadata = extractor.Extract(document);

        metadata.NormalizedHeadings.Should().NotBeEmpty();
        metadata.NormalizedHeadings.Should().AllSatisfy(h =>
        {
            h.Id.Should().StartWith("heading-");
            h.Text.Should().NotBeNullOrWhiteSpace();
            h.Level.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(6);
            h.OriginalText.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public void MetadataAttacher_WithDifferentChunkers_AllReceiveMetadata()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var extractor = new DocumentMetadataExtractor();
        var metadata = extractor.Extract(document);

        IChunkingStrategy[] strategies =
        [
            new HeadingBasedChunker(),
            new ParagraphBasedChunker(),
            new TokenAwareChunker(),
        ];

        foreach (var strategy in strategies)
        {
            var chunks = strategy.Chunk(document);
            var enriched = MetadataAttacher.AttachToChunks(chunks, metadata);

            enriched.Should().HaveSameCount(chunks);
            enriched.Should().AllSatisfy(c =>
            {
                c.Metadata.Should().ContainKey("DocumentTitle",
                    $"strategy '{strategy.Name}' should attach document title");
            });
        }
    }
}
