<!-- AI Skill: Use this to teach coding assistants about the markitdown CLI and ElBruno.MarkItDotNet library -->

# MarkItDotNet CLI & Library Skill

**Unified .NET document conversion for AI pipelines, RAG ingestion, and batch processing.**

This skill teaches coding assistants how to use the `markitdown` CLI for terminal-based document conversion and the ElBruno.MarkItDotNet library for programmatic conversion workflows.

## When to Use

- **Converting documents for RAG pipelines** — ingest mixed document types into vector databases
- **Batch processing workflows** — mass-convert directories of files with parallel execution
- **Documentation automation** — convert Office/PDF docs to Markdown for static site generators
- **File ingestion** — turn files into structured Markdown for downstream AI processing
- **Streaming large files** — memory-efficient conversion of PDFs and large documents
- **Web scraping to Markdown** — extract and convert web pages to structured text

## CLI Quick Reference

| Command | Purpose |
|---------|---------|
| `markitdown <file>` | Convert single file to Markdown (stdout or file) |
| `markitdown <file> -o output.md` | Save conversion to file |
| `markitdown <file> --format json` | Output as JSON (with metadata) |
| `markitdown <file> --streaming` | Stream large files chunk-by-chunk |
| `markitdown batch <dir> -o <out>` | Batch convert directory |
| `markitdown batch <dir> -o <out> -r` | Batch recursively (with subdirs) |
| `markitdown batch <dir> -o <out> --pattern "*.pdf"` | Batch with glob filter |
| `markitdown batch <dir> -o <out> --parallel 4` | Control parallelism |
| `markitdown url <url>` | Convert web page to Markdown |
| `markitdown url <url> -o page.md` | Save web page to file |
| `markitdown formats` | List all supported formats |

## Library API Quick Reference

### Register with Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using ElBruno.MarkItDotNet;

var services = new ServiceCollection();

// Core library with 12 built-in converters
services.AddMarkItDotNet();

// Satellite packages (plugins)
services.AddMarkItDotNetExcel();        // .xlsx
services.AddMarkItDotNetPowerPoint();   // .pptx
services.AddMarkItDotNetWhisper();      // Local audio transcription
// services.AddMarkItDotNetAI();         // AI-powered OCR & transcription

var provider = services.BuildServiceProvider();
var markdownService = provider.GetRequiredService<MarkdownService>();
```

### Basic Conversion

```csharp
var result = await markdownService.ConvertAsync("document.pdf");
if (result.Success)
{
    Console.WriteLine(result.Markdown);
}
```

### Stream Conversion

```csharp
using var stream = File.OpenRead("document.pdf");
var result = await markdownService.ConvertAsync(stream, ".pdf");
```

### Streaming (Chunk-by-Chunk)

```csharp
using var stream = File.OpenRead("large-document.pdf");

await foreach (var chunk in markdownService.ConvertStreamingAsync(stream, ".pdf"))
{
    Console.Write(chunk);  // Process or write each chunk
}
```

### URL Conversion

```csharp
var result = await markdownService.ConvertUrlAsync("https://example.com");
Console.WriteLine(result.Markdown);
```

### Plugin System

Extend with custom converters:

```csharp
public interface IConverterPlugin
{
    string Name { get; }
    IEnumerable<IMarkdownConverter> GetConverters();
}

// Register in DI
services.AddSingleton<IConverterPlugin>(new MyCustomPlugin());
```

## Common Patterns for AI Agents

### "Convert this PDF to Markdown"

**CLI:**
```bash
markitdown report.pdf
```

**Code:**
```csharp
var result = await service.ConvertAsync("report.pdf");
Console.WriteLine(result.Markdown);
```

### "Convert all docs in a folder"

**CLI:**
```bash
markitdown batch ./documents -o ./output -r
```

**Code:**
```csharp
var files = Directory.GetFiles("./documents", "*.*", SearchOption.AllDirectories);
foreach (var file in files)
{
    var result = await service.ConvertAsync(file);
    if (result.Success)
    {
        var outPath = Path.Combine("./output", Path.GetFileName(file) + ".md");
        File.WriteAllText(outPath, result.Markdown);
    }
}
```

### "Get structured output for processing"

**CLI:**
```bash
markitdown file.pdf --format json | jq .metadata.wordCount
```

**Code:**
```csharp
var result = await service.ConvertAsync("file.pdf");
// ConversionResult includes SourceFormat, Success, ErrorMessage
Console.WriteLine($"Format: {result.SourceFormat}, Success: {result.Success}");
```

### "Stream large PDF to avoid memory issues"

**CLI:**
```bash
markitdown large.pdf --streaming -o large.md
```

**Code:**
```csharp
using var stream = File.OpenRead("large.pdf");
using var outStream = File.Create("large.md");
using var writer = new StreamWriter(outStream);

await foreach (var chunk in service.ConvertStreamingAsync(stream, ".pdf"))
{
    await writer.WriteAsync(chunk);
}
```

### "Convert web pages to Markdown"

**CLI:**
```bash
markitdown url https://example.com -o page.md
```

**Code:**
```csharp
var result = await service.ConvertUrlAsync("https://example.com");
Console.WriteLine(result.Markdown);
```

### "Batch convert with filtering"

**CLI:**
```bash
markitdown batch ./mixed -o ./converted -r --pattern "*.{pdf,docx}"
```

**Code:**
```csharp
var files = Directory.GetFiles("./mixed", "*.pdf", SearchOption.AllDirectories)
    .Concat(Directory.GetFiles("./mixed", "*.docx", SearchOption.AllDirectories));

foreach (var file in files)
{
    var result = await service.ConvertAsync(file);
    // ...
}
```

### "Parallel batch processing for speed"

**CLI:**
```bash
markitdown batch ./corpus -o ./output -r --parallel 8
```

**Code:**
```csharp
var files = Directory.GetFiles("./corpus", "*.*", SearchOption.AllDirectories);

await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, 
    async (file, ct) =>
    {
        var result = await service.ConvertAsync(file);
        // Write result to output
    });
```

## Supported Formats

| Format | Extensions | Notes |
|--------|-----------|-------|
| Plain Text | `.txt`, `.md`, `.log` | Minimal processing |
| JSON | `.json` | Pretty-printed, fenced code block |
| HTML | `.html`, `.htm` | Strips scripts/styles, converts to Markdown |
| URL | `.url` | Fetches web page and converts |
| Word (DOCX) | `.docx` | Headings, tables, lists, images |
| PDF | `.pdf` | Text extraction + optional streaming |
| CSV / TSV | `.csv`, `.tsv` | Converts to Markdown tables |
| XML | `.xml` | Fenced code block |
| YAML | `.yaml`, `.yml` | Fenced code block |
| RTF | `.rtf` | Text with basic formatting |
| EPUB | `.epub` | Full book content as Markdown |
| Images | `.jpg`, `.png`, `.gif`, `.bmp`, `.webp`, `.svg` | Alt text extraction, file metadata |
| Excel | `.xlsx` | Tables for each sheet (Excel package) |
| PowerPoint | `.pptx` | Slides + speaker notes (PowerPoint package) |
| Images (AI-OCR) | All image formats | LLM vision-based extraction (AI package) |
| Audio (AI) | `.mp3`, `.wav`, `.m4a`, `.ogg` | LLM transcription (AI package) |
| Audio (Local) | `.wav`, `.mp3`, `.m4a`, `.ogg`, `.flac` | Whisper ONNX offline (Whisper package) |

## Installation

### Global Tool

```bash
dotnet tool install -g ElBruno.MarkItDotNet.Cli
markitdown --version
```

### As NuGet Package

```csharp
// In your project
dotnet add package ElBruno.MarkItDotNet
dotnet add package ElBruno.MarkItDotNet.Excel
dotnet add package ElBruno.MarkItDotNet.PowerPoint
```

## Key Concepts

### MarkdownConverter (Simple Facade)

Quick start with pre-registered converters:

```csharp
var converter = new MarkdownConverter();
var markdown = converter.ConvertToMarkdown("document.pdf");
```

### MarkdownService (Advanced)

Use with custom converter registry:

```csharp
var registry = new ConverterRegistry();
registry.RegisterPlugin(myPlugin);
var service = new MarkdownService(registry);
```

### ConversionResult

Always check `Success` before accessing `Markdown`:

```csharp
public class ConversionResult
{
    public string Markdown { get; }          // Converted content
    public string SourceFormat { get; }      // File extension
    public bool Success { get; }             // Conversion successful?
    public string? ErrorMessage { get; }     // Error details
}
```

## Exit Codes (CLI)

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | Conversion error (file corrupted, format error) |
| `2` | File not found |
| `3` | Unsupported format (no converter registered) |

## Performance Tips

1. **Streaming** — Use `--streaming` or `ConvertStreamingAsync()` for PDFs 50+ MB
2. **Parallelism** — On multi-core machines, increase `--parallel` or use `Parallel.ForEachAsync`
3. **Batch Processing** — Use batch mode for directory conversions, not one-by-one
4. **Format Selection** — Plain text/JSON faster than PDF/DOCX; use appropriate format for your needs

## Integration Examples

### RAG Pipeline (Ingest & Chunk)

```bash
markitdown batch ./documents -o ./md -r
# Then feed .md files to chunking/embedding pipeline
```

### Documentation Site Generation

```bash
# Convert all Office docs to Markdown for Hugo/Jekyll
markitdown batch ./docs -o ./content -r --pattern "*.{docx,pdf}"
```

### Document Processing Workflow

```csharp
// 1. Convert
var result = await service.ConvertAsync("document.pdf");

// 2. Process
var chunks = SplitIntoChunks(result.Markdown, 500);

// 3. Embed & store
foreach (var chunk in chunks)
{
    await vectorStore.AddAsync(chunk, metadata);
}
```
