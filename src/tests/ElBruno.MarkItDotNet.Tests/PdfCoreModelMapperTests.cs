using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class PdfCoreModelMapperTests
{
    private readonly PdfCoreModelMapper _mapper = new();

    [Fact]
    public void CanHandle_PdfFile_ReturnsTrue()
    {
        _mapper.CanHandle("document.pdf").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_PdfFile_CaseInsensitive()
    {
        _mapper.CanHandle("document.PDF").Should().BeTrue();
        _mapper.CanHandle("document.Pdf").Should().BeTrue();
    }

    [Theory]
    [InlineData("document.txt")]
    [InlineData("document.docx")]
    [InlineData("document.html")]
    public void CanHandle_NonPdfFile_ReturnsFalse(string filePath)
    {
        _mapper.CanHandle(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertToDocumentAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _mapper.ConvertToDocumentAsync(null!, "test.pdf");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertToDocumentAsync_NullFileName_ThrowsArgumentException()
    {
        using var stream = new MemoryStream();
        var act = () => _mapper.ConvertToDocumentAsync(stream, null!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ConvertToDocumentAsync_MinimalPdf_ReturnsDocumentWithContent()
    {
        using var pdfStream = CreateMinimalPdf("Hello from PDF");

        var result = await _mapper.ConvertToDocumentAsync(pdfStream, "test.pdf");

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Metadata.Should().NotBeNull();
        result.Metadata.SourceFormat.Should().Be(".pdf");
        result.Metadata.PageCount.Should().Be(1);
        result.Source.Should().NotBeNull();
        result.Source!.FilePath.Should().Be("test.pdf");

        // Should have at least one section with content
        result.Sections.Should().NotBeEmpty();
        var allBlocks = result.Sections.SelectMany(s => s.Blocks).ToList();
        allBlocks.Should().NotBeEmpty();

        // The text should appear in a ParagraphBlock
        var paragraphs = allBlocks.OfType<CoreModel.ParagraphBlock>().ToList();
        paragraphs.Should().Contain(p => p.Text.Contains("Hello from PDF"));
    }

    [Fact]
    public async Task ConvertToDocumentAsync_MinimalPdf_SetsPageNumberInSource()
    {
        using var pdfStream = CreateMinimalPdf("Test content");

        var result = await _mapper.ConvertToDocumentAsync(pdfStream, "test.pdf");

        var blocks = result.Sections.SelectMany(s => s.Blocks).ToList();
        blocks.Should().NotBeEmpty();

        // All blocks should have a source reference with page 1
        foreach (var block in blocks)
        {
            block.Source.Should().NotBeNull();
            block.Source!.PageNumber.Should().Be(1);
        }
    }

    [Fact]
    public async Task ConvertToDocumentAsync_MinimalPdf_CountsWords()
    {
        using var pdfStream = CreateMinimalPdf("Hello from PDF");

        var result = await _mapper.ConvertToDocumentAsync(pdfStream, "test.pdf");

        result.Metadata.WordCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertToDocumentAsync_EmptyPdf_DoesNotThrow()
    {
        using var pdfStream = CreateMinimalPdf("");

        var act = async () => await _mapper.ConvertToDocumentAsync(pdfStream, "empty.pdf");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ConvertToDocumentAsync_EmptyPdf_ReturnsDocumentWithMetadata()
    {
        using var pdfStream = CreateMinimalPdf("");

        var result = await _mapper.ConvertToDocumentAsync(pdfStream, "empty.pdf");

        result.Should().NotBeNull();
        result.Metadata.SourceFormat.Should().Be(".pdf");
        result.Metadata.PageCount.Should().Be(1);
    }

    /// <summary>
    /// Creates a minimal valid PDF document containing the given text.
    /// </summary>
    private static MemoryStream CreateMinimalPdf(string text)
    {
        var escapedText = text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var streamContent = $"BT /F1 12 Tf 100 700 Td ({escapedText}) Tj ET";
        var streamBytes = System.Text.Encoding.ASCII.GetBytes(streamContent);

        var pdf = new System.Text.StringBuilder();
        pdf.AppendLine("%PDF-1.4");

        // Object 1: Catalog
        pdf.AppendLine("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj");

        // Object 2: Pages
        pdf.AppendLine("2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj");

        // Object 3: Page
        pdf.AppendLine("3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj");

        // Object 4: Content stream
        pdf.AppendLine($"4 0 obj << /Length {streamBytes.Length} >> stream");
        pdf.Append(streamContent);
        pdf.AppendLine();
        pdf.AppendLine("endstream endobj");

        // Object 5: Font
        pdf.AppendLine("5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj");

        // Cross-reference table (simplified)
        pdf.AppendLine("xref");
        pdf.AppendLine("0 6");
        pdf.AppendLine("0000000000 65535 f ");
        pdf.AppendLine("0000000009 00000 n ");
        pdf.AppendLine("0000000058 00000 n ");
        pdf.AppendLine("0000000115 00000 n ");
        pdf.AppendLine("0000000266 00000 n ");
        pdf.AppendLine("0000000400 00000 n ");

        // Trailer
        pdf.AppendLine("trailer << /Size 6 /Root 1 0 R >>");
        pdf.AppendLine("startxref");
        pdf.AppendLine("9");
        pdf.AppendLine("%%EOF");

        var bytes = System.Text.Encoding.ASCII.GetBytes(pdf.ToString());
        return new MemoryStream(bytes);
    }
}
