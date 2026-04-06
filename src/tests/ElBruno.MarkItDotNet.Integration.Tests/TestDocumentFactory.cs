// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// Shared helper for creating test documents used across integration tests.
/// </summary>
public static class TestDocumentFactory
{
    /// <summary>
    /// Creates a realistic multi-section report with headings, paragraphs, a table,
    /// a figure, and source references with page numbers.
    /// </summary>
    public static Document CreateRealisticReport()
    {
        return new Document
        {
            Id = DocumentIdGenerator.ForDocument("reports/annual-report.pdf", "Annual Performance Report"),
            Source = new SourceReference
            {
                FilePath = "reports/annual-report.pdf",
                HeadingPath = null,
            },
            Metadata = new DocumentMetadata
            {
                Title = "Annual Performance Report",
                Author = "Jane Smith",
                SourceFormat = "pdf",
                CreatedAt = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero),
                ModifiedAt = new DateTimeOffset(2024, 6, 30, 0, 0, 0, TimeSpan.Zero),
                PageCount = 12,
                WordCount = 3500,
            },
            Sections =
            [
                new DocumentSection
                {
                    Heading = new HeadingBlock
                    {
                        Id = DocumentIdGenerator.ForBlock("heading", "1. Executive Summary", 0),
                        Text = "1. Executive Summary",
                        Level = 1,
                        Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 1 },
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "This report presents the annual performance...", 1),
                            Text = "This report presents the annual performance metrics for the fiscal year ending December 2024. The organization achieved significant growth across all key performance indicators, with revenue increasing by 23% year-over-year.",
                            Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 1 },
                        },
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "Key highlights include...", 2),
                            Text = "Key highlights include the successful launch of three new product lines, expansion into two international markets, and a 15% improvement in customer satisfaction scores compared to the previous reporting period.",
                            Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 1 },
                        },
                    ],
                    SubSections = [],
                },
                new DocumentSection
                {
                    Heading = new HeadingBlock
                    {
                        Id = DocumentIdGenerator.ForBlock("heading", "2. Financial Overview", 3),
                        Text = "2. Financial Overview",
                        Level = 1,
                        Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 3 },
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "The financial results...", 4),
                            Text = "The financial results for the year demonstrate robust growth and improved operational efficiency. Total revenue reached $4.2 billion, representing a 23% increase from the previous fiscal year.",
                            Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 3 },
                        },
                        new TableBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("table", "Revenue|Growth|Margin", 5),
                            Headers = ["Metric", "Q1", "Q2", "Q3", "Q4"],
                            Rows =
                            [
                                (IReadOnlyList<string>)["Revenue ($M)", "950", "1,050", "1,100", "1,100"],
                                ["Growth (%)", "18", "22", "25", "27"],
                                ["Margin (%)", "32", "34", "35", "36"],
                            ],
                            Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 4 },
                        },
                    ],
                    SubSections =
                    [
                        new DocumentSection
                        {
                            Heading = new HeadingBlock
                            {
                                Id = DocumentIdGenerator.ForBlock("heading", "2.1 Revenue Breakdown", 6),
                                Text = "2.1 Revenue Breakdown",
                                Level = 2,
                                Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 5 },
                            },
                            Blocks =
                            [
                                new ParagraphBlock
                                {
                                    Id = DocumentIdGenerator.ForBlock("paragraph", "Revenue was driven primarily...", 7),
                                    Text = "Revenue was driven primarily by the enterprise software division, which contributed 60% of total revenue. The cloud services segment grew 45% year-over-year, becoming the fastest-growing business unit.",
                                    Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 5 },
                                },
                            ],
                            SubSections = [],
                        },
                    ],
                },
                new DocumentSection
                {
                    Heading = new HeadingBlock
                    {
                        Id = DocumentIdGenerator.ForBlock("heading", "3. Strategic Initiatives", 8),
                        Text = "3. Strategic Initiatives",
                        Level = 1,
                        Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 7 },
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "Several strategic initiatives...", 9),
                            Text = "Several strategic initiatives were launched during the fiscal year to position the organization for continued growth. These initiatives span technology modernization, talent acquisition, and market expansion.",
                            Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 7 },
                        },
                        new FigureBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("figure", "strategic-roadmap", 10),
                            AltText = "Strategic Roadmap 2024-2026",
                            Caption = "Figure 1: Strategic Roadmap for 2024-2026",
                            ImagePath = "images/strategic-roadmap.png",
                            Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 8 },
                        },
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "The technology modernization...", 11),
                            Text = "The technology modernization program focuses on migrating legacy systems to cloud-native architectures, implementing AI-driven analytics across all business functions, and establishing a robust data governance framework.",
                            Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 8 },
                        },
                    ],
                    SubSections = [],
                },
            ],
        };
    }

    /// <summary>
    /// Creates a minimal document for quick tests.
    /// </summary>
    public static Document CreateSimpleDocument()
    {
        return new Document
        {
            Id = DocumentIdGenerator.ForDocument("notes/simple.md", "Quick Note"),
            Source = new SourceReference { FilePath = "notes/simple.md" },
            Metadata = new DocumentMetadata
            {
                Title = "Quick Note",
                Author = "Test Author",
                SourceFormat = "markdown",
            },
            Sections =
            [
                new DocumentSection
                {
                    Heading = new HeadingBlock
                    {
                        Id = DocumentIdGenerator.ForBlock("heading", "Introduction", 0),
                        Text = "Introduction",
                        Level = 1,
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "This is a simple document for testing purposes.", 1),
                            Text = "This is a simple document for testing purposes. It contains just enough content to validate basic pipeline operations without added complexity.",
                        },
                    ],
                    SubSections = [],
                },
            ],
        };
    }

    /// <summary>
    /// Creates a low-quality document with garbled text, empty blocks, and broken headings.
    /// </summary>
    public static Document CreateLowQualityDocument()
    {
        return new Document
        {
            Id = DocumentIdGenerator.ForDocument("scans/garbled-scan.pdf", "Garbled Scan"),
            Source = new SourceReference { FilePath = "scans/garbled-scan.pdf" },
            Metadata = new DocumentMetadata
            {
                Title = "Garbled Scan",
                SourceFormat = "pdf",
                PageCount = 3,
            },
            Sections =
            [
                new DocumentSection
                {
                    Heading = new HeadingBlock
                    {
                        Id = DocumentIdGenerator.ForBlock("heading", "Xr#$T1ng", 0),
                        Text = "Xr#$T1ng",
                        Level = 1,
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "", 1),
                            Text = "",
                        },
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "Th!$ t3xt h@s b33n b@dly r3c0gn!z3d", 2),
                            Text = "Th!$ t3xt h@s b33n b@dly r3c0gn!z3d by th3 0CR 3ng!n3 @nd c0nt@!ns m@ny #rt!f@ct$ th@t m@k3 !t unr3@d@bl3.",
                        },
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "   ", 3),
                            Text = "   ",
                        },
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "xyzzy plugh", 4),
                            Text = "xyzzy plugh brrrt kkkk nnnn zzzz qqqq wwww yyyy rrrr tttt pppp ssss ffff ggggg hhhh jjjj llll cccc vvvv bbbb mmmm",
                        },
                    ],
                    SubSections = [],
                },
                new DocumentSection
                {
                    Heading = new HeadingBlock
                    {
                        Id = DocumentIdGenerator.ForBlock("heading", "", 5),
                        Text = "",
                        Level = 4,
                    },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = DocumentIdGenerator.ForBlock("paragraph", "W#$%^ @&*!", 6),
                            Text = "W#$%^ @&*! P$$%^& Q!@#$ R%^&* S!@#$% T&*() U!@#$% V^&*() more garbled text with s!gn!f!c@nt @rt!f@cts present.",
                        },
                    ],
                    SubSections = [],
                },
            ],
        };
    }
}
