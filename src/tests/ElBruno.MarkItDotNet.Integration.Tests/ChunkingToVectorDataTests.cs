// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.VectorData;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// Tests the Chunking → VectorData → JSONL pipeline with all three chunking strategies.
/// </summary>
public class ChunkingToVectorDataTests
{
    private readonly Document _document = TestDocumentFactory.CreateRealisticReport();
    private readonly DefaultVectorRecordMapper _mapper = new();

    [Fact]
    public void HeadingBasedChunker_MapsToVectorRecordsWithHeadingPaths()
    {
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(_document);
        var records = MapAndVerify(chunks);

        // Heading-based chunker should produce heading paths
        records.Where(r => r.HeadingPath is not null).Should().NotBeEmpty();
        records.Should().AllSatisfy(r => r.Tags.Should().NotBeNull());
    }

    [Fact]
    public void ParagraphBasedChunker_MapsToVectorRecordsCorrectly()
    {
        var chunker = new ParagraphBasedChunker();
        var options = new ChunkingOptions { MaxChunkSize = 500, OverlapSize = 50 };
        var chunks = chunker.Chunk(_document, options);
        var records = MapAndVerify(chunks);

        records.Should().AllSatisfy(r =>
        {
            r.Content.Length.Should().BeGreaterThan(0);
            r.DocumentId.Should().Be(_document.Id);
        });
    }

    [Fact]
    public void TokenAwareChunker_MapsToVectorRecordsCorrectly()
    {
        var chunker = new TokenAwareChunker();
        var options = new ChunkingOptions { MaxChunkSize = 100, OverlapSize = 10 };
        var chunks = chunker.Chunk(_document, options);
        var records = MapAndVerify(chunks);

        records.Should().AllSatisfy(r =>
        {
            r.Content.Should().NotBeNullOrWhiteSpace();
            r.ChunkIndex.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public void AllStrategies_ProduceValidJsonl()
    {
        IChunkingStrategy[] strategies =
        [
            new HeadingBasedChunker(),
            new ParagraphBasedChunker(),
            new TokenAwareChunker(),
        ];

        foreach (var strategy in strategies)
        {
            var chunks = strategy.Chunk(_document);
            var records = chunks.Select(c => _mapper.MapChunk(c, _document)).ToList();
            var jsonl = JsonlExporter.ExportToString(records);

            var lines = jsonl.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            lines.Should().HaveSameCount(records, $"strategy '{strategy.Name}' should produce one line per record");

            foreach (var line in lines)
            {
                var action = () => JsonDocument.Parse(line);
                action.Should().NotThrow($"strategy '{strategy.Name}' should produce valid JSON lines");
            }
        }
    }

    [Fact]
    public void VectorRecords_CarrySourceMetadataFromChunks()
    {
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(_document);

        foreach (var chunk in chunks)
        {
            var record = _mapper.MapChunk(chunk, _document);

            record.Id.Should().Be(chunk.Id);
            record.Content.Should().Be(chunk.Content);
            record.ChunkIndex.Should().Be(chunk.Index);
            record.HeadingPath.Should().Be(chunk.HeadingPath);
            record.DocumentId.Should().Be(_document.Id);
            record.DocumentTitle.Should().Be(_document.Metadata.Title);

            if (chunk.Sources.Count > 0)
            {
                record.PageNumber.Should().Be(chunk.Sources[0].PageNumber);
                record.FilePath.Should().NotBeNullOrEmpty();
            }
        }
    }

    [Fact]
    public void JsonlExport_ViaStream_MatchesStringExport()
    {
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(_document);
        var records = chunks.Select(c => _mapper.MapChunk(c, _document)).ToList();

        var stringOutput = JsonlExporter.ExportToString(records);

        using var memoryStream = new MemoryStream();
        JsonlExporter.ExportToStream(records, memoryStream);
        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        var streamOutput = reader.ReadToEnd();

        streamOutput.Should().Be(stringOutput);
    }

    private List<VectorRecord> MapAndVerify(IReadOnlyList<ChunkResult> chunks)
    {
        chunks.Should().NotBeEmpty();
        var records = chunks.Select(c => _mapper.MapChunk(c, _document)).ToList();

        records.Should().HaveSameCount(chunks);
        records.Should().AllSatisfy(r =>
        {
            r.Id.Should().NotBeNullOrEmpty();
            r.Content.Should().NotBeNullOrWhiteSpace();
            r.DocumentId.Should().Be(_document.Id);
        });

        return records;
    }
}
