// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.VectorData.Tests;

public class JsonlExporterTests
{
    [Fact]
    public void ExportToString_SingleRecord_OutputsOneJsonLine()
    {
        var records = new[]
        {
            new VectorRecord { Id = "chunk-1", Content = "Hello world", ChunkIndex = 0 },
        };

        var result = JsonlExporter.ExportToString(records);
        var lines = result.TrimEnd().Split('\n');

        lines.Should().HaveCount(1);
        var doc = JsonDocument.Parse(lines[0]);
        doc.RootElement.GetProperty("id").GetString().Should().Be("chunk-1");
        doc.RootElement.GetProperty("content").GetString().Should().Be("Hello world");
        doc.RootElement.GetProperty("chunkIndex").GetInt32().Should().Be(0);
    }

    [Fact]
    public void ExportToString_MultipleRecords_OutputsOneJsonPerLine()
    {
        var records = new[]
        {
            new VectorRecord { Id = "chunk-1", Content = "First", ChunkIndex = 0 },
            new VectorRecord { Id = "chunk-2", Content = "Second", ChunkIndex = 1 },
            new VectorRecord { Id = "chunk-3", Content = "Third", ChunkIndex = 2 },
        };

        var result = JsonlExporter.ExportToString(records);
        var lines = result.TrimEnd().Split('\n');

        lines.Should().HaveCount(3);

        for (int i = 0; i < 3; i++)
        {
            var doc = JsonDocument.Parse(lines[i]);
            doc.RootElement.GetProperty("id").GetString().Should().Be($"chunk-{i + 1}");
        }
    }

    [Fact]
    public void ExportToString_EmptyRecords_ReturnsEmptyString()
    {
        var records = Array.Empty<VectorRecord>();

        var result = JsonlExporter.ExportToString(records);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ExportToString_SpecialCharacters_AreProperlyEscaped()
    {
        var records = new[]
        {
            new VectorRecord
            {
                Id = "chunk-1",
                Content = "Line with \"quotes\" and \\ backslash and unicode: \u00e9",
                ChunkIndex = 0,
            },
        };

        var result = JsonlExporter.ExportToString(records);
        var lines = result.TrimEnd().Split('\n');

        lines.Should().HaveCount(1);
        var doc = JsonDocument.Parse(lines[0]);
        doc.RootElement.GetProperty("content").GetString()
            .Should().Be("Line with \"quotes\" and \\ backslash and unicode: \u00e9");
    }

    [Fact]
    public void ExportToString_NullProperties_AreOmitted()
    {
        var records = new[]
        {
            new VectorRecord { Id = "chunk-1", Content = "Test", ChunkIndex = 0 },
        };

        var result = JsonlExporter.ExportToString(records);
        var doc = JsonDocument.Parse(result.TrimEnd());

        doc.RootElement.TryGetProperty("documentId", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("embedding", out _).Should().BeFalse();
    }

    [Fact]
    public void ExportToStream_WritesToStream()
    {
        var records = new[]
        {
            new VectorRecord { Id = "chunk-1", Content = "Stream test", ChunkIndex = 0 },
        };

        using var stream = new MemoryStream();
        JsonlExporter.ExportToStream(records, stream);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = reader.ReadToEnd();

        content.TrimEnd().Should().NotBeEmpty();
        var doc = JsonDocument.Parse(content.TrimEnd());
        doc.RootElement.GetProperty("id").GetString().Should().Be("chunk-1");
    }

    [Fact]
    public void ExportToFile_CreatesValidJsonlFile()
    {
        var records = new[]
        {
            new VectorRecord { Id = "chunk-1", Content = "File test", ChunkIndex = 0 },
            new VectorRecord { Id = "chunk-2", Content = "Second record", ChunkIndex = 1 },
        };

        var filePath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.jsonl");

        try
        {
            JsonlExporter.ExportToFile(records, filePath);

            File.Exists(filePath).Should().BeTrue();
            var lines = File.ReadAllLines(filePath);
            lines.Should().HaveCount(2);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ExportToString_NullRecords_ThrowsArgumentNullException()
    {
        var act = () => JsonlExporter.ExportToString(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExportToStream_NullStream_ThrowsArgumentNullException()
    {
        var act = () => JsonlExporter.ExportToStream([], null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExportToString_WithMetadata_IncludesMetadataInOutput()
    {
        var records = new[]
        {
            new VectorRecord
            {
                Id = "chunk-1",
                Content = "With metadata",
                ChunkIndex = 0,
                Metadata = new Dictionary<string, object> { ["lang"] = "en", ["score"] = 0.95 },
            },
        };

        var result = JsonlExporter.ExportToString(records);
        var doc = JsonDocument.Parse(result.TrimEnd());

        doc.RootElement.TryGetProperty("metadata", out var metadata).Should().BeTrue();
        metadata.GetProperty("lang").GetString().Should().Be("en");
    }
}
