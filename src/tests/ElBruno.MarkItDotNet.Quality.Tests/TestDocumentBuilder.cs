// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Quality.Tests;

/// <summary>
/// Helper for building test <see cref="Document"/> instances with various quality characteristics.
/// </summary>
internal static class TestDocumentBuilder
{
    /// <summary>
    /// Builds a clean, well-structured document with good quality signals.
    /// </summary>
    public static Document CreateCleanDocument()
    {
        return new Document
        {
            Id = "clean-doc",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Introduction", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p1",
                            Text = "This is a well-written paragraph with more than ten words to demonstrate quality text content in the document."
                        },
                        new ParagraphBlock
                        {
                            Id = "p2",
                            Text = "Another paragraph that contains meaningful content and is long enough to pass the text density threshold easily."
                        },
                    ],
                    SubSections =
                    [
                        new DocumentSection
                        {
                            Id = "s1-1",
                            Heading = new HeadingBlock { Id = "h2", Text = "Background", Level = 2 },
                            Blocks =
                            [
                                new ParagraphBlock
                                {
                                    Id = "p3",
                                    Text = "This section provides background information that is relevant to understanding the rest of the document clearly."
                                },
                            ],
                        },
                    ],
                },
                new DocumentSection
                {
                    Id = "s2",
                    Heading = new HeadingBlock { Id = "h3", Text = "Conclusion", Level = 2 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p4",
                            Text = "In conclusion the document is well formed and should receive a high quality score from the analyzer."
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Builds a document with garbled OCR-like text.
    /// </summary>
    public static Document CreateGarbledOcrDocument()
    {
        return new Document
        {
            Id = "ocr-doc",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "S3ct10n", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p1",
                            Text = "Th#$ !s g@rbl3d t3xt fr#m b@d #CR pr#c3ss!ng w!th m@ny sp3c!@l ch@rs",
                        },
                        new ParagraphBlock
                        {
                            Id = "p2",
                            Text = "xkcd brrr zzzz qqqq ntrk pldj mwrx kkkk nnnnn bbbbb ccccc",
                        },
                        new ParagraphBlock
                        {
                            Id = "p3",
                            Text = "@@## $$%% ^^&& **!! ~~`` ||// \\\\-- ++== <<>> @@## $$%%",
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Builds a document with extensive duplicate paragraphs.
    /// </summary>
    public static Document CreateDuplicateContentDocument()
    {
        const string repeatedText = "This paragraph is duplicated multiple times across the document which indicates a quality issue.";
        return new Document
        {
            Id = "dup-doc",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Section One", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p1", Text = repeatedText },
                        new ParagraphBlock { Id = "p2", Text = repeatedText },
                        new ParagraphBlock { Id = "p3", Text = repeatedText },
                        new ParagraphBlock
                        {
                            Id = "p4",
                            Text = "This is a unique paragraph that only appears once in the entire document with enough words."
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Builds a document where most paragraphs are empty or whitespace.
    /// </summary>
    public static Document CreateEmptyBlocksDocument()
    {
        return new Document
        {
            Id = "empty-doc",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Empty Section", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock { Id = "p1", Text = "" },
                        new ParagraphBlock { Id = "p2", Text = "   " },
                        new ParagraphBlock { Id = "p3", Text = "\t\n" },
                        new ParagraphBlock
                        {
                            Id = "p4",
                            Text = "Only this paragraph has real content with enough words to be considered text rich in the analysis."
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Builds a document with broken heading order (H1 → H4 without H2/H3).
    /// </summary>
    public static Document CreateBrokenHeadingOrderDocument()
    {
        return new Document
        {
            Id = "heading-doc",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Top Level", Level = 1 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p1",
                            Text = "Content for the top level section with enough words for text density metrics to work well."
                        },
                    ],
                    SubSections =
                    [
                        new DocumentSection
                        {
                            Id = "s1-1",
                            Heading = new HeadingBlock { Id = "h2", Text = "Skipped To Level Four", Level = 4 },
                            Blocks =
                            [
                                new ParagraphBlock
                                {
                                    Id = "p2",
                                    Text = "This section skipped heading levels two and three which is a reading order violation."
                                },
                            ],
                            SubSections =
                            [
                                new DocumentSection
                                {
                                    Id = "s1-1-1",
                                    Heading = new HeadingBlock { Id = "h3", Text = "Back to Level Six", Level = 6 },
                                    Blocks =
                                    [
                                        new ParagraphBlock
                                        {
                                            Id = "p3",
                                            Text = "Another level skip here to demonstrate multiple reading order violations within the same document."
                                        },
                                    ],
                                },
                            ],
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Builds a document with inconsistent heading capitalization.
    /// </summary>
    public static Document CreateInconsistentHeadingsDocument()
    {
        return new Document
        {
            Id = "inconsistent-heading-doc",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Title Case Heading", Level = 2 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p1",
                            Text = "Content for section one with enough words to pass text density checks in the analyzer metrics."
                        },
                    ],
                },
                new DocumentSection
                {
                    Id = "s2",
                    Heading = new HeadingBlock { Id = "h2", Text = "another title case heading", Level = 2 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p2",
                            Text = "Content for section two with enough words to pass text density checks in the analyzer metrics."
                        },
                    ],
                },
                new DocumentSection
                {
                    Id = "s3",
                    Heading = new HeadingBlock { Id = "h3", Text = "ALL CAPS HEADING", Level = 2 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = "p3",
                            Text = "Content for section three with enough words to pass text density checks in the analyzer metrics."
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Builds a document with tables that have quality issues.
    /// </summary>
    public static Document CreateProblematicTablesDocument()
    {
        return new Document
        {
            Id = "table-doc",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Tables Section", Level = 1 },
                    Blocks =
                    [
                        new TableBlock
                        {
                            Id = "t1",
                            Headers = [""],
                            Rows = [[""], [""]],
                        },
                        new TableBlock
                        {
                            Id = "t2",
                            Headers = ["Col1", "Col2"],
                            Rows = [["a"], ["b", "c", "d"]],
                        },
                        new ParagraphBlock
                        {
                            Id = "p1",
                            Text = "This paragraph provides context around the tables and has enough words for the density calculation."
                        },
                    ],
                },
            ],
        };
    }

    /// <summary>
    /// Builds a minimal empty document with no sections.
    /// </summary>
    public static Document CreateEmptyDocument()
    {
        return new Document { Id = "empty" };
    }
}
