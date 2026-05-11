using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

/// <summary>
/// Backward-compatibility tests for the <see cref="MarkdownConverter"/> façade.
/// </summary>
public class MarkdownConverterTests
{
    private readonly MarkdownConverter _converter = new();

    [Fact]
    public void ConvertToMarkdown_WithNullPath_ThrowsArgumentException()
    {
        var act = () => _converter.ConvertToMarkdown(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertToMarkdown_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _converter.ConvertToMarkdown(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertToMarkdown_WithUnsupportedFormat_ThrowsNotSupportedException()
    {
        var act = () => _converter.ConvertToMarkdown("test.xyz");
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*not supported*");
    }

    [Fact]
    public async Task ConvertAsync_WithStreamAndExplicitFileExtension_ReturnsMarkdown()
    {
        using var stream = new MemoryStream("Stream content"u8.ToArray());

        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().Be("Stream content");
    }

    [Fact]
    public async Task ConvertAsync_WithFilePath_ReturnsMarkdown()
    {
        var testDirectory = Path.Combine(AppContext.BaseDirectory, "Issue11RegressionTests");
        Directory.CreateDirectory(testDirectory);
        var filePath = Path.Combine(testDirectory, $"{Guid.NewGuid():N}.txt");

        try
        {
            await File.WriteAllTextAsync(filePath, "Async file path content");

            var result = await _converter.ConvertAsync(filePath);

            result.Should().Be("Async file path content");
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public async Task ConvertAsync_WithUnsupportedStreamFormat_ThrowsNotSupportedException()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var act = () => _converter.ConvertAsync(stream, ".xyz");

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*not supported*");
    }
}
