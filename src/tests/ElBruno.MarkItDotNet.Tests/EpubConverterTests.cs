using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class EpubConverterTests
{
    private readonly EpubConverter _converter = new();

    [Fact]
    public void CanHandle_Epub_ReturnsTrue()
    {
        _converter.CanHandle(".epub").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".EPUB").Should().BeTrue();
        _converter.CanHandle(".Epub").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".html")]
    public void CanHandle_NonEpubExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".epub");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
