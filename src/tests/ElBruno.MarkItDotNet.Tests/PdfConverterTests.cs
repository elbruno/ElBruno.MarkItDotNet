using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class PdfConverterTests
{
    private readonly PdfConverter _converter = new();

    [Fact]
    public void CanHandle_Pdf_ReturnsTrue()
    {
        _converter.CanHandle(".pdf").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".PDF").Should().BeTrue();
        _converter.CanHandle(".Pdf").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".docx")]
    [InlineData(".html")]
    [InlineData(".json")]
    public void CanHandle_NonPdfExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".pdf");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertAsync_MinimalPdf_ReturnsContent()
    {
        // Create a minimal valid PDF in memory
        using var pdfStream = CreateMinimalPdf("Hello from PDF");

        var result = await _converter.ConvertAsync(pdfStream, ".pdf");

        result.Should().Contain("Hello from PDF");
    }

    [Fact]
    public async Task ConvertAsync_EmptyPdf_ReturnsEmptyOrMinimalContent()
    {
        using var pdfStream = CreateMinimalPdf("");

        // An empty-text PDF should not throw; result can be empty or whitespace
        var act = async () => await _converter.ConvertAsync(pdfStream, ".pdf");
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Creates a minimal valid PDF document containing the given text.
    /// This uses raw PDF syntax to avoid requiring a third-party PDF generation library.
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
