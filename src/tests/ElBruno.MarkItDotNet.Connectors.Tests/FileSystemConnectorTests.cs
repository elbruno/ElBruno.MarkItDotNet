// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ElBruno.MarkItDotNet.Connectors.Tests;

public sealed class FileSystemConnectorTests : IDisposable
{
    private readonly string _rootPath;

    public FileSystemConnectorTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "MarkItDotNet.Connectors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
    }

    [Fact]
    public async Task GetDocumentsAsync_WithRecursiveAndMaxDepth_RespectsDepthLimit()
    {
        WriteFile("root.md", "root");
        WriteFile(Path.Combine("nested", "level1.md"), "level1");
        WriteFile(Path.Combine("nested", "deep", "level2.md"), "level2");

        var connector = CreateConnector(new FileSystemConnectorOptions
        {
            RootPath = _rootPath,
            Recursive = true,
            MaxDepth = 1
        });

        var documents = await ToListAsync(connector.GetDocumentsAsync());

        documents.Select(d => d.Name).Should().BeEquivalentTo(["root.md", "level1.md"]);
    }

    [Fact]
    public async Task GetDocumentsAsync_WithIncludePatterns_FiltersFiles()
    {
        WriteFile("one.md", "markdown");
        WriteFile("two.txt", "text");
        WriteFile(Path.Combine("docs", "three.md"), "nested markdown");

        var connector = CreateConnector(new FileSystemConnectorOptions
        {
            RootPath = _rootPath,
            Recursive = true,
            IncludePatterns = ["*.md", "docs/**/*.md"]
        });

        var documents = await ToListAsync(connector.GetDocumentsAsync());

        documents.Select(d => d.Name).Should().BeEquivalentTo(["one.md", "three.md"]);
    }

    [Fact]
    public async Task GetDocumentsAsync_WithMaxFileSize_SkipsLargeFiles()
    {
        WriteFile("small.md", "small");
        WriteFile("large.md", new string('x', 1024));

        var connector = CreateConnector(new FileSystemConnectorOptions
        {
            RootPath = _rootPath,
            MaxFileSizeBytes = 100
        });

        var documents = await ToListAsync(connector.GetDocumentsAsync());

        documents.Should().ContainSingle(d => d.Name == "small.md");
        documents.Should().NotContain(d => d.Name == "large.md");
    }

    [Fact]
    public async Task GetDocumentsAsync_ExtractsExpectedMetadata()
    {
        var content = "metadata-content";
        WriteFile(Path.Combine("docs", "meta.md"), content);

        var connector = CreateConnector(new FileSystemConnectorOptions
        {
            RootPath = _rootPath,
            Recursive = true
        });

        var document = await FirstAsync(connector.GetDocumentsAsync());

        document.Metadata.Should().ContainKey(SourceMetadataKeys.SourcePath);
        document.Metadata.Should().ContainKey(SourceMetadataKeys.RelativePath).WhoseValue.Should().Be("docs/meta.md");
        document.Metadata.Should().ContainKey(SourceMetadataKeys.FileName).WhoseValue.Should().Be("meta.md");
        document.Metadata.Should().ContainKey(SourceMetadataKeys.Extension).WhoseValue.Should().Be(".md");
        document.Metadata.Should().ContainKey(SourceMetadataKeys.FileSizeBytes).WhoseValue.Should().Be(content.Length.ToString());
        document.Metadata.Should().ContainKey(SourceMetadataKeys.Depth).WhoseValue.Should().Be("1");
    }

    [Fact]
    public async Task SourceDocument_OpenReadAsync_StreamsFileContent()
    {
        const string content = "stream-me";
        WriteFile("stream.md", content);

        var connector = CreateConnector(new FileSystemConnectorOptions
        {
            RootPath = _rootPath
        });

        var document = await FirstAsync(connector.GetDocumentsAsync());
        await using var stream = await document.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var loaded = await reader.ReadToEndAsync();

        loaded.Should().Be(content);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFalse_WhenRootPathDoesNotExist()
    {
        var nonExistentPath = Path.Combine(_rootPath, "missing-root");
        var connector = CreateConnector(new FileSystemConnectorOptions
        {
            RootPath = nonExistentPath
        });

        var isValid = await connector.ValidateAsync();

        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ReturnsFilteredDocumentCount()
    {
        WriteFile("one.md", "one");
        WriteFile("two.txt", "two");
        WriteFile(Path.Combine("nested", "three.md"), "three");

        var connector = CreateConnector(new FileSystemConnectorOptions
        {
            RootPath = _rootPath,
            Recursive = true,
            IncludePatterns = ["*.md"]
        });

        var count = await connector.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task Connector_SupportsDirectAsyncEnumeration()
    {
        WriteFile("one.md", "one");
        WriteFile("two.md", "two");
        var connector = CreateConnector(new FileSystemConnectorOptions { RootPath = _rootPath });

        var count = 0;
        await foreach (var _ in connector)
        {
            count++;
        }

        count.Should().Be(2);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private FileSystemConnector CreateConnector(FileSystemConnectorOptions options)
    {
        var logger = new TestLogger<FileSystemConnector>();
        return new FileSystemConnector(options, logger);
    }

    private void WriteFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
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

    private static async Task<SourceDocument> FirstAsync(IAsyncEnumerable<SourceDocument> source)
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            return item;
        }

        throw new InvalidOperationException("No documents found.");
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullDisposable.Instance;
        }

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
