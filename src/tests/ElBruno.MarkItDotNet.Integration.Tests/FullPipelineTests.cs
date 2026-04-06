// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.Metadata;
using ElBruno.MarkItDotNet.Quality;
using ElBruno.MarkItDotNet.VectorData;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// End-to-end tests that run the full ingestion pipeline:
/// Document → Quality → Metadata → Chunking → Citations → VectorData → JSONL.
/// </summary>
public class FullPipelineTests
{
    [Fact]
    public void FullPipeline_ProducesValidJsonlRecords()
    {
        // Arrange
        var document = TestDocumentFactory.CreateRealisticReport();

        // Step 1: Quality check
        var qualityAnalyzer = new DocumentQualityAnalyzer();
        var qualityReport = qualityAnalyzer.Analyze(document);

        qualityReport.OverallScore.Should().BeGreaterThan(0.5, "a realistic report should have acceptable quality");
        qualityReport.SuggestedAction.Should().BeOneOf(QualityAction.None, QualityAction.Review);

        // Step 2: Metadata extraction
        var metadataExtractor = new DocumentMetadataExtractor();
        var metadataResult = metadataExtractor.Extract(document);

        metadataResult.Title.Should().Be("Annual Performance Report");
        metadataResult.Author.Should().Be("Jane Smith");
        metadataResult.WordCount.Should().BeGreaterThan(0);
        metadataResult.SectionCount.Should().Be(3);

        // Step 3: Enrich document with metadata
        var enrichedDocument = MetadataAttacher.AttachToDocument(document, metadataResult);
        enrichedDocument.Metadata.Title.Should().Be("Annual Performance Report");

        // Step 4: Chunk using heading-based strategy
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(enrichedDocument);

        chunks.Should().HaveCountGreaterThanOrEqualTo(3, "the report has 3 top-level sections plus a subsection");
        chunks.Should().AllSatisfy(c =>
        {
            c.Id.Should().NotBeNullOrEmpty();
            c.Content.Should().NotBeNullOrWhiteSpace();
        });

        // Step 5: Propagate citations
        var chunkInfos = chunks.Select(c => new ChunkInfo
        {
            ChunkId = c.Id,
            Content = c.Content,
            Sources = c.Sources,
            HeadingPath = c.HeadingPath,
        }).ToList();

        var citationSets = CitationPropagator.PropagateToChunks(enrichedDocument, chunkInfos);
        citationSets.Should().HaveSameCount(chunks);
        citationSets.Should().AllSatisfy(cs =>
        {
            cs.ChunkId.Should().NotBeNullOrEmpty();
            cs.Citations.Should().NotBeEmpty();
        });

        // Step 6: Map to VectorRecords
        var mapper = new DefaultVectorRecordMapper();
        var vectorRecords = chunks.Select(c => mapper.MapChunk(c, enrichedDocument)).ToList();

        vectorRecords.Should().HaveSameCount(chunks);
        vectorRecords.Should().AllSatisfy(vr =>
        {
            vr.Id.Should().NotBeNullOrEmpty();
            vr.Content.Should().NotBeNullOrWhiteSpace();
            vr.DocumentId.Should().Be(enrichedDocument.Id);
            vr.DocumentTitle.Should().Be("Annual Performance Report");
        });

        // Step 7: Export to JSONL
        var jsonl = JsonlExporter.ExportToString(vectorRecords);

        jsonl.Should().NotBeNullOrWhiteSpace();
        var jsonLines = jsonl.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        jsonLines.Should().HaveSameCount(vectorRecords);

        // Verify each line is valid JSON with expected fields
        foreach (var line in jsonLines)
        {
            var parsed = JsonDocument.Parse(line);
            parsed.RootElement.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
            parsed.RootElement.GetProperty("content").GetString().Should().NotBeNullOrWhiteSpace();
            parsed.RootElement.GetProperty("documentId").GetString().Should().Be(enrichedDocument.Id);
            parsed.RootElement.GetProperty("documentTitle").GetString().Should().Be("Annual Performance Report");
        }
    }

    [Fact]
    public void FullPipeline_ChunksRetainSourceReferences()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        // Chunks from sections with source references should carry them through
        var chunksWithSources = chunks.Where(c => c.Sources.Count > 0).ToList();
        chunksWithSources.Should().NotBeEmpty("the report has blocks with source references");

        foreach (var chunk in chunksWithSources)
        {
            chunk.Sources.Should().AllSatisfy(s =>
            {
                s.FilePath.Should().Be("reports/annual-report.pdf");
                s.PageNumber.Should().BeGreaterThan(0);
            });
        }
    }

    [Fact]
    public void FullPipeline_HeadingPathsArePreserved()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var headingPaths = chunks.Select(c => c.HeadingPath).Where(hp => hp is not null).ToList();
        headingPaths.Should().NotBeEmpty();

        // The subsection should produce a composite heading path
        headingPaths.Should().Contain(hp => hp!.Contains(">"), "subsections produce hierarchical heading paths");
    }

    [Fact]
    public void FullPipeline_VectorRecordsHavePageNumbers()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var mapper = new DefaultVectorRecordMapper();
        var records = chunks.Select(c => mapper.MapChunk(c, document)).ToList();

        var recordsWithPages = records.Where(r => r.PageNumber.HasValue).ToList();
        recordsWithPages.Should().NotBeEmpty("chunks with source references should have page numbers");
    }
}
