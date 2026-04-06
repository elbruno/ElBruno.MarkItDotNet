using Xunit;
using FluentAssertions;

namespace ElBruno.MarkItDotNet.DocumentIntelligence.Tests;

/// <summary>
/// Tests for <see cref="DocumentIntelligenceConverter"/> CanHandle logic and error handling.
/// These tests do not call Azure — they validate local behavior only.
/// </summary>
public class DocumentIntelligenceConverterTests
{
    private static DocumentIntelligenceConverter CreateConverter(DocumentIntelligenceOptions? options = null)
    {
        return new DocumentIntelligenceConverter(options ?? new DocumentIntelligenceOptions());
    }

    [Theory]
    [InlineData("report.pdf", true)]
    [InlineData("photo.png", true)]
    [InlineData("scan.jpg", true)]
    [InlineData("image.jpeg", true)]
    [InlineData("document.tiff", true)]
    [InlineData("image.bmp", true)]
    [InlineData("document.docx", true)]
    [InlineData("spreadsheet.xlsx", true)]
    [InlineData("presentation.pptx", true)]
    [InlineData("report.PDF", true)]
    [InlineData("notes.txt", false)]
    [InlineData("data.csv", false)]
    [InlineData("page.html", false)]
    [InlineData("config.json", false)]
    [InlineData("", false)]
    public void CanHandle_ShouldReturnExpectedResult(string filePath, bool expected)
    {
        var converter = CreateConverter();

        converter.CanHandle(filePath).Should().Be(expected);
    }

    [Fact]
    public void CanHandle_WithNullPath_ShouldReturnFalse()
    {
        var converter = CreateConverter();

        converter.CanHandle(null!).Should().BeFalse();
    }

    [Fact]
    public void CanHandle_WithCustomExtensions_ShouldRespectConfiguration()
    {
        var options = new DocumentIntelligenceOptions
        {
            SupportedExtensions = [".pdf", ".tiff"]
        };
        var converter = CreateConverter(options);

        converter.CanHandle("document.pdf").Should().BeTrue();
        converter.CanHandle("scan.tiff").Should().BeTrue();
        converter.CanHandle("photo.png").Should().BeFalse();
        converter.CanHandle("image.jpg").Should().BeFalse();
    }

    [Fact]
    public async Task ConvertToDocumentAsync_WithoutEndpoint_ShouldThrowInvalidOperationException()
    {
        var converter = CreateConverter(new DocumentIntelligenceOptions
        {
            Endpoint = null,
            ApiKey = "some-key"
        });

        var act = () => converter.ConvertToDocumentAsync(
            new MemoryStream([0x25, 0x50, 0x44, 0x46]),
            "test.pdf");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*endpoint*not configured*");
    }

    [Fact]
    public async Task ConvertToDocumentAsync_FilePath_WithoutEndpoint_ShouldThrowInvalidOperationException()
    {
        var converter = CreateConverter(new DocumentIntelligenceOptions
        {
            Endpoint = null
        });

        // Using a path that won't exist — but the endpoint check happens before file access
        var act = () => converter.ConvertToDocumentAsync(
            new MemoryStream([0x25, 0x50, 0x44, 0x46]),
            "nonexistent.pdf");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*endpoint*not configured*");
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        var act = () => new DocumentIntelligenceConverter(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
