using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class PlainTextConverterTests
{
    private readonly PlainTextConverter _converter = new();

    [Fact]
    public void CanHandle_Txt_ReturnsTrue()
    {
        _converter.CanHandle(".txt").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherExtension_ReturnsFalse()
    {
        _converter.CanHandle(".pdf").Should().BeFalse();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".TXT").Should().BeTrue();
        _converter.CanHandle(".Txt").Should().BeTrue();
    }

    [Fact]
    public async Task ConvertAsync_ReturnsFileContent()
    {
        using var stream = new MemoryStream("Hello, Markdown!"u8.ToArray());

        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().Be("Hello, Markdown!");
    }

    [Fact]
    public async Task ConvertAsync_EmptyStream_ReturnsEmptyString()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".txt");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertAsync_UnicodeContent_IsPreserved()
    {
        var unicode = "café, naïve, résumé, 日本語, 🎉";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(unicode));

        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().Be(unicode);
    }

    [Fact]
    public async Task ConvertAsync_MultilineContent_IsPreserved()
    {
        var content = "Line 1\nLine 2\nLine 3";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().Be(content);
    }

    [Fact]
    public async Task ConvertAsync_WithTestDataFile()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.txt");
        if (!File.Exists(testDataPath))
            return; // skip if TestData not copied

        using var stream = File.OpenRead(testDataPath);
        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().Contain("sample text file");
        result.Should().Contain("café");
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".html")]
    [InlineData(".json")]
    [InlineData(".docx")]
    [InlineData(".csv")]
    public void CanHandle_NonTxtExtensions_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }
}
