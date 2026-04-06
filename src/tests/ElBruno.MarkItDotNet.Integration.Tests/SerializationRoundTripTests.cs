// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.VectorData;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// Tests cross-package serialization round trips for documents, citations, and vector records.
/// </summary>
public class SerializationRoundTripTests
{
    [Fact]
    public void Document_SerializeDeserialize_PreservesStructure()
    {
        var document = TestDocumentFactory.CreateRealisticReport();

        var json = DocumentSerializer.Serialize(document);
        json.Should().NotBeNullOrWhiteSpace();

        var deserialized = DocumentSerializer.Deserialize(json);
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(document.Id);
        deserialized.Metadata.Title.Should().Be(document.Metadata.Title);
        deserialized.Metadata.Author.Should().Be(document.Metadata.Author);
        deserialized.Sections.Should().HaveCount(document.Sections.Count);
    }

    [Fact]
    public void Document_SerializeDeserialize_PreservesBlocks()
    {
        var document = TestDocumentFactory.CreateRealisticReport();

        var json = DocumentSerializer.Serialize(document);
        var deserialized = DocumentSerializer.Deserialize(json)!;

        // Verify block types are preserved through polymorphic serialization
        var originalBlocks = FlattenBlocks(document);
        var deserializedBlocks = FlattenBlocks(deserialized);

        deserializedBlocks.Should().HaveSameCount(originalBlocks);

        for (int i = 0; i < originalBlocks.Count; i++)
        {
            deserializedBlocks[i].GetType().Should().Be(originalBlocks[i].GetType(),
                $"block at index {i} should preserve its type");
        }
    }

    [Fact]
    public void Document_SerializeDeserialize_PreservesSourceReferences()
    {
        var document = TestDocumentFactory.CreateRealisticReport();

        var json = DocumentSerializer.Serialize(document);
        var deserialized = DocumentSerializer.Deserialize(json)!;

        deserialized.Source.Should().NotBeNull();
        deserialized.Source!.FilePath.Should().Be(document.Source!.FilePath);

        // Check block-level source references
        var originalBlocksWithSources = FlattenBlocks(document).Where(b => b.Source is not null).ToList();
        var deserializedBlocksWithSources = FlattenBlocks(deserialized).Where(b => b.Source is not null).ToList();

        deserializedBlocksWithSources.Should().HaveSameCount(originalBlocksWithSources);
    }

    [Fact]
    public void Document_RenderToMarkdown_ProducesValidOutput()
    {
        var document = TestDocumentFactory.CreateRealisticReport();

        var markdown = MarkdownRenderer.Render(document);

        markdown.Should().NotBeNullOrWhiteSpace();
        markdown.Should().Contain("# Annual Performance Report");
        markdown.Should().Contain("# 1. Executive Summary");
        markdown.Should().Contain("# 2. Financial Overview");
        markdown.Should().Contain("# 3. Strategic Initiatives");
        markdown.Should().Contain("## 2.1 Revenue Breakdown");

        // Should contain table content
        markdown.Should().Contain("Metric");
        markdown.Should().Contain("Revenue ($M)");

        // Should contain figure
        markdown.Should().Contain("Strategic Roadmap");
    }

    [Fact]
    public void SimpleDocument_RenderToMarkdown_ProducesValidOutput()
    {
        var document = TestDocumentFactory.CreateSimpleDocument();

        var markdown = MarkdownRenderer.Render(document);

        markdown.Should().NotBeNullOrWhiteSpace();
        markdown.Should().Contain("# Quick Note");
        markdown.Should().Contain("Introduction");
        markdown.Should().Contain("simple document for testing");
    }

    [Fact]
    public void CitationSet_SerializeDeserialize_RoundTrip()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var citations = CitationBuilder.FromDocument(document);

        citations.Should().NotBeEmpty();

        var citationSet = new CitationSet
        {
            ChunkId = "test-chunk-001",
            Citations = citations.ToList(),
        };

        var json = CitationSerializer.Serialize(citationSet);
        json.Should().NotBeNullOrWhiteSpace();

        var deserialized = CitationSerializer.Deserialize(json);
        deserialized.Should().NotBeNull();
        deserialized.ChunkId.Should().Be(citationSet.ChunkId);
        deserialized.Citations.Should().HaveCount(citationSet.Citations.Count);

        for (int i = 0; i < citationSet.Citations.Count; i++)
        {
            deserialized.Citations[i].FilePath.Should().Be(citationSet.Citations[i].FilePath);
            deserialized.Citations[i].PageNumber.Should().Be(citationSet.Citations[i].PageNumber);
            deserialized.Citations[i].Mode.Should().Be(citationSet.Citations[i].Mode);
        }
    }

    [Fact]
    public void VectorRecords_JsonlExport_ParsesBackCorrectly()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);
        var mapper = new DefaultVectorRecordMapper();
        var records = chunks.Select(c => mapper.MapChunk(c, document)).ToList();

        var jsonl = JsonlExporter.ExportToString(records);
        var lines = jsonl.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines.Should().HaveSameCount(records);

        for (int i = 0; i < lines.Length; i++)
        {
            var parsed = JsonDocument.Parse(lines[i]);
            var root = parsed.RootElement;

            root.GetProperty("id").GetString().Should().Be(records[i].Id);
            root.GetProperty("content").GetString().Should().Be(records[i].Content);
            root.GetProperty("chunkIndex").GetInt32().Should().Be(records[i].ChunkIndex);

            if (records[i].DocumentId is not null)
            {
                root.GetProperty("documentId").GetString().Should().Be(records[i].DocumentId);
            }

            if (records[i].HeadingPath is not null)
            {
                root.GetProperty("headingPath").GetString().Should().Be(records[i].HeadingPath);
            }

            if (records[i].PageNumber is { } pageNumber)
            {
                root.GetProperty("pageNumber").GetInt32().Should().Be(pageNumber);
            }
        }
    }

    [Fact]
    public void CitationFormatter_ProducesReadableOutput()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var citations = CitationBuilder.FromDocument(document);

        foreach (var citation in citations)
        {
            var formatted = CitationFormatter.Format(citation);
            formatted.Should().NotBe("Unknown source");

            var shortForm = CitationFormatter.FormatShort(citation);
            shortForm.Should().NotBe("Unknown");

            var markdown = CitationFormatter.FormatMarkdown(citation);
            markdown.Should().StartWith("[").And.Contain("](");
        }
    }

    [Fact]
    public void Document_SerializeDeserialize_PreservesTableBlock()
    {
        var document = TestDocumentFactory.CreateRealisticReport();

        var json = DocumentSerializer.Serialize(document);
        var deserialized = DocumentSerializer.Deserialize(json)!;

        var originalTable = FlattenBlocks(document).OfType<TableBlock>().First();
        var deserializedTable = FlattenBlocks(deserialized).OfType<TableBlock>().First();

        deserializedTable.Headers.Should().BeEquivalentTo(originalTable.Headers);
        deserializedTable.Rows.Should().HaveSameCount(originalTable.Rows);
    }

    [Fact]
    public void Document_SerializeDeserialize_PreservesFigureBlock()
    {
        var document = TestDocumentFactory.CreateRealisticReport();

        var json = DocumentSerializer.Serialize(document);
        var deserialized = DocumentSerializer.Deserialize(json)!;

        var originalFigure = FlattenBlocks(document).OfType<FigureBlock>().First();
        var deserializedFigure = FlattenBlocks(deserialized).OfType<FigureBlock>().First();

        deserializedFigure.AltText.Should().Be(originalFigure.AltText);
        deserializedFigure.Caption.Should().Be(originalFigure.Caption);
        deserializedFigure.ImagePath.Should().Be(originalFigure.ImagePath);
    }

    private static List<DocumentBlock> FlattenBlocks(Document document)
    {
        var blocks = new List<DocumentBlock>();
        foreach (var section in document.Sections)
        {
            FlattenSection(section, blocks);
        }
        return blocks;
    }

    private static void FlattenSection(DocumentSection section, List<DocumentBlock> blocks)
    {
        if (section.Heading is not null)
        {
            blocks.Add(section.Heading);
        }

        foreach (var block in section.Blocks)
        {
            blocks.Add(block);
        }

        foreach (var sub in section.SubSections)
        {
            FlattenSection(sub, blocks);
        }
    }
}
