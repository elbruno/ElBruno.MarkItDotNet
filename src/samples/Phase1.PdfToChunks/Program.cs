// =============================================================================
// Phase 1 E2E Sample: PDF → Structured Document → Chunks → Citations
// =============================================================================
// This sample demonstrates the end-to-end ingestion pipeline without requiring
// an actual PDF file or Azure credentials. It programmatically creates a Document
// that simulates what a converter (e.g., DocumentIntelligenceConverter) would produce,
// then chunks it and generates citations for each chunk.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;

// Step 1: Create a sample document (simulating what a converter would produce)
Console.WriteLine("=== Phase 1: PDF to Chunks Pipeline Demo ===\n");

var document = new Document
{
    Id = Guid.NewGuid().ToString(),
    Metadata = new DocumentMetadata
    {
        Title = "Quarterly Report Q4 2024",
        Author = "Finance Department",
        SourceFormat = "PDF",
        PageCount = 3,
        WordCount = 250
    },
    Source = new SourceReference { FilePath = "quarterly-report-q4-2024.pdf" },
    Sections =
    [
        new DocumentSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "Executive Summary", Level = 1 },
            Blocks =
            [
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "This report presents the financial results for Q4 2024. Revenue grew by 15% year-over-year, driven by strong performance in the cloud services division.",
                    Source = new SourceReference { FilePath = "quarterly-report-q4-2024.pdf", PageNumber = 1 }
                }
            ]
        },
        new DocumentSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "Revenue Breakdown", Level = 2 },
            Blocks =
            [
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "Cloud services contributed $45M in revenue, representing 60% of total revenue. On-premises solutions generated $30M.",
                    Source = new SourceReference { FilePath = "quarterly-report-q4-2024.pdf", PageNumber = 1 }
                },
                new TableBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Headers = ["Division", "Revenue ($M)", "YoY Growth"],
                    Rows =
                    [
                        (IReadOnlyList<string>)["Cloud Services", "45", "+22%"],
                        ["On-Premises", "30", "+5%"]
                    ],
                    Source = new SourceReference { FilePath = "quarterly-report-q4-2024.pdf", PageNumber = 2 }
                }
            ]
        },
        new DocumentSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "Outlook", Level = 2 },
            Blocks =
            [
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "We expect continued growth in Q1 2025, with cloud services projected to exceed $50M. Strategic investments in AI-powered features will drive adoption.",
                    Source = new SourceReference { FilePath = "quarterly-report-q4-2024.pdf", PageNumber = 3 }
                }
            ]
        }
    ]
};

Console.WriteLine($"Document: {document.Metadata.Title}");
Console.WriteLine($"  Source: {document.Source?.FilePath}");
Console.WriteLine($"  Pages: {document.Metadata.PageCount}");
Console.WriteLine($"  Sections: {document.Sections.Count}");
Console.WriteLine();

// Step 2: Chunk the document using heading-based chunking
Console.WriteLine("--- Chunking ---");
var chunker = new HeadingBasedChunker();
var chunks = chunker.Chunk(document);

foreach (var chunk in chunks)
{
    var preview = chunk.Content.Length > 80
        ? chunk.Content[..80] + "..."
        : chunk.Content;
    Console.WriteLine($"Chunk {chunk.Index}: {preview}");
    Console.WriteLine($"  Heading: {chunk.HeadingPath}");
    Console.WriteLine($"  Sources: {chunk.Sources.Count}");
    Console.WriteLine();
}

// Step 3: Generate citations for each chunk
Console.WriteLine("--- Citations ---");
var citations = CitationBuilder.FromDocument(document);
var index = 0;
foreach (var citation in citations)
{
    Console.WriteLine($"[{index++}] {CitationFormatter.Format(citation)}");
}
Console.WriteLine();

// Step 4: Serialize the document to JSON
Console.WriteLine("--- Document JSON ---");
var json = DocumentSerializer.Serialize(document);
Console.WriteLine(json);
