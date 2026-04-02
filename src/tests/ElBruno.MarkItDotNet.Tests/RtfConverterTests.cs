using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class RtfConverterTests
{
    private readonly RtfConverter _converter = new();

    [Fact]
    public void CanHandle_Rtf_ReturnsTrue()
    {
        _converter.CanHandle(".rtf").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".RTF").Should().BeTrue();
        _converter.CanHandle(".Rtf").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".html")]
    [InlineData(".doc")]
    public void CanHandle_NonRtfExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_BasicRtf_ProducesMarkdown()
    {
        var rtf = @"{\rtf1\ansi{\b Hello World}}";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rtf));

        var result = await _converter.ConvertAsync(stream, ".rtf");

        result.Should().Contain("Hello World");
    }

    [Fact]
    public async Task ConvertAsync_EmptyContent_ReturnsEmpty()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".rtf");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".rtf");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
