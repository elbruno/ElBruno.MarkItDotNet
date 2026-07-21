using ElBruno.MarkItDotNet.Connectors;
using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class PolicyFilteredDocumentSourceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly MarkdownService _service;

    public PolicyFilteredDocumentSourceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"pf-src-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _service = new ServiceCollection()
            .AddMarkItDotNet()
            .BuildServiceProvider()
            .GetRequiredService<MarkdownService>();
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private async Task WriteFile(string name, string content)
        => await File.WriteAllTextAsync(Path.Combine(_tempDir, name), content);

    private FileSystemConnector MakeConnector()
        => new(
            new FileSystemConnectorOptions { RootPath = _tempDir },
            NullLogger<FileSystemConnector>.Instance);

    // --- metadata predicate filter ---

    [Fact]
    public async Task MetadataPredicate_ExcludesNonMatchingDocs()
    {
        await WriteFile("keep.txt", "keep me");
        await WriteFile("skip.log", "skip me");

        var filtered = new PolicyFilteredDocumentSource(
            MakeConnector(),
            doc => doc.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

        var docs = new List<SourceDocument>();
        await foreach (var doc in filtered.GetDocumentsAsync())
            docs.Add(doc);

        docs.Should().HaveCount(1);
        docs[0].Name.Should().EndWith(".txt");
    }

    [Fact]
    public async Task MetadataPredicate_AllMatch_AllDocumentsYielded()
    {
        await WriteFile("a.txt", "one");
        await WriteFile("b.txt", "two");

        var filtered = new PolicyFilteredDocumentSource(MakeConnector(), _ => true);

        var count = 0;
        await foreach (var _ in filtered.GetDocumentsAsync()) count++;

        count.Should().Be(2);
    }

    [Fact]
    public async Task MetadataPredicate_NoneMatch_EmptyResult()
    {
        await WriteFile("a.txt", "one");

        var filtered = new PolicyFilteredDocumentSource(MakeConnector(), _ => false);

        var count = 0;
        await foreach (var _ in filtered.GetDocumentsAsync()) count++;

        count.Should().Be(0);
    }

    [Fact]
    public async Task CountAsync_ReflectsFilteredCount()
    {
        await WriteFile("keep.txt", "keep");
        await WriteFile("skip.log", "skip");

        var filtered = new PolicyFilteredDocumentSource(
            MakeConnector(),
            doc => doc.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

        var count = await filtered.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task ValidateAsync_DelegatesToInner()
    {
        var filtered = new PolicyFilteredDocumentSource(MakeConnector(), _ => true);
        var valid = await filtered.ValidateAsync();
        valid.Should().BeTrue();
    }

    // --- content policy filter ---

    [Fact]
    public async Task ContentPolicy_ExcludesDocsWithPii()
    {
        await WriteFile("clean.txt", "# Clean document with no sensitive data.");
        await WriteFile("pii.txt", "SSN: 123-45-6789");

        var policy = new PiiDetector();
        var filtered = new PolicyFilteredDocumentSource(MakeConnector(), _service, policy);

        var docs = new List<SourceDocument>();
        await foreach (var doc in filtered.GetDocumentsAsync())
            docs.Add(doc);

        // Only the clean file passes
        docs.Should().HaveCount(1);
        docs[0].Name.Should().Be("clean.txt");
    }

    [Fact]
    public async Task ContentPolicy_AllClean_AllDocumentsYielded()
    {
        await WriteFile("a.txt", "# Title A\n\nClean content.");
        await WriteFile("b.txt", "# Title B\n\nAlso clean.");

        var policy = new PiiDetector();
        var filtered = new PolicyFilteredDocumentSource(MakeConnector(), _service, policy);

        var count = 0;
        await foreach (var _ in filtered.GetDocumentsAsync()) count++;

        count.Should().Be(2);
    }

    [Fact]
    public async Task MetadataAndContentPolicy_BothApplied()
    {
        await WriteFile("safe.txt", "# Safe document.");
        await WriteFile("pii.txt", "Email: user@example.com");
        await WriteFile("excluded.log", "Not a txt file.");

        var policy = new PiiDetector();
        // Metadata: only .txt files; content: no PII
        var filtered = new PolicyFilteredDocumentSource(
            MakeConnector(),
            _service,
            policy,
            doc => doc.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

        var docs = new List<SourceDocument>();
        await foreach (var doc in filtered.GetDocumentsAsync())
            docs.Add(doc);

        docs.Should().HaveCount(1);
        docs[0].Name.Should().Be("safe.txt");
    }

    // --- AsyncEnumerable interface ---

    [Fact]
    public async Task AsyncEnumerable_DirectAwaitForeach_Works()
    {
        await WriteFile("x.txt", "content");
        var filtered = new PolicyFilteredDocumentSource(MakeConnector(), _ => true);

        var count = 0;
        await foreach (var _ in filtered) count++;

        count.Should().Be(1);
    }
}
