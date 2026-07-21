// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Connectors.Tests;

public sealed class ConnectorsIntegrationTests : IDisposable
{
    private readonly string _rootPath;

    public ConnectorsIntegrationTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "MarkItDotNet.Connectors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
    }

    [Fact]
    public async Task FileSystemConnector_FromServiceProvider_EnumeratesAndReadsDocuments()
    {
        WriteFile("input\\one.md", "# one");
        WriteFile("input\\two.txt", "skip");
        WriteFile("input\\nested\\three.md", "# three");

        var services = new ServiceCollection();
        services.AddFileSystemConnector(options =>
        {
            options.RootPath = Path.Combine(_rootPath, "input");
            options.Recursive = true;
            options.IncludePatterns = ["*.md"];
        });

        using var provider = services.BuildServiceProvider();
        var source = provider.GetRequiredService<IDocumentSource>();

        (await source.ValidateAsync()).Should().BeTrue();
        (await source.CountAsync()).Should().Be(2);

        var docs = new List<SourceDocument>();
        await foreach (var doc in source)
        {
            docs.Add(doc);
        }

        docs.Should().HaveCount(2);
        docs.Select(d => d.Name).Should().BeEquivalentTo(["one.md", "three.md"]);

        await using var stream = await docs[0].OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        (await reader.ReadToEndAsync()).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task FileSystemConnector_FromServiceProvider_ReturnsFalseValidationForMissingPath()
    {
        var services = new ServiceCollection();
        services.AddFileSystemConnector(options =>
        {
            options.RootPath = Path.Combine(_rootPath, "missing");
        });

        using var provider = services.BuildServiceProvider();
        var source = provider.GetRequiredService<IDocumentSource>();

        (await source.ValidateAsync()).Should().BeFalse();
        (await source.CountAsync()).Should().Be(0);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private void WriteFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }
}
