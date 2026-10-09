// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Text.Json;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Connectors;
using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.DataIngestion;
using ElBruno.MarkItDotNet.Markdown;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging.Abstractions;

var markdown = """
    # Deployment guide

    ## Configuration

    Use **environment variables** for configuration.

    | Setting | Value |
    | --- | --- |
    | Mode | Preview |

    ## Checklist

    - [x] Build the application
    - [ ] Review the deployment

    ```csharp
    Console.WriteLine("Ready!");
    ```

    > Review before deploying.
    """;

var model = MarkdownDocumentParser.Parse(markdown, "docs/deployment.md");
Console.WriteLine($"Title: {model.Metadata.Title}");
PrintSections(model.Sections);

var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine($"Structured JSON length: {json.Length}");
Console.WriteLine(MarkdownRenderer.Render(model));

var ingestion = model.ToIngestionDocument();
Console.WriteLine($"Ingestion identifier: {ingestion.Identifier}");
foreach (var element in ingestion.EnumerateContent())
{
    Console.WriteLine($"{element.GetType().Name}: {element.GetMarkdown()}");
    if (element.Metadata.TryGetValue("markitdotnet.sourceOffset", out var offset))
    {
        Console.WriteLine($"  Source offset: {offset}");
    }
}

var generated = new Document
{
    Id = "release-notes",
    Metadata = new DocumentMetadata { Title = "Release notes", SourceFormat = "generated" },
    Sections =
    [
        new DocumentSection
        {
            Id = "changes",
            Heading = new HeadingBlock { Id = "changes-heading", Text = "Changes", Level = 2 },
            Blocks =
            [
                new ParagraphBlock { Id = "intro", Text = "Documents now have structure." },
                new TableBlock
                {
                    Id = "features",
                    Headers = ["Feature", "Status"],
                    Rows = [["DataIngestion adapter", "Preview"]]
                }
            ]
        }
    ]
};

var generatedIngestion = generated.ToIngestionDocument();
var table = generatedIngestion.EnumerateContent().OfType<IngestionDocumentTable>().Single();
Console.WriteLine($"Generated table: {table.Cells.GetLength(0)} rows x {table.Cells.GetLength(1)} columns");
Console.WriteLine($"Data cell: {table.Cells[1, 0]?.Text}");

var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"markitdotnet-demo-{Guid.NewGuid():N}");
Directory.CreateDirectory(temporaryDirectory);
var archivePath = Path.Combine(temporaryDirectory, "documents.zip");

try
{
    using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
    {
        await WriteEntryAsync(archive, "docs/deployment.md", markdown);
        await WriteEntryAsync(archive, "data/settings.json", """{"mode":"preview","enabled":true}""");
        await WriteEntryAsync(archive, "notes.txt", "Review the deployment checklist.");
    }

    var source = new ZipArchiveConnector(
        new ZipArchiveConnectorOptions
        {
            ArchivePath = archivePath,
            MaxArchiveSizeBytes = 10 * 1024 * 1024,
            MaxEntrySizeBytes = 2 * 1024 * 1024,
            MaxTotalUncompressedBytes = 20 * 1024 * 1024,
            MaxEntries = 100
        },
        NullLogger<ZipArchiveConnector>.Instance);

    var converter = new MarkdownConverter();
    var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".txt", ".json"
    };

    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    await foreach (var document in source.GetDocumentsAsync(timeout.Token))
    {
        var extension = Path.GetExtension(document.Name);
        if (!allowedExtensions.Contains(extension))
        {
            Console.WriteLine($"Skipped unsupported document: {document.Name}");
            continue;
        }

        await using var content = await document.OpenReadAsync(timeout.Token);
        var convertedMarkdown = await converter.ConvertAsync(content, extension, timeout.Token);
        var structured = MarkdownDocumentParser.Parse(convertedMarkdown, document.Source);
        var readyForIngestion = structured.ToIngestionDocument();

        Console.WriteLine($"ZIP -> Markdown -> CoreModel -> IngestionDocument: {document.Name}");
        Console.WriteLine($"  Entry: {document.Metadata[SourceMetadataKeys.ArchiveEntryPath]}");
        Console.WriteLine($"  Elements: {readyForIngestion.EnumerateContent().Count()}");
    }
}
finally
{
    File.Delete(archivePath);
    Directory.Delete(temporaryDirectory);
}

static void PrintSections(IReadOnlyList<DocumentSection> sections, int depth = 0)
{
    foreach (var section in sections)
    {
        var indent = new string(' ', depth * 2);
        Console.WriteLine($"{indent}Section: {section.Heading?.Text ?? "(root)"}");
        foreach (var block in section.Blocks)
        {
            Console.WriteLine($"{indent}  {block.GetType().Name}, ID: {block.Id}");
        }

        PrintSections(section.SubSections, depth + 1);
    }
}

static async Task WriteEntryAsync(ZipArchive archive, string name, string content)
{
    await using var writer = new StreamWriter(archive.CreateEntry(name).Open());
    await writer.WriteAsync(content);
}
