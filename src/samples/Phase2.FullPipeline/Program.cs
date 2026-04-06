// =============================================================================
// Phase 2 E2E Sample: Full Production-Readiness Pipeline
// =============================================================================
// Demonstrates every stage of the ingestion pipeline: document creation,
// quality analysis, metadata extraction, chunking, citations, vector-record
// mapping, sync planning, and serialisation — all without external dependencies.

using System.Text;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.Metadata;
using ElBruno.MarkItDotNet.Quality;
using ElBruno.MarkItDotNet.Sync;
using ElBruno.MarkItDotNet.VectorData;

Console.WriteLine("=== Phase 2: Full Ingestion Pipeline Demo ===");
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 1: Create Document
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 1: Create Document ---");

var document = new Document
{
    Id = Guid.NewGuid().ToString(),
    Metadata = new DocumentMetadata
    {
        Title = "Annual Technology Report 2024",
        Author = "Innovation Lab",
        SourceFormat = "PDF",
        PageCount = 5,
        WordCount = 600
    },
    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf" },
    Sections =
    [
        // Section 1 — Executive Summary (H1)
        new DocumentSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "Executive Summary", Level = 1 },
            Blocks =
            [
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "This report provides a comprehensive overview of technology investments and outcomes for fiscal year 2024. The organisation accelerated its cloud-first strategy, achieving a 30% reduction in infrastructure costs while improving system reliability to 99.95% uptime.",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 1 }
                },
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "Key highlights include the successful migration of 85% of workloads to the cloud, the launch of three AI-powered products, and a 40% improvement in deployment frequency through DevOps transformation.",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 1 }
                }
            ]
        },

        // Section 2 — Cloud Infrastructure (H1) with Investment Breakdown (H2)
        new DocumentSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "Cloud Infrastructure", Level = 1 },
            Blocks =
            [
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "The cloud infrastructure division led the transition from on-premises data centres to a hybrid-cloud architecture. Azure and AWS were selected as primary providers, with a multi-cloud governance framework ensuring cost optimisation and compliance.",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 2 }
                }
            ],
            SubSections =
            [
                new DocumentSection
                {
                    Id = Guid.NewGuid().ToString(),
                    Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "Investment Breakdown", Level = 2 },
                    Blocks =
                    [
                        new ParagraphBlock
                        {
                            Id = Guid.NewGuid().ToString(),
                            Text = "Total cloud spending reached $12.5M in FY2024, an increase of 18% over the prior year. The table below summarises the allocation across major categories.",
                            Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 2 }
                        },
                        new TableBlock
                        {
                            Id = Guid.NewGuid().ToString(),
                            Headers = ["Category", "FY2023 ($M)", "FY2024 ($M)", "Change"],
                            Rows =
                            [
                                (IReadOnlyList<string>)["Compute", "4.2", "5.0", "+19%"],
                                ["Storage", "2.8", "3.1", "+11%"],
                                ["Networking", "1.5", "1.8", "+20%"],
                                ["Security", "1.1", "1.6", "+45%"],
                                ["Other", "1.0", "1.0", "0%"]
                            ],
                            Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 3 }
                        }
                    ]
                }
            ]
        },

        // Section 3 — AI & Machine Learning (H1)
        new DocumentSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "AI & Machine Learning", Level = 1 },
            Blocks =
            [
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "The AI Centre of Excellence shipped three production models in 2024: a customer-churn predictor (AUC 0.92), a document-extraction pipeline powered by GPT-4o, and a real-time anomaly detector for network traffic.",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 3 }
                },
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "Responsible-AI governance was strengthened with mandatory bias audits and model cards for every production deployment. Training compute was consolidated onto a shared GPU cluster, reducing per-experiment costs by 55%.",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 4 }
                },
                new FigureBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    AltText = "AI model accuracy trend 2022-2024",
                    Caption = "Figure 1: AI Model Accuracy Trend (2022–2024)",
                    ImagePath = "images/ai-accuracy-trend.png",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 4 }
                }
            ]
        },

        // Section 4 — Recommendations & Next Steps (H1)
        new DocumentSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = new HeadingBlock { Id = Guid.NewGuid().ToString(), Text = "Recommendations & Next Steps", Level = 1 },
            Blocks =
            [
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "Based on this year's results we recommend: (1) increasing the security budget by 25% to address emerging threats, (2) expanding the AI Centre of Excellence to include a dedicated MLOps team, and (3) piloting edge computing for latency-sensitive workloads.",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 5 }
                },
                new ParagraphBlock
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = "A detailed roadmap will be published in Q1 2025. Stakeholders are encouraged to review the appendix for supporting data and methodology notes.",
                    Source = new SourceReference { FilePath = "annual-tech-report-2024.pdf", PageNumber = 5 }
                }
            ]
        }
    ]
};

Console.WriteLine($"  Title      : {document.Metadata.Title}");
Console.WriteLine($"  Author     : {document.Metadata.Author}");
Console.WriteLine($"  Pages      : {document.Metadata.PageCount}");
Console.WriteLine($"  Sections   : {document.Sections.Count}");
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 2: Quality Analysis
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 2: Quality Analysis ---");

var qualityAnalyzer = new DocumentQualityAnalyzer();
var qualityReport = qualityAnalyzer.Analyze(document);

Console.WriteLine($"  Overall Score    : {qualityReport.OverallScore:F2}");
Console.WriteLine($"  Issues           : {qualityReport.Issues.Count}");
Console.WriteLine($"  Suggested Action : {qualityReport.SuggestedAction}");
Console.WriteLine();

var qualityText = QualityReportFormatter.FormatAsText(qualityReport);
Console.WriteLine(qualityText);
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 3: Metadata Extraction
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 3: Metadata Extraction ---");

var metadataExtractor = new DocumentMetadataExtractor();
var metadataResult = metadataExtractor.Extract(document);

Console.WriteLine($"  Title         : {metadataResult.Title}");
Console.WriteLine($"  Author        : {metadataResult.Author}");
Console.WriteLine($"  Language      : {metadataResult.Language}");
Console.WriteLine($"  Document Type : {metadataResult.DocumentType}");
Console.WriteLine($"  Word Count    : {metadataResult.WordCount}");
Console.WriteLine($"  Heading Count : {metadataResult.HeadingCount}");
Console.WriteLine();

document = MetadataAttacher.AttachToDocument(document, metadataResult);
Console.WriteLine("  (Metadata attached to document)");
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 4: Chunking
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 4: Chunking ---");

var chunker = new HeadingBasedChunker();
var chunks = chunker.Chunk(document);

Console.WriteLine($"  Chunk count: {chunks.Count}");
Console.WriteLine();

foreach (var chunk in chunks)
{
    var preview = chunk.Content.Length > 80
        ? chunk.Content[..80] + "..."
        : chunk.Content;
    Console.WriteLine($"  Chunk {chunk.Index}: {preview}");
    Console.WriteLine($"    Heading: {chunk.HeadingPath}");
}
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 5: Citations
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 5: Citations ---");

var citations = CitationBuilder.FromDocument(document);

Console.WriteLine($"  Citation count: {citations.Count}");
Console.WriteLine();

var citationIndex = 0;
foreach (var citation in citations)
{
    Console.WriteLine($"  [{citationIndex++}] {CitationFormatter.Format(citation)}");
}
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 6: Vector Records
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 6: Vector Records ---");

var mapper = new DefaultVectorRecordMapper();
var vectorRecords = chunks.Select(c => mapper.MapChunk(c, document)).ToList();

Console.WriteLine($"  Record count: {vectorRecords.Count}");
Console.WriteLine();

if (vectorRecords.Count > 0)
{
    var sample = vectorRecords[0];
    Console.WriteLine("  Sample record:");
    Console.WriteLine($"    Id           : {sample.Id}");
    Console.WriteLine($"    DocumentId   : {sample.DocumentId}");
    Console.WriteLine($"    DocumentTitle: {sample.DocumentTitle}");
    Console.WriteLine($"    HeadingPath  : {sample.HeadingPath}");
    Console.WriteLine($"    ChunkIndex   : {sample.ChunkIndex}");
    Console.WriteLine($"    Content (80) : {(sample.Content.Length > 80 ? sample.Content[..80] + "..." : sample.Content)}");
    Console.WriteLine();
}

var jsonl = JsonlExporter.ExportToString(vectorRecords);
var jsonlLines = jsonl.Split('\n', StringSplitOptions.RemoveEmptyEntries);

Console.WriteLine($"  JSONL lines: {jsonlLines.Length}");
for (var i = 0; i < Math.Min(2, jsonlLines.Length); i++)
{
    var line = jsonlLines[i].Length > 120 ? jsonlLines[i][..120] + "..." : jsonlLines[i];
    Console.WriteLine($"  Line {i}: {line}");
}
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 7: Sync Check
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 7: Sync Check ---");

var sourceBytes = Encoding.UTF8.GetBytes(DocumentSerializer.Serialize(document));
var sourceHash = ContentHasher.ComputeSourceHash(sourceBytes);
var chunkHashes = ContentHasher.ComputeChunkHashes(chunks);

Console.WriteLine($"  Source hash : {sourceHash[..16]}...");
Console.WriteLine($"  Chunk hashes: {chunkHashes.Count}");
Console.WriteLine();

// First sync — everything should be Add
var store = new InMemorySyncStateStore();
var previousState = await store.GetStateAsync(document.Id);
var plan1 = SyncPlanner.ComputePlan(document.Id, sourceHash, chunkHashes, previousState);

Console.WriteLine("  First sync plan:");
Console.WriteLine($"    Action         : {plan1.Action}");
Console.WriteLine($"    Chunks to add  : {plan1.ChunksToAdd.Count}");
Console.WriteLine($"    Chunks to skip : {plan1.ChunksUnchanged.Count}");
Console.WriteLine($"    New version    : {plan1.NewVersion}");
Console.WriteLine();

// Persist state so the second run sees it
var syncState = new SyncState
{
    DocumentId = document.Id,
    SourceHash = sourceHash,
    ChunkHashes = chunkHashes,
    Version = plan1.NewVersion,
    LastSyncedAt = DateTimeOffset.UtcNow
};
await store.SaveStateAsync(syncState);

// Second sync — same content ⇒ everything should be Skip
previousState = await store.GetStateAsync(document.Id);
var plan2 = SyncPlanner.ComputePlan(document.Id, sourceHash, chunkHashes, previousState);

Console.WriteLine("  Re-sync plan (no changes):");
Console.WriteLine($"    Action         : {plan2.Action}");
Console.WriteLine($"    Chunks to add  : {plan2.ChunksToAdd.Count}");
Console.WriteLine($"    Chunks to skip : {plan2.ChunksUnchanged.Count}");
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 8: Document Serialization
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("--- Step 8: Document Serialization ---");

var json = DocumentSerializer.Serialize(document);
Console.WriteLine($"  JSON size    : {json.Length} chars");
Console.WriteLine($"  JSON preview : {(json.Length > 200 ? json[..200] + "..." : json)}");
Console.WriteLine();

var markdown = MarkdownRenderer.Render(document);
Console.WriteLine($"  Markdown size: {markdown.Length} chars");
Console.WriteLine($"  MD preview   : {(markdown.Length > 200 ? markdown[..200] + "..." : markdown)}");
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Summary
// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine("=== Pipeline Complete ===");
Console.WriteLine($"  Sections       : {document.Sections.Count}");
Console.WriteLine($"  Chunks         : {chunks.Count}");
Console.WriteLine($"  Vector records : {vectorRecords.Count}");
Console.WriteLine($"  Quality score  : {qualityReport.OverallScore:F2}");
