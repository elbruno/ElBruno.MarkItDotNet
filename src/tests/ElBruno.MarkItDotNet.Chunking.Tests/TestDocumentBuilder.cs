// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

#pragma warning disable SA1118 // ParameterMustNotSpanMultipleLines

namespace ElBruno.MarkItDotNet.Chunking.Tests;

/// <summary>
/// Helper class to create sample <see cref="Document"/> instances for testing chunking strategies.
/// </summary>
internal static class TestDocumentBuilder
{
    /// <summary>
    /// Creates a simple document with 3 sections, each containing a heading and a few paragraphs.
    /// </summary>
    public static Document CreateSimpleDocument(string id = "doc-1")
    {
        return new Document
        {
            Id = id,
            Source = new SourceReference { FilePath = "test-source" },
            Metadata = new DocumentMetadata { Title = "Simple Test Document" },
            Sections =
            [
                new DocumentSection
                {
                    Id = "section-1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Introduction", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p1", Text = "This is the first paragraph of the introduction." },
                        new ParagraphBlock { Id = "p2", Text = "This is the second paragraph with more details." },
                    ],
                },
                new DocumentSection
                {
                    Id = "section-2",
                    Heading = new HeadingBlock { Id = "h2", Text = "Main Content", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p3", Text = "The main content discusses important topics." },
                        new ParagraphBlock { Id = "p4", Text = "Additional information is provided here." },
                        new ParagraphBlock { Id = "p5", Text = "This paragraph concludes the main section." },
                    ],
                },
                new DocumentSection
                {
                    Id = "section-3",
                    Heading = new HeadingBlock { Id = "h3", Text = "Conclusion", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p6", Text = "In conclusion, the document covers all key points." },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Creates a document that includes tables.
    /// </summary>
    public static Document CreateDocumentWithTables(string id = "doc-tables")
    {
        return new Document
        {
            Id = id,
            Source = new SourceReference { FilePath = "test-source" },
            Metadata = new DocumentMetadata { Title = "Document With Tables" },
            Sections =
            [
                new DocumentSection
                {
                    Id = "section-1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Data Section", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p1", Text = "The following table shows quarterly results." },
                        new TableBlock
                        {
                            Id = "t1",
                            Headers = ["Quarter", "Revenue", "Profit"],
                            Rows =
                            [
                                ["Q1", "$100M", "$20M"],
                                ["Q2", "$120M", "$25M"],
                                ["Q3", "$110M", "$22M"],
                                ["Q4", "$130M", "$30M"],
                            ],
                        },
                        new ParagraphBlock { Id = "p2", Text = "As shown above, Q4 was the strongest quarter." },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Creates a document with nested subsections.
    /// </summary>
    public static Document CreateDocumentWithNestedSections(string id = "doc-nested")
    {
        return new Document
        {
            Id = id,
            Source = new SourceReference { FilePath = "test-source" },
            Metadata = new DocumentMetadata { Title = "Nested Document" },
            Sections =
            [
                new DocumentSection
                {
                    Id = "section-1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Chapter 1", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p1", Text = "Chapter 1 introduction." },
                    ],
                    SubSections =
                    [
                        new DocumentSection
                        {
                            Id = "section-1-1",
                            Heading = new HeadingBlock { Id = "h1-1", Text = "Section 1.1", Level = 2 },
                            Blocks =
                            [
                                new ParagraphBlock { Id = "p2", Text = "Content of section 1.1." },
                            ],
                            SubSections =
                            [
                                new DocumentSection
                                {
                                    Id = "section-1-1-1",
                                    Heading = new HeadingBlock { Id = "h1-1-1", Text = "Subsection 1.1.1", Level = 3 },
                                    Blocks =
                                    [
                                        new ParagraphBlock { Id = "p3", Text = "Deeply nested content." },
                                    ],
                                },
                            ],
                        },
                        new DocumentSection
                        {
                            Id = "section-1-2",
                            Heading = new HeadingBlock { Id = "h1-2", Text = "Section 1.2", Level = 2 },
                            Blocks =
                            [
                                new ParagraphBlock { Id = "p4", Text = "Content of section 1.2." },
                            ],
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Creates a document with many paragraphs to test token limits and large content chunking.
    /// </summary>
    public static Document CreateLargeDocument(string id = "doc-large", int paragraphCount = 50)
    {
        var blocks = new List<DocumentBlock>();
        for (var i = 0; i < paragraphCount; i++)
        {
            blocks.Add(new ParagraphBlock
            {
                Id = $"p{i}",
                Text = $"This is paragraph number {i}. It contains some text that helps test token-aware chunking strategies with realistic content lengths and word counts that simulate actual documents.",
            });
        }

        return new Document
        {
            Id = id,
            Source = new SourceReference { FilePath = "test-source" },
            Metadata = new DocumentMetadata { Title = "Large Document" },
            Sections =
            [
                new DocumentSection
                {
                    Id = "section-1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Large Section", Level = 1 },
                    Blocks = blocks,
                },
            ],
        };
    }

    /// <summary>
    /// Creates a document with a figure block.
    /// </summary>
    public static Document CreateDocumentWithFigures(string id = "doc-figures")
    {
        return new Document
        {
            Id = id,
            Source = new SourceReference { FilePath = "test-source" },
            Metadata = new DocumentMetadata { Title = "Document With Figures" },
            Sections =
            [
                new DocumentSection
                {
                    Id = "section-1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Visual Section", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p1", Text = "Below is an important diagram." },
                        new FigureBlock { Id = "f1", AltText = "Architecture diagram", Caption = "System Architecture", ImagePath = "/images/arch.png" },
                        new ParagraphBlock { Id = "p2", Text = "The diagram shows the system components." },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Creates a document with an empty section (no blocks, no heading).
    /// </summary>
    public static Document CreateDocumentWithEmptySection(string id = "doc-empty")
    {
        return new Document
        {
            Id = id,
            Source = new SourceReference { FilePath = "test-source" },
            Metadata = new DocumentMetadata { Title = "Empty Section Document" },
            Sections =
            [
                new DocumentSection
                {
                    Id = "section-1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Non-Empty", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p1", Text = "This section has content." },
                    ],
                },
                new DocumentSection
                {
                    Id = "section-2",
                    Blocks = [],
                },
            ],
        };
    }
}
