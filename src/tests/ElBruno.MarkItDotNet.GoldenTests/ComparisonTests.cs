using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.GoldenTests;

/// <summary>
/// Comparison tests between C# MarkItDotNet and Python markitdown outputs.
/// These tests verify both libraries produce output and highlight differences.
/// </summary>
public class ComparisonTests : IClassFixture<MarkdownServiceFixture>
{
    private readonly MarkdownService _service;

    public ComparisonTests(MarkdownServiceFixture fixture)
    {
        _service = fixture.Service;
    }

    [Theory]
    [InlineData("sample.txt")]
    [InlineData("sample.html")]
    [InlineData("sample.json")]
    [InlineData("sample.csv")]
    [InlineData("sample.xml")]
    [InlineData("sample.yaml")]
    [InlineData("sample.rtf")]
    [InlineData("sample.pdf")]
    [InlineData("sample.docx")]
    [InlineData("sample.epub")]
    [InlineData("sample.xlsx")]
    [InlineData("sample.pptx")]
    public async Task BothLibraries_ShouldProduceNonEmptyOutput(string filename)
    {
        var documentPath = TestPaths.GetDocument(filename);
        var markitdownExpectedPath = TestPaths.GetExpectedMarkItDown(filename);

        if (!File.Exists(markitdownExpectedPath))
        {
            // Skip if Python golden file not available
            return;
        }

        // C# conversion
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        using var stream = File.OpenRead(documentPath);
        var csharpResult = await _service.ConvertAsync(stream, ext);

        // Python golden file
        var pythonOutput = await File.ReadAllTextAsync(markitdownExpectedPath);

        // Both should produce non-empty output
        csharpResult.Success.Should().BeTrue($"C# conversion of '{filename}' should succeed");
        csharpResult.Markdown.Should().NotBeNullOrWhiteSpace($"C# output for '{filename}' should not be empty");
        pythonOutput.Should().NotBeNullOrWhiteSpace($"Python output for '{filename}' should not be empty");
    }

    [Theory]
    [InlineData("sample.txt")]
    [InlineData("sample.html")]
    [InlineData("sample.json")]
    [InlineData("sample.csv")]
    [InlineData("sample.xml")]
    [InlineData("sample.yaml")]
    [InlineData("sample.rtf")]
    [InlineData("sample.pdf")]
    [InlineData("sample.docx")]
    [InlineData("sample.epub")]
    [InlineData("sample.xlsx")]
    [InlineData("sample.pptx")]
    public async Task BothLibraries_ShouldPreserveKeyContent(string filename)
    {
        var documentPath = TestPaths.GetDocument(filename);
        var markitdownExpectedPath = TestPaths.GetExpectedMarkItDown(filename);

        if (!File.Exists(markitdownExpectedPath))
        {
            return;
        }

        var ext = Path.GetExtension(filename).ToLowerInvariant();
        using var stream = File.OpenRead(documentPath);
        var csharpResult = await _service.ConvertAsync(stream, ext);
        var pythonOutput = await File.ReadAllTextAsync(markitdownExpectedPath);

        // Both libraries should extract the same key content from documents.
        // The exact formatting may differ, but core text should be present in both.
        var keyPhrases = GetKeyPhrasesForFormat(filename);
        foreach (var phrase in keyPhrases)
        {
            csharpResult.Markdown.Should().Contain(phrase,
                $"C# output for '{filename}' should contain key phrase '{phrase}'");
            pythonOutput.Should().Contain(phrase,
                $"Python output for '{filename}' should contain key phrase '{phrase}'");
        }
    }

    [Theory]
    [InlineData("sample.txt")]
    [InlineData("sample.html")]
    [InlineData("sample.json")]
    [InlineData("sample.csv")]
    [InlineData("sample.xml")]
    [InlineData("sample.yaml")]
    [InlineData("sample.rtf")]
    [InlineData("sample.pdf")]
    [InlineData("sample.docx")]
    [InlineData("sample.epub")]
    [InlineData("sample.xlsx")]
    [InlineData("sample.pptx")]
    public async Task Comparison_OutputSimilarity(string filename)
    {
        var documentPath = TestPaths.GetDocument(filename);
        var markitdownExpectedPath = TestPaths.GetExpectedMarkItDown(filename);

        if (!File.Exists(markitdownExpectedPath))
        {
            return;
        }

        var ext = Path.GetExtension(filename).ToLowerInvariant();
        using var stream = File.OpenRead(documentPath);
        var csharpResult = await _service.ConvertAsync(stream, ext);
        var pythonOutput = await File.ReadAllTextAsync(markitdownExpectedPath);

        // Extract normalized words from both outputs for a rough similarity check
        var csharpWords = NormalizeToWords(csharpResult.Markdown);
        var pythonWords = NormalizeToWords(pythonOutput);

        // Both should have extracted some words
        csharpWords.Should().NotBeEmpty($"C# should extract words from '{filename}'");
        pythonWords.Should().NotBeEmpty($"Python should extract words from '{filename}'");

        // Check overlap: at least some common words should exist
        var commonWords = csharpWords.Intersect(pythonWords).ToList();
        commonWords.Should().NotBeEmpty(
            $"C# and Python outputs for '{filename}' should share common words");
    }

    private static string[] GetKeyPhrasesForFormat(string filename) => Path.GetExtension(filename).ToLowerInvariant() switch
    {
        ".txt" => ["Sample Document", "MarkItDotNet"],
        ".html" => ["Sample Document", "bold", "italic"],
        ".json" => ["Sample Document", "MarkItDotNet"],
        ".csv" => ["HTML", ".html", "PDF", ".pdf"],
        ".xml" => ["Sample Document", "MarkItDotNet"],
        ".yaml" => ["Sample Document", "MarkItDotNet"],
        ".rtf" => ["Sample Document"],
        ".pdf" => ["Sample Document", "Features"],
        ".docx" => ["Sample Document", "Features"],
        ".epub" => ["Sample Document"],
        ".xlsx" => ["HTML", ".html", "PDF"],
        ".pptx" => ["Sample Document"],
        _ => ["Sample"]
    };

    private static HashSet<string> NormalizeToWords(string text)
    {
        return text
            .Split([' ', '\t', '\n', '\r', '|', '#', '*', '-', '`', '[', ']', '(', ')', '{', '}', '<', '>', ',', '.', ':', ';', '!', '?', '"', '\''],
                StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim().ToLowerInvariant())
            .Where(w => w.Length > 2)
            .ToHashSet();
    }
}
