using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ElBruno.MarkItDotNet.Converters;
using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class DocxCoreModelMapperTests
{
    private readonly DocxCoreModelMapper _mapper = new();

    [Fact]
    public void CanHandle_DocxFile_ReturnsTrue()
    {
        _mapper.CanHandle("document.docx").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_DocxFile_CaseInsensitive()
    {
        _mapper.CanHandle("document.DOCX").Should().BeTrue();
        _mapper.CanHandle("document.Docx").Should().BeTrue();
    }

    [Theory]
    [InlineData("document.doc")]
    [InlineData("document.txt")]
    [InlineData("document.pdf")]
    public void CanHandle_NonDocxFile_ReturnsFalse(string filePath)
    {
        _mapper.CanHandle(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertToDocumentAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _mapper.ConvertToDocumentAsync(null!, "test.docx");
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
    public async Task ConvertToDocumentAsync_SimpleParagraph_ReturnsParagraphBlock()
    {
        using var docxStream = CreateDocx(body =>
        {
            body.Append(new Paragraph(
                new Run(new Text("Hello from DOCX"))));
        });

        var result = await _mapper.ConvertToDocumentAsync(docxStream, "test.docx");

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Metadata.SourceFormat.Should().Be(".docx");
        result.Source.Should().NotBeNull();
        result.Source!.FilePath.Should().Be("test.docx");

        var allBlocks = result.Sections.SelectMany(s => s.Blocks).ToList();
        var paragraphs = allBlocks.OfType<ParagraphBlock>().ToList();
        paragraphs.Should().Contain(p => p.Text.Contains("Hello from DOCX"));
    }

    [Fact]
    public async Task ConvertToDocumentAsync_Heading_CreatesNewSection()
    {
        using var docxStream = CreateDocx(body =>
        {
            body.Append(new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
                new Run(new Text("Title"))));
            body.Append(new Paragraph(
                new Run(new Text("Body text"))));
        });

        var result = await _mapper.ConvertToDocumentAsync(docxStream, "test.docx");

        result.Sections.Should().NotBeEmpty();

        // Find the section with heading "Title"
        var headingSection = result.Sections
            .FirstOrDefault(s => s.Heading?.Text == "Title");
        headingSection.Should().NotBeNull();
        headingSection!.Heading!.Level.Should().Be(1);

        var paragraphs = headingSection.Blocks.OfType<ParagraphBlock>().ToList();
        paragraphs.Should().Contain(p => p.Text == "Body text");
    }

    [Fact]
    public async Task ConvertToDocumentAsync_Heading2_SetsCorrectLevel()
    {
        using var docxStream = CreateDocx(body =>
        {
            body.Append(new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading2" }),
                new Run(new Text("Subtitle"))));
        });

        var result = await _mapper.ConvertToDocumentAsync(docxStream, "test.docx");

        var headingSection = result.Sections
            .FirstOrDefault(s => s.Heading?.Text == "Subtitle");
        headingSection.Should().NotBeNull();
        headingSection!.Heading!.Level.Should().Be(2);
    }

    [Fact]
    public async Task ConvertToDocumentAsync_Table_CreatesTableBlock()
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

        var result = await _mapper.ConvertToDocumentAsync(docxStream, "test.docx");

        var allBlocks = result.Sections.SelectMany(s => s.Blocks).ToList();
        var tables = allBlocks.OfType<TableBlock>().ToList();
        tables.Should().NotBeEmpty();

        var tableBlock = tables.First();
        tableBlock.Headers.Should().Contain("Header1");
        tableBlock.Headers.Should().Contain("Header2");
        tableBlock.Rows.Should().HaveCount(1);
        tableBlock.Rows[0].Should().Contain("Cell1");
        tableBlock.Rows[0].Should().Contain("Cell2");
    }

    [Fact]
    public async Task ConvertToDocumentAsync_EmptyDocument_ReturnsEmptyDocument()
    {
        using var docxStream = CreateDocx(_ => { });

        var result = await _mapper.ConvertToDocumentAsync(docxStream, "empty.docx");

        result.Should().NotBeNull();
        result.Metadata.SourceFormat.Should().Be(".docx");
        result.Sections.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertToDocumentAsync_MultipleParagraphs_CountsWords()
    {
        using var docxStream = CreateDocx(body =>
        {
            body.Append(new Paragraph(new Run(new Text("First paragraph content"))));
            body.Append(new Paragraph(new Run(new Text("Second paragraph content"))));
        });

        var result = await _mapper.ConvertToDocumentAsync(docxStream, "test.docx");

        result.Metadata.WordCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertToDocumentAsync_MultipleSections_PreservesAll()
    {
        using var docxStream = CreateDocx(body =>
        {
            body.Append(new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
                new Run(new Text("Section One"))));
            body.Append(new Paragraph(new Run(new Text("Content one"))));

            body.Append(new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
                new Run(new Text("Section Two"))));
            body.Append(new Paragraph(new Run(new Text("Content two"))));
        });

        var result = await _mapper.ConvertToDocumentAsync(docxStream, "test.docx");

        // Should have sections for both headings
        var allSections = FlattenSections(result.Sections);
        var headings = allSections
            .Where(s => s.Heading is not null)
            .Select(s => s.Heading!.Text)
            .ToList();

        headings.Should().Contain("Section One");
        headings.Should().Contain("Section Two");
    }

    private static List<DocumentSection> FlattenSections(IEnumerable<DocumentSection> sections)
    {
        var result = new List<DocumentSection>();
        foreach (var section in sections)
        {
            result.Add(section);
            result.AddRange(FlattenSections(section.SubSections));
        }
        return result;
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
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            var body = new Body();
            buildBody(body);
            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }
        ms.Position = 0;
        return ms;
    }
}
