// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ElBruno.MarkItDotNet.Connectors.Tests;

public sealed class ZipArchiveConnectorTests : IDisposable
{
    private readonly string _rootPath;

    public ZipArchiveConnectorTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "MarkItDotNet.Connectors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
    }

    [Fact]
    public async Task GetDocumentsAsync_EnumeratesNestedEntriesAndStreamsContentWithoutExtraction()
    {
        var archivePath = CreateArchive(("docs/guide.md", "# Guide"), ("notes.txt", "Notes"));
        var connector = CreateConnector(new ZipArchiveConnectorOptions { ArchivePath = archivePath });

        var documents = await ToListAsync(connector.GetDocumentsAsync());

        documents.Select(document => document.Name).Should().BeEquivalentTo("guide.md", "notes.txt");
        documents.Should().ContainSingle(document =>
            document.Metadata[SourceMetadataKeys.ArchiveEntryPath] == "docs/guide.md");
        documents.Single(document => document.Name == "guide.md").Source
            .Should().Be($"{Path.GetFullPath(archivePath)}!/docs/guide.md");

        var guide = documents.Single(document => document.Name == "guide.md");
        await using var stream = await guide.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        (await reader.ReadToEndAsync()).Should().Be("# Guide");
        Directory.GetFiles(_rootPath).Should().ContainSingle().Which.Should().Be(archivePath);
    }

    [Fact]
    public async Task GetDocumentsAsync_SkipsEntriesAboveThePerEntryLimit()
    {
        var archivePath = CreateArchive(("small.txt", "ok"), ("large.txt", new string('x', 100)));
        var connector = CreateConnector(new ZipArchiveConnectorOptions
        {
            ArchivePath = archivePath,
            MaxEntrySizeBytes = 10
        });

        var documents = await ToListAsync(connector.GetDocumentsAsync());

        documents.Should().ContainSingle().Which.Name.Should().Be("small.txt");
    }

    [Fact]
    public async Task GetDocumentsAsync_RejectsArchivesAboveEntryCountLimit()
    {
        var archivePath = CreateArchive(("one.txt", "1"), ("two.txt", "2"));
        var connector = CreateConnector(new ZipArchiveConnectorOptions
        {
            ArchivePath = archivePath,
            MaxEntries = 1
        });

        var act = async () => await ToListAsync(connector.GetDocumentsAsync());

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Fact]
    public async Task GetDocumentsAsync_RejectsArchivesAboveTotalUncompressedLimit()
    {
        var archivePath = CreateArchive(("one.txt", "1234"), ("two.txt", "5678"));
        var connector = CreateConnector(new ZipArchiveConnectorOptions
        {
            ArchivePath = archivePath,
            MaxTotalUncompressedBytes = 7
        });

        var act = async () => await ToListAsync(connector.GetDocumentsAsync());

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFalse_WhenArchiveExceedsCompressedSizeLimit()
    {
        var archivePath = CreateArchive(("file.txt", new string('x', 100)));
        var connector = CreateConnector(new ZipArchiveConnectorOptions
        {
            ArchivePath = archivePath,
            MaxArchiveSizeBytes = 1
        });

        (await connector.ValidateAsync()).Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private ZipArchiveConnector CreateConnector(ZipArchiveConnectorOptions options) =>
        new(options, new TestLogger<ZipArchiveConnector>());

    private string CreateArchive(params (string Name, string Content)[] entries)
    {
        var archivePath = Path.Combine(_rootPath, "documents.zip");
        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            using var writer = new StreamWriter(archive.CreateEntry(name).Open());
            writer.Write(content);
        }

        return archivePath;
    }

    private static async Task<List<SourceDocument>> ToListAsync(IAsyncEnumerable<SourceDocument> source)
    {
        var documents = new List<SourceDocument>();
        await foreach (var item in source.ConfigureAwait(false))
        {
            documents.Add(item);
        }

        return documents;
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull =>
            NullDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose()
            {
            }
        }
    }
}
