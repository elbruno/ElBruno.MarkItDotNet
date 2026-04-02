using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class HtmlConverterTests
{
    private readonly HtmlConverter _converter = new();

    [Fact]
    public void CanHandle_Html_ReturnsTrue()
    {
        _converter.CanHandle(".html").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_Htm_ReturnsTrue()
    {
        _converter.CanHandle(".htm").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".HTML").Should().BeTrue();
        _converter.CanHandle(".HTM").Should().BeTrue();
        _converter.CanHandle(".Html").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".json")]
    [InlineData(".pdf")]
    [InlineData(".xml")]
    public void CanHandle_NonHtmlExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_Heading_ConvertsToMarkdownHeading()
    {
        var html = "<h1>Hello World</h1>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("# Hello World");
    }

    [Fact]
    public async Task ConvertAsync_H2_ConvertsToMarkdownH2()
    {
        var html = "<h2>Sub Heading</h2>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("## Sub Heading");
    }

    [Fact]
    public async Task ConvertAsync_Link_ConvertsToMarkdownLink()
    {
        var html = """<a href="https://github.com">GitHub</a>""";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("[GitHub](https://github.com)");
    }

    [Fact]
    public async Task ConvertAsync_Bold_ConvertsToMarkdownBold()
    {
        var html = "<strong>bold text</strong>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("**bold text**");
    }

    [Fact]
    public async Task ConvertAsync_Italic_ConvertsToMarkdownItalic()
    {
        var html = "<em>italic text</em>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("*italic text*");
    }

    [Fact]
    public async Task ConvertAsync_EmptyHtml_ReturnsEmpty()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_WhitespaceOnly_ReturnsEmpty()
    {
        using var stream = new MemoryStream("   \n  "u8.ToArray());

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_Paragraph_ReturnsText()
    {
        var html = "<p>Hello world</p>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("Hello world");
    }

    [Fact]
    public async Task ConvertAsync_ScriptTag_IsStrippedOrIgnored()
    {
        var html = "<p>Content</p><script>alert('xss');</script>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("Content");
        result.Should().NotContain("alert");
    }

    [Fact]
    public async Task ConvertAsync_StyleTag_IsStrippedOrIgnored()
    {
        var html = "<style>body { color: red; }</style><p>Styled</p>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("Styled");
        result.Should().NotContain("color: red");
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".html");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertAsync_UnorderedList_ConvertsToMarkdownList()
    {
        var html = "<ul><li>Item 1</li><li>Item 2</li></ul>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("Item 1");
        result.Should().Contain("Item 2");
    }

    [Fact]
    public async Task ConvertAsync_FullDocument_StripsHeadAndScript()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title><style>body{}</style></head>
            <body>
              <h1>Title</h1>
              <p>Paragraph</p>
              <script>alert('x');</script>
            </body>
            </html>
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("# Title");
        result.Should().Contain("Paragraph");
        result.Should().NotContain("alert");
    }

    [Fact]
    public async Task ConvertAsync_WithTestDataFile()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.html");
        if (!File.Exists(testDataPath))
            return;

        using var stream = File.OpenRead(testDataPath);
        var result = await _converter.ConvertAsync(stream, ".html");

        result.Should().Contain("Main Heading");
        result.Should().Contain("GitHub");
    }
}
