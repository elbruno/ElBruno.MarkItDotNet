using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class MarkdownPassthroughConverterTests
{
    private readonly MarkdownPassthroughConverter _converter = new();

    [Theory]
    [InlineData(".md")]
    [InlineData(".MD")]
    [InlineData(".Md")]
    [InlineData(".markdown")]
    [InlineData(".MARKDOWN")]
    [InlineData(".Markdown")]
    public void CanHandle_MarkdownExtensions_ReturnsTrue(string extension)
    {
        _converter.CanHandle(extension).Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".html")]
    [InlineData(".docx")]
    public void CanHandle_NonMarkdownExtensions_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_ReturnsContentAsIs()
    {
        var content = "# Hello\n\nThis is **bold** and *italic*.";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var result = await _converter.ConvertAsync(stream, ".md");

        result.Should().Be(content);
    }

    [Fact]
    public async Task ConvertAsync_EmptyStream_ReturnsEmptyString()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".md");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".md");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertAsync_UnicodeContent_IsPreserved()
    {
        var unicode = "# café, naïve, résumé, 日本語, 🎉";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(unicode));

        var result = await _converter.ConvertAsync(stream, ".markdown");

        result.Should().Be(unicode);
    }

    [Fact]
    public async Task ConvertAsync_MultilineMarkdown_IsPreserved()
    {
        var content = "# Title\n\n- item 1\n- item 2\n\n```csharp\nvar x = 1;\n```";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var result = await _converter.ConvertAsync(stream, ".md");

        result.Should().Be(content);
    }

    [Fact]
    public async Task MarkdownService_ConvertAsync_MdFile_ReturnsSuccess()
    {
        var registry = new ConverterRegistry();
        registry.Register(new MarkdownPassthroughConverter());
        var service = new MarkdownService(registry);

        var content = "# Test Markdown";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var result = await service.ConvertAsync(stream, ".md");

        result.Success.Should().BeTrue();
        result.Markdown.Should().Be(content);
    }

    [Fact]
    public void MarkdownConverter_Facade_MdFile_ReturnsContent()
    {
        var converter = new MarkdownConverter();
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.md");

        try
        {
            var content = "# Bulk Convert Test\n\nMarkdown should pass through.";
            File.WriteAllText(tempFile, content);

            var result = converter.ConvertToMarkdown(tempFile);

            result.Should().Be(content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
