using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class DocxConverterTests
{
    private readonly DocxConverter _converter = new();

    [Fact]
    public void CanHandle_Docx_ReturnsTrue()
    {
        _converter.CanHandle(".docx").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".DOCX").Should().BeTrue();
        _converter.CanHandle(".Docx").Should().BeTrue();
    }

    [Theory]
    [InlineData(".doc")]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".html")]
    public void CanHandle_NonDocxExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".docx");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertAsync_SimpleParagraph_ReturnsText()
    {
        using var docxStream = CreateDocx(body =>
        {
            body.Append(new Paragraph(
                new Run(new Text("Hello from DOCX"))));
        });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().Contain("Hello from DOCX");
    }

    [Fact]
    public async Task ConvertAsync_Heading1_ConvertsToMarkdownH1()
    {
        using var docxStream = CreateDocx(body =>
        {
            var para = new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
                new Run(new Text("Title")));
            body.Append(para);
        });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().Contain("# Title");
    }

    [Fact]
    public async Task ConvertAsync_Heading2_ConvertsToMarkdownH2()
    {
        using var docxStream = CreateDocx(body =>
        {
            var para = new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading2" }),
                new Run(new Text("Subtitle")));
            body.Append(para);
        });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().Contain("## Subtitle");
    }

    [Fact]
    public async Task ConvertAsync_BoldText_ConvertsToMarkdownBold()
    {
        using var docxStream = CreateDocx(body =>
        {
            var run = new Run(
                new RunProperties(new Bold()),
                new Text("bold text"));
            body.Append(new Paragraph(run));
        });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().Contain("**bold text**");
    }

    [Fact]
    public async Task ConvertAsync_ItalicText_ConvertsToMarkdownItalic()
    {
        using var docxStream = CreateDocx(body =>
        {
            var run = new Run(
                new RunProperties(new Italic()),
                new Text("italic text"));
            body.Append(new Paragraph(run));
        });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().Contain("*italic text*");
    }

    [Fact]
    public async Task ConvertAsync_MultipleParagraphs_ReturnsAllText()
    {
        using var docxStream = CreateDocx(body =>
        {
            body.Append(new Paragraph(new Run(new Text("First paragraph"))));
            body.Append(new Paragraph(new Run(new Text("Second paragraph"))));
        });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().Contain("First paragraph");
        result.Should().Contain("Second paragraph");
    }

    [Fact]
    public async Task ConvertAsync_EmptyDocument_ReturnsEmpty()
    {
        using var docxStream = CreateDocx(_ => { });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_Table_ConvertsToMarkdownTable()
    {
        using var docxStream = CreateDocx(body =>
        {
            var table = new Table(
                new TableRow(
                    new TableCell(new Paragraph(new Run(new Text("Header1")))),
                    new TableCell(new Paragraph(new Run(new Text("Header2"))))),
                new TableRow(
                    new TableCell(new Paragraph(new Run(new Text("Cell1")))),
                    new TableCell(new Paragraph(new Run(new Text("Cell2"))))));
            body.Append(table);
        });

        var result = await _converter.ConvertAsync(docxStream, ".docx");

        result.Should().Contain("|");
        result.Should().Contain("Header1");
        result.Should().Contain("Cell1");
        result.Should().Contain("---");
    }

    /// <summary>
    /// Helper to create an in-memory .docx file from a body-building action.
    /// </summary>
    private static MemoryStream CreateDocx(Action<Body> buildBody)
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();
            buildBody(body);
            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }
        ms.Position = 0;
        return ms;
    }
}
