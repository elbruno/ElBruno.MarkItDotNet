using Xunit;
using Azure.AI.DocumentIntelligence;
using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;

namespace ElBruno.MarkItDotNet.DocumentIntelligence.Tests;

/// <summary>
/// Tests for <see cref="DocumentIntelligenceMapper"/> that verify Azure Document Intelligence
/// <see cref="AnalyzeResult"/> objects are correctly mapped to CoreModel <see cref="Document"/> types.
/// Uses <see cref="DocumentIntelligenceModelFactory"/> to create test instances of Azure SDK types.
/// </summary>
public class DocumentIntelligenceMapperTests
{
    #region Helper methods

    private static AnalyzeResult CreateAnalyzeResult(
        string content = "",
        IEnumerable<DocumentParagraph>? paragraphs = null,
        IEnumerable<DocumentTable>? tables = null,
        IEnumerable<DocumentFigure>? figures = null,
        IEnumerable<DocumentPage>? pages = null)
    {
        return DocumentIntelligenceModelFactory.AnalyzeResult(
            apiVersion: "2024-11-30",
            modelId: "prebuilt-layout",
            content: content,
            pages: pages ?? [],
            paragraphs: paragraphs,
            tables: tables,
            figures: figures);
    }

    private static DocumentParagraph CreateParagraph(
        string content,
        ParagraphRole? role = null,
        int pageNumber = 1,
        int offset = 0,
        int? length = null)
    {
        var spans = new[] { DocumentIntelligenceModelFactory.DocumentSpan(offset, length ?? content.Length) };
        var regions = new[] { DocumentIntelligenceModelFactory.BoundingRegion(pageNumber, [0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f]) };

        return DocumentIntelligenceModelFactory.DocumentParagraph(
            role: role,
            content: content,
            boundingRegions: regions,
            spans: spans);
    }

    private static DocumentTable CreateTable(
        int rowCount,
        int columnCount,
        IEnumerable<DocumentTableCell> cells,
        int pageNumber = 1,
        int offset = 0)
    {
        var spans = new[] { DocumentIntelligenceModelFactory.DocumentSpan(offset, 100) };
        var regions = new[] { DocumentIntelligenceModelFactory.BoundingRegion(pageNumber, [0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f]) };

        return DocumentIntelligenceModelFactory.DocumentTable(
            rowCount: rowCount,
            columnCount: columnCount,
            cells: cells,
            boundingRegions: regions,
            spans: spans);
    }

    private static DocumentTableCell CreateCell(
        int rowIndex,
        int columnIndex,
        string content,
        DocumentTableCellKind? kind = null)
    {
        return DocumentIntelligenceModelFactory.DocumentTableCell(
            kind: kind ?? DocumentTableCellKind.Content,
            rowIndex: rowIndex,
            columnIndex: columnIndex,
            rowSpan: 1,
            columnSpan: 1,
            content: content);
    }

    private static DocumentFigure CreateFigure(
        string? captionContent = null,
        int pageNumber = 1,
        int offset = 0)
    {
        var spans = new[] { DocumentIntelligenceModelFactory.DocumentSpan(offset, 50) };
        var regions = new[] { DocumentIntelligenceModelFactory.BoundingRegion(pageNumber, [0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f]) };

        DocumentCaption? caption = captionContent is not null
            ? DocumentIntelligenceModelFactory.DocumentCaption(content: captionContent, boundingRegions: regions, spans: spans)
            : null;

        return DocumentIntelligenceModelFactory.DocumentFigure(
            boundingRegions: regions,
            spans: spans,
            caption: caption);
    }

    #endregion

    [Fact]
    public void MapToDocument_WithNullResult_ShouldThrowArgumentNullException()
    {
        var act = () => DocumentIntelligenceMapper.MapToDocument(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MapToDocument_WithEmptyResult_ShouldReturnDocumentWithEmptySection()
    {
        var result = CreateAnalyzeResult();

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Should().NotBeNull();
        document.Sections.Should().HaveCount(1);
        document.Sections[0].Blocks.Should().BeEmpty();
        document.Sections[0].Heading.Should().BeNull();
    }

    [Fact]
    public void MapToDocument_WithParagraphs_ShouldMapToParagraphBlocks()
    {
        var paragraphs = new[]
        {
            CreateParagraph("First paragraph.", offset: 0),
            CreateParagraph("Second paragraph.", offset: 20)
        };
        var result = CreateAnalyzeResult(
            content: "First paragraph. Second paragraph.",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(1);
        var blocks = document.Sections[0].Blocks;
        blocks.Should().HaveCount(2);
        blocks[0].Should().BeOfType<ParagraphBlock>()
            .Which.Text.Should().Be("First paragraph.");
        blocks[1].Should().BeOfType<ParagraphBlock>()
            .Which.Text.Should().Be("Second paragraph.");
    }

    [Fact]
    public void MapToDocument_WithTitleParagraph_ShouldMapToHeadingBlock()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Document Title", role: ParagraphRole.Title, offset: 0),
            CreateParagraph("Some content.", offset: 20)
        };
        var result = CreateAnalyzeResult(
            content: "Document Title Some content.",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(1);
        var section = document.Sections[0];
        section.Heading.Should().NotBeNull();
        section.Heading!.Text.Should().Be("Document Title");
        section.Heading.Level.Should().Be(1);
        section.Blocks.Should().HaveCount(1);
        section.Blocks[0].Should().BeOfType<ParagraphBlock>()
            .Which.Text.Should().Be("Some content.");
    }

    [Fact]
    public void MapToDocument_WithSectionHeading_ShouldMapToLevel2Heading()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Section One", role: ParagraphRole.SectionHeading, offset: 0),
            CreateParagraph("Section content.", offset: 20)
        };
        var result = CreateAnalyzeResult(
            content: "Section One Section content.",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        var section = document.Sections[0];
        section.Heading.Should().NotBeNull();
        section.Heading!.Text.Should().Be("Section One");
        section.Heading.Level.Should().Be(2);
    }

    [Fact]
    public void MapToDocument_WithMultipleSections_ShouldCreateSeparateSections()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Introduction", role: ParagraphRole.SectionHeading, offset: 0),
            CreateParagraph("Intro content.", offset: 20),
            CreateParagraph("Methods", role: ParagraphRole.SectionHeading, offset: 40),
            CreateParagraph("Methods content.", offset: 55),
            CreateParagraph("Results", role: ParagraphRole.SectionHeading, offset: 75),
            CreateParagraph("Results content.", offset: 90)
        };
        var result = CreateAnalyzeResult(
            content: "Introduction Intro content. Methods Methods content. Results Results content.",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(3);
        document.Sections[0].Heading!.Text.Should().Be("Introduction");
        document.Sections[0].Blocks.Should().HaveCount(1);
        document.Sections[1].Heading!.Text.Should().Be("Methods");
        document.Sections[1].Blocks.Should().HaveCount(1);
        document.Sections[2].Heading!.Text.Should().Be("Results");
        document.Sections[2].Blocks.Should().HaveCount(1);
    }

    [Fact]
    public void MapToDocument_WithTable_ShouldMapToTableBlock()
    {
        var cells = new[]
        {
            CreateCell(0, 0, "Name", DocumentTableCellKind.ColumnHeader),
            CreateCell(0, 1, "Age", DocumentTableCellKind.ColumnHeader),
            CreateCell(1, 0, "Alice"),
            CreateCell(1, 1, "30"),
            CreateCell(2, 0, "Bob"),
            CreateCell(2, 1, "25")
        };
        var table = CreateTable(rowCount: 3, columnCount: 2, cells: cells);
        var result = CreateAnalyzeResult(
            content: "Name Age Alice 30 Bob 25",
            tables: [table]);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(1);
        var tableBlock = document.Sections[0].Blocks[0].Should().BeOfType<TableBlock>().Subject;
        tableBlock.Headers.Should().BeEquivalentTo(["Name", "Age"]);
        tableBlock.Rows.Should().HaveCount(2);
        tableBlock.Rows[0].Should().BeEquivalentTo(["Alice", "30"]);
        tableBlock.Rows[1].Should().BeEquivalentTo(["Bob", "25"]);
    }

    [Fact]
    public void MapToDocument_WithTableWithoutColumnHeaders_ShouldUseFirstRowAsHeaders()
    {
        var cells = new[]
        {
            CreateCell(0, 0, "Header1"),
            CreateCell(0, 1, "Header2"),
            CreateCell(1, 0, "Value1"),
            CreateCell(1, 1, "Value2")
        };
        var table = CreateTable(rowCount: 2, columnCount: 2, cells: cells);
        var result = CreateAnalyzeResult(
            content: "Header1 Header2 Value1 Value2",
            tables: [table]);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        var tableBlock = document.Sections[0].Blocks[0].Should().BeOfType<TableBlock>().Subject;
        tableBlock.Headers.Should().BeEquivalentTo(["Header1", "Header2"]);
        tableBlock.Rows.Should().HaveCount(1);
        tableBlock.Rows[0].Should().BeEquivalentTo(["Value1", "Value2"]);
    }

    [Fact]
    public void MapToDocument_WithFigure_ShouldMapToFigureBlock()
    {
        var figure = CreateFigure(captionContent: "Figure 1: Architecture diagram");
        var result = CreateAnalyzeResult(
            content: "Figure 1: Architecture diagram",
            figures: [figure]);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(1);
        var figureBlock = document.Sections[0].Blocks[0].Should().BeOfType<FigureBlock>().Subject;
        figureBlock.Caption.Should().Be("Figure 1: Architecture diagram");
    }

    [Fact]
    public void MapToDocument_WithFigureWithoutCaption_ShouldMapWithNullCaption()
    {
        var figure = CreateFigure(captionContent: null);
        var result = CreateAnalyzeResult(
            content: "",
            figures: [figure]);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        var figureBlock = document.Sections[0].Blocks[0].Should().BeOfType<FigureBlock>().Subject;
        figureBlock.Caption.Should().BeNull();
    }

    [Fact]
    public void MapToDocument_ShouldPreservePageNumbers()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Page 1 content.", pageNumber: 1, offset: 0),
            CreateParagraph("Page 2 content.", pageNumber: 2, offset: 20)
        };
        var result = CreateAnalyzeResult(
            content: "Page 1 content. Page 2 content.",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        var blocks = document.Sections[0].Blocks;
        blocks[0].Source!.PageNumber.Should().Be(1);
        blocks[1].Source!.PageNumber.Should().Be(2);
    }

    [Fact]
    public void MapToDocument_ShouldPreserveSpanReferences()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Hello world.", offset: 0, length: 12),
            CreateParagraph("Second line.", offset: 13, length: 12)
        };
        var result = CreateAnalyzeResult(
            content: "Hello world. Second line.",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        var blocks = document.Sections[0].Blocks;
        blocks[0].Source!.Span.Should().NotBeNull();
        blocks[0].Source!.Span!.Offset.Should().Be(0);
        blocks[0].Source!.Span!.Length.Should().Be(12);
        blocks[1].Source!.Span!.Offset.Should().Be(13);
        blocks[1].Source!.Span!.Length.Should().Be(12);
    }

    [Fact]
    public void MapToDocument_WithFilePath_ShouldSetSourceReference()
    {
        var result = CreateAnalyzeResult(content: "Some text.");

        var document = DocumentIntelligenceMapper.MapToDocument(result, "report.pdf");

        document.Source.Should().NotBeNull();
        document.Source!.FilePath.Should().Be("report.pdf");
    }

    [Fact]
    public void MapToDocument_WithoutFilePath_ShouldHaveNullSource()
    {
        var result = CreateAnalyzeResult(content: "Some text.");

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Source.Should().BeNull();
    }

    [Fact]
    public void MapToDocument_ShouldSetMetadata()
    {
        var pages = new[]
        {
            DocumentIntelligenceModelFactory.DocumentPage(pageNumber: 1, width: 8.5f, height: 11f, unit: LengthUnit.Inch),
            DocumentIntelligenceModelFactory.DocumentPage(pageNumber: 2, width: 8.5f, height: 11f, unit: LengthUnit.Inch)
        };
        var result = CreateAnalyzeResult(
            content: "Hello world on multiple pages.",
            pages: pages);

        var document = DocumentIntelligenceMapper.MapToDocument(result, "document.pdf");

        document.Metadata.Should().NotBeNull();
        document.Metadata.PageCount.Should().Be(2);
        document.Metadata.SourceFormat.Should().Be("PDF");
        document.Metadata.WordCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MapToDocument_ShouldSkipPageHeadersAndFooters()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Page Header", role: ParagraphRole.PageHeader, offset: 0),
            CreateParagraph("Real content.", offset: 20),
            CreateParagraph("Page Footer", role: ParagraphRole.PageFooter, offset: 40),
            CreateParagraph("Page 1", role: ParagraphRole.PageNumber, offset: 60)
        };
        var result = CreateAnalyzeResult(
            content: "Page Header Real content. Page Footer Page 1",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(1);
        document.Sections[0].Blocks.Should().HaveCount(1);
        document.Sections[0].Blocks[0].Should().BeOfType<ParagraphBlock>()
            .Which.Text.Should().Be("Real content.");
    }

    [Fact]
    public void MapToDocument_ContentBeforeFirstHeading_ShouldBeInFirstSection()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Preface content.", offset: 0),
            CreateParagraph("Chapter 1", role: ParagraphRole.SectionHeading, offset: 20),
            CreateParagraph("Chapter content.", offset: 35)
        };
        var result = CreateAnalyzeResult(
            content: "Preface content. Chapter 1 Chapter content.",
            paragraphs: paragraphs);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(2);
        // First section has no heading, just the preface content
        document.Sections[0].Heading.Should().BeNull();
        document.Sections[0].Blocks.Should().HaveCount(1);
        document.Sections[0].Blocks[0].Should().BeOfType<ParagraphBlock>()
            .Which.Text.Should().Be("Preface content.");
        // Second section has the heading and content
        document.Sections[1].Heading!.Text.Should().Be("Chapter 1");
        document.Sections[1].Blocks.Should().HaveCount(1);
    }

    [Fact]
    public void MapToDocument_WithMixedContent_ShouldPreserveOrder()
    {
        var paragraphs = new[]
        {
            CreateParagraph("Intro text.", offset: 0),
            CreateParagraph("After table text.", offset: 200)
        };
        var table = CreateTable(
            rowCount: 2, columnCount: 1,
            cells: [CreateCell(0, 0, "H1"), CreateCell(1, 0, "V1")],
            offset: 100);
        var result = CreateAnalyzeResult(
            content: "Intro text. [table] After table text.",
            paragraphs: paragraphs,
            tables: [table]);

        var document = DocumentIntelligenceMapper.MapToDocument(result);

        document.Sections.Should().HaveCount(1);
        var blocks = document.Sections[0].Blocks;
        blocks.Should().HaveCount(3);
        blocks[0].Should().BeOfType<ParagraphBlock>();
        blocks[1].Should().BeOfType<TableBlock>();
        blocks[2].Should().BeOfType<ParagraphBlock>();
    }
}
