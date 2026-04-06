// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Metadata.Tests;

public class DocumentMetadataExtractorTests
{
    private readonly DocumentMetadataExtractor _extractor = new();

    #region Title extraction

    [Fact]
    public void Extract_UsesMetadataTitleWhenAvailable()
    {
        var doc = CreateDocument(
            metadata: new DocumentMetadata { Title = "  My Title  " },
            headings: [("H1 Heading", 1)]);

        var result = _extractor.Extract(doc);

        result.Title.Should().Be("My Title");
    }

    [Fact]
    public void Extract_FallsBackToFirstH1WhenNoMetadataTitle()
    {
        var doc = CreateDocument(
            headings: [("  First H1  Heading  ", 1), ("Second H1", 1)]);

        var result = _extractor.Extract(doc);

        result.Title.Should().Be("First H1 Heading");
    }

    [Fact]
    public void Extract_FallsBackToFilenameWhenNoH1()
    {
        var doc = CreateDocument(
            source: new SourceReference { FilePath = "/docs/my-report.pdf" },
            headings: [("Sub Heading", 2)]);

        var result = _extractor.Extract(doc);

        result.Title.Should().Be("my-report");
    }

    [Fact]
    public void Extract_ReturnsNullTitleWhenNoFallbackAvailable()
    {
        var doc = CreateDocument();

        var result = _extractor.Extract(doc);

        result.Title.Should().BeNull();
    }

    #endregion

    #region Author extraction

    [Fact]
    public void Extract_ExtractsAuthorFromMetadata()
    {
        var doc = CreateDocument(
            metadata: new DocumentMetadata { Author = "Bruno Capuano" });

        var result = _extractor.Extract(doc);

        result.Author.Should().Be("Bruno Capuano");
    }

    [Fact]
    public void Extract_AuthorIsNullWhenNotInMetadata()
    {
        var doc = CreateDocument();

        var result = _extractor.Extract(doc);

        result.Author.Should().BeNull();
    }

    #endregion

    #region Language detection

    [Fact]
    public void Extract_DetectsEnglishLanguage()
    {
        var doc = CreateDocument(
            paragraphs: ["The quick brown fox jumps over the lazy dog. This is a test of the English language detection system that should work well with common words."]);

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("en");
    }

    [Fact]
    public void Extract_DetectsSpanishLanguage()
    {
        var doc = CreateDocument(
            paragraphs: ["El perro marrón rápido salta sobre el perro perezoso. Esta es una prueba del sistema de detección del idioma español que debería funcionar bien con las palabras comunes."]);

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("es");
    }

    [Fact]
    public void Extract_LanguageIsNullForEmptyContent()
    {
        var doc = CreateDocument();

        var result = _extractor.Extract(doc);

        result.Language.Should().BeNull();
    }

    #endregion

    #region Document type inference

    [Fact]
    public void Extract_InfersPresentationFromFormat()
    {
        var doc = CreateDocument(
            metadata: new DocumentMetadata { SourceFormat = "pptx" });

        var result = _extractor.Extract(doc);

        result.DocumentType.Should().Be(DocumentType.Presentation);
    }

    [Fact]
    public void Extract_InfersSpreadsheetFromFormat()
    {
        var doc = CreateDocument(
            metadata: new DocumentMetadata { SourceFormat = "xlsx" });

        var result = _extractor.Extract(doc);

        result.DocumentType.Should().Be(DocumentType.Spreadsheet);
    }

    [Fact]
    public void Extract_InfersLegalFromHeadings()
    {
        var doc = CreateDocument(
            headings: [("Terms and Conditions", 1), ("Agreement Clause 1", 2), ("Agreement Clause 2", 2),
                       ("Obligations", 2), ("Termination", 2), ("Dispute Resolution", 2)]);

        var result = _extractor.Extract(doc);

        result.DocumentType.Should().Be(DocumentType.Legal);
    }

    [Fact]
    public void Extract_InfersManualFromHeadings()
    {
        var doc = CreateDocument(
            headings: [("Getting Started", 1), ("Installation", 2), ("Configuration", 2),
                       ("Troubleshooting", 2), ("FAQ", 2), ("Contact", 2)]);

        var result = _extractor.Extract(doc);

        result.DocumentType.Should().Be(DocumentType.Manual);
    }

    [Fact]
    public void Extract_InfersArticleForSimpleDocuments()
    {
        var doc = CreateDocument(
            headings: [("Introduction", 1)],
            paragraphs: ["This is an article about something interesting."]);

        var result = _extractor.Extract(doc);

        result.DocumentType.Should().Be(DocumentType.Article);
    }

    #endregion

    #region Word count

    [Fact]
    public void Extract_CountsWordsAcrossParagraphs()
    {
        var doc = CreateDocument(
            paragraphs: ["Hello world", "This is a test"]);

        var result = _extractor.Extract(doc);

        result.WordCount.Should().Be(6);
    }

    [Fact]
    public void Extract_WordCountIsZeroForEmptyDocument()
    {
        var doc = CreateDocument();

        var result = _extractor.Extract(doc);

        result.WordCount.Should().Be(0);
    }

    #endregion

    #region Heading normalization

    [Fact]
    public void Extract_NormalizesHeadingWhitespace()
    {
        var doc = CreateDocument(
            headings: [("  Hello   World  ", 1), ("  Sub  Section  ", 2)]);

        var result = _extractor.Extract(doc);

        result.NormalizedHeadings.Should().HaveCount(2);
        result.NormalizedHeadings[0].Text.Should().Be("Hello World");
        result.NormalizedHeadings[0].Level.Should().Be(1);
        result.NormalizedHeadings[0].OriginalText.Should().Be("  Hello   World  ");
        result.NormalizedHeadings[1].Text.Should().Be("Sub Section");
        result.NormalizedHeadings[1].Level.Should().Be(2);
    }

    [Fact]
    public void Extract_NormalizedHeadingsHaveSequentialIds()
    {
        var doc = CreateDocument(
            headings: [("A", 1), ("B", 2), ("C", 3)]);

        var result = _extractor.Extract(doc);

        result.NormalizedHeadings.Select(h => h.Id)
            .Should().BeEquivalentTo(["heading-0", "heading-1", "heading-2"]);
    }

    #endregion

    #region Section count and metadata propagation

    [Fact]
    public void Extract_CountsSections()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection { Heading = new HeadingBlock { Text = "A", Level = 1 } },
                new DocumentSection { Heading = new HeadingBlock { Text = "B", Level = 1 } },
            ],
        };

        var result = _extractor.Extract(doc);

        result.SectionCount.Should().Be(2);
    }

    [Fact]
    public void Extract_PropagatesCreatedAtAndModifiedAt()
    {
        var created = DateTimeOffset.UtcNow.AddDays(-7);
        var modified = DateTimeOffset.UtcNow;
        var doc = CreateDocument(
            metadata: new DocumentMetadata { CreatedAt = created, ModifiedAt = modified });

        var result = _extractor.Extract(doc);

        result.CreatedAt.Should().Be(created);
        result.ModifiedAt.Should().Be(modified);
    }

    [Fact]
    public void Extract_PropagatesPageCount()
    {
        var doc = CreateDocument(
            metadata: new DocumentMetadata { PageCount = 42 });

        var result = _extractor.Extract(doc);

        result.PageCount.Should().Be(42);
    }

    [Fact]
    public void Extract_PropagatesCustomMetadata()
    {
        var doc = CreateDocument(
            metadata: new DocumentMetadata
            {
                Custom = new Dictionary<string, object> { ["key1"] = "value1" },
            });

        var result = _extractor.Extract(doc);

        result.Custom.Should().ContainKey("key1");
        result.Custom["key1"].Should().Be("value1");
    }

    #endregion

    #region Helpers

    private static Document CreateDocument(
        DocumentMetadata? metadata = null,
        SourceReference? source = null,
        (string Text, int Level)[]? headings = null,
        string[]? paragraphs = null)
    {
        var blocks = new List<DocumentBlock>();

        if (headings is not null)
        {
            blocks.AddRange(headings.Select(h => new HeadingBlock { Text = h.Text, Level = h.Level }));
        }

        if (paragraphs is not null)
        {
            blocks.AddRange(paragraphs.Select(p => new ParagraphBlock { Text = p }));
        }

        // Build sections: first heading becomes the section heading, rest are blocks
        var sections = new List<DocumentSection>();
        if (blocks.Count > 0)
        {
            var sectionHeading = blocks.OfType<HeadingBlock>().FirstOrDefault();
            var sectionBlocks = blocks.Where(b => b != sectionHeading).ToList();
            sections.Add(new DocumentSection
            {
                Heading = sectionHeading,
                Blocks = sectionBlocks,
            });
        }

        return new Document
        {
            Metadata = metadata ?? new DocumentMetadata(),
            Source = source,
            Sections = sections,
        };
    }

    #endregion
}
