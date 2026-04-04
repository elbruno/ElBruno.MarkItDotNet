using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.GoldenTests;

/// <summary>
/// Golden file tests that convert each shared sample document and compare
/// the output against the expected markdown golden file.
/// </summary>
public class GoldenFileTests : IClassFixture<MarkdownServiceFixture>
{
    private readonly MarkdownService _service;

    public GoldenFileTests(MarkdownServiceFixture fixture)
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
    public async Task Convert_ShouldMatchGoldenFile(string filename)
    {
        var documentPath = TestPaths.GetDocument(filename);
        var expectedPath = TestPaths.GetExpectedMarkItDotNet(filename);

        File.Exists(documentPath).Should().BeTrue($"source document '{filename}' should exist");
        File.Exists(expectedPath).Should().BeTrue($"golden file for '{filename}' should exist");

        var ext = Path.GetExtension(filename).ToLowerInvariant();
        using var stream = File.OpenRead(documentPath);
        var result = await _service.ConvertAsync(stream, ext);

        result.Success.Should().BeTrue($"conversion of '{filename}' should succeed");

        var expected = await File.ReadAllTextAsync(expectedPath);
        result.Markdown.Should().Be(expected, $"output for '{filename}' should match golden file");
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
    public async Task Convert_ShouldSucceedAndProduceNonEmptyOutput(string filename)
    {
        var documentPath = TestPaths.GetDocument(filename);
        File.Exists(documentPath).Should().BeTrue($"source document '{filename}' should exist");

        var ext = Path.GetExtension(filename).ToLowerInvariant();
        using var stream = File.OpenRead(documentPath);
        var result = await _service.ConvertAsync(stream, ext);

        result.Success.Should().BeTrue($"conversion of '{filename}' should succeed");
        result.Markdown.Should().NotBeNullOrWhiteSpace($"conversion of '{filename}' should produce output");
        result.SourceFormat.Should().Be(ext);
    }
}
