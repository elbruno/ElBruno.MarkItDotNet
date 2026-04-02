# ElBruno.MarkItDotNet

[![Build](https://github.com/elbruno/ElBruno.MarkItDotNet/actions/workflows/ci.yml/badge.svg)](https://github.com/elbruno/ElBruno.MarkItDotNet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**.NET library that converts 15+ file formats to Markdown** for AI pipelines, documentation workflows, and developer tools. Inspired by Python [markitdown](https://github.com/microsoft/markitdown).

## 📦 NuGet Packages

| Package | Version | Downloads | Description |
|---------|---------|-----------|-------------|
| [ElBruno.MarkItDotNet](https://www.nuget.org/packages/ElBruno.MarkItDotNet) | [![NuGet](https://img.shields.io/nuget/v/ElBruno.MarkItDotNet.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet) | [![Downloads](https://img.shields.io/nuget/dt/ElBruno.MarkItDotNet.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet) | Core library — 12 built-in converters |
| [ElBruno.MarkItDotNet.Excel](https://www.nuget.org/packages/ElBruno.MarkItDotNet.Excel) | [![NuGet](https://img.shields.io/nuget/v/ElBruno.MarkItDotNet.Excel.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.Excel) | [![Downloads](https://img.shields.io/nuget/dt/ElBruno.MarkItDotNet.Excel.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.Excel) | Excel (.xlsx) → Markdown tables |
| [ElBruno.MarkItDotNet.PowerPoint](https://www.nuget.org/packages/ElBruno.MarkItDotNet.PowerPoint) | [![NuGet](https://img.shields.io/nuget/v/ElBruno.MarkItDotNet.PowerPoint.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.PowerPoint) | [![Downloads](https://img.shields.io/nuget/dt/ElBruno.MarkItDotNet.PowerPoint.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.PowerPoint) | PowerPoint (.pptx) → slides + notes |
| [ElBruno.MarkItDotNet.AI](https://www.nuget.org/packages/ElBruno.MarkItDotNet.AI) | [![NuGet](https://img.shields.io/nuget/v/ElBruno.MarkItDotNet.AI.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.AI) | [![Downloads](https://img.shields.io/nuget/dt/ElBruno.MarkItDotNet.AI.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.AI) | AI-powered OCR, captioning, transcription |
| [ElBruno.MarkItDotNet.Whisper](https://www.nuget.org/packages/ElBruno.MarkItDotNet.Whisper) | [![NuGet](https://img.shields.io/nuget/v/ElBruno.MarkItDotNet.Whisper.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.Whisper) | [![Downloads](https://img.shields.io/nuget/dt/ElBruno.MarkItDotNet.Whisper.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.MarkItDotNet.Whisper) | Local audio transcription via Whisper ONNX |

## Description

ElBruno.MarkItDotNet provides a unified interface to convert 15+ file formats into clean, structured Markdown. The core package handles text, JSON, HTML, Word, PDF, RTF, EPUB, images, CSV, XML, YAML, and **URLs (web pages)**. Extend with satellite packages for Excel, PowerPoint, AI-powered features (OCR, image captioning, audio transcription), and **local audio transcription via Whisper**. Designed for AI content pipelines, documentation systems, and any scenario where you need consistent Markdown output from mixed file sources.

## Supported Formats

| Format | Extensions | Converter | Package | Dependencies |
|--------|-----------|-----------|---------|---|
| Plain Text | `.txt`, `.md`, `.log` | `PlainTextConverter` | Core | None |
| JSON | `.json` | `JsonConverter` | Core | None |
| HTML | `.html`, `.htm` | `HtmlConverter` | Core | `ReverseMarkdown` |
| URL (Web Pages) | `.url` | `UrlConverter` | Core | `ReverseMarkdown` |
| Word (DOCX) | `.docx` | `DocxConverter` | Core | `DocumentFormat.OpenXml` |
| PDF | `.pdf` | `PdfConverter` | Core | `PdfPig` |
| CSV | `.csv` | `CsvConverter` | Core | None |
| XML | `.xml` | `XmlConverter` | Core | None |
| YAML | `.yaml`, `.yml` | `YamlConverter` | Core | None |
| RTF | `.rtf` | `RtfConverter` | Core | `RtfPipe` |
| EPUB | `.epub` | `EpubConverter` | Core | `VersOne.Epub` |
| Images | `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.webp`, `.svg` | `ImageConverter` | Core | None |
| Excel (XLSX) | `.xlsx` | `ExcelConverter` | **Excel** | `ClosedXML` |
| PowerPoint (PPTX) | `.pptx` | `PowerPointConverter` | **PowerPoint** | `DocumentFormat.OpenXml` |
| Images (AI-OCR) | All image formats | `AiImageConverter` | **AI** | `Microsoft.Extensions.AI` |
| Audio (AI Transcription) | `.mp3`, `.wav`, `.m4a`, `.ogg` | `AiAudioConverter` | **AI** | `Microsoft.Extensions.AI` |
| PDF (AI-OCR) | `.pdf` | `AiPdfConverter` | **AI** | `Microsoft.Extensions.AI` |
| Audio (Local Whisper) | `.wav`, `.mp3`, `.m4a`, `.ogg`, `.flac` | `WhisperAudioConverter` | **Whisper** | `ElBruno.Whisper` |

## Target Frameworks

- .NET 8.0 (LTS)
- .NET 10.0

## Packages

ElBruno.MarkItDotNet is distributed across multiple NuGet packages for flexibility:

### Core Package

**ElBruno.MarkItDotNet** — The main library with 12 built-in converters.

```bash
dotnet add package ElBruno.MarkItDotNet
```

Includes: Plain text, JSON, HTML, **URLs (web pages)**, Word, PDF, RTF, EPUB, images, CSV, XML, YAML.

### Satellite Packages

**ElBruno.MarkItDotNet.Excel** — Excel (XLSX) to Markdown converter (v0.2.0+)

```bash
dotnet add package ElBruno.MarkItDotNet.Excel
```

Converts spreadsheet sheets to Markdown tables.

**ElBruno.MarkItDotNet.PowerPoint** — PowerPoint (PPTX) to Markdown converter (v0.2.0+)

```bash
dotnet add package ElBruno.MarkItDotNet.PowerPoint
```

Converts slides and speaker notes to Markdown.

**ElBruno.MarkItDotNet.AI** — AI-powered converters (v0.2.0+)

```bash
dotnet add package ElBruno.MarkItDotNet.AI
```

Requires `Microsoft.Extensions.AI` (for `IChatClient`). Provides:
- **AiImageConverter** — OCR for images using LLM vision
- **AiPdfConverter** — OCR for PDFs using LLM vision
- **AiAudioConverter** — Transcription for audio files using LLM audio APIs

**ElBruno.MarkItDotNet.Whisper** — Local audio transcription via Whisper ONNX (v0.3.0+)

```bash
dotnet add package ElBruno.MarkItDotNet.Whisper
```

Uses [ElBruno.Whisper](https://www.nuget.org/packages/ElBruno.Whisper) for offline speech-to-text. No cloud API needed — runs locally via ONNX Runtime. Supports `.wav`, `.mp3`, `.m4a`, `.ogg`, `.flac`.

## Installation

For the core library only:

```bash
dotnet add package ElBruno.MarkItDotNet
```

For Excel support:

```bash
dotnet add package ElBruno.MarkItDotNet.Excel
```

For PowerPoint support:

```bash
dotnet add package ElBruno.MarkItDotNet.PowerPoint
```

For AI-powered features (requires separate `IChatClient` registration):

```bash
dotnet add package ElBruno.MarkItDotNet.AI
```

For local audio transcription (offline, no API key needed):

```bash
dotnet add package ElBruno.MarkItDotNet.Whisper
```

## Quick Start

The simplest way to get started is with the `MarkdownConverter` façade:

```csharp
using ElBruno.MarkItDotNet;

// Convert a file to Markdown
var converter = new MarkdownConverter();
var markdown = converter.ConvertToMarkdown("document.txt");
Console.WriteLine(markdown);

// Or convert from a stream
using var stream = File.OpenRead("document.pdf");
var result = await converter.ConvertAsync(stream, ".pdf");
Console.WriteLine(result.Markdown);
```

The `MarkdownConverter` class pre-registers all built-in converters (from the core package) and provides synchronous and asynchronous conversion methods.

### URL Conversion

Convert web pages directly to Markdown:

```csharp
var service = new MarkdownService(registry);
var result = await service.ConvertUrlAsync("https://example.com");
Console.WriteLine(result.Markdown);
```

The URL converter fetches the page, strips navigation/scripts/styles, extracts the title, and converts the content to clean Markdown.

### With Satellite Packages

When you install satellite packages (Excel, PowerPoint, AI), converters are automatically registered during dependency injection setup. The system discovers them via the plugin system.

## Dependency Injection with Plugin System

For advanced scenarios (e.g., ASP.NET Core applications), use the DI extension methods to register MarkItDotNet services:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Excel;
using ElBruno.MarkItDotNet.PowerPoint;

var services = new ServiceCollection();

// Register core MarkItDotNet with built-in converters
services.AddMarkItDotNet();

// Register satellite package converters (plugins)
services.AddMarkItDotNetExcel();
services.AddMarkItDotNetPowerPoint();

// Register AI converters (requires IChatClient)
// services.AddMarkItDotNetAI();

var provider = services.BuildServiceProvider();
var markdownService = provider.GetRequiredService<MarkdownService>();

// Convert files through the service (converters auto-discovered)
var result = await markdownService.ConvertAsync("document.xlsx");
if (result.Success)
{
    Console.WriteLine(result.Markdown);
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

All registered converters (core + plugins) are automatically available through the `MarkdownService`.

## Streaming Conversion

For large files, use the streaming API to process content chunk-by-chunk:

```csharp
var converter = new MarkdownConverter();
using var stream = File.OpenRead("large-document.pdf");

await foreach (var chunk in converter.ConvertStreamingAsync(stream, ".pdf"))
{
    Console.Write(chunk);
}
```

The streaming API yields Markdown chunks asynchronously (e.g., page-by-page for PDFs), enabling memory-efficient processing of large files.

## AI-Powered Conversion

The `ElBruno.MarkItDotNet.AI` package provides converters that use LLM vision and audio APIs for advanced capabilities:

### Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.AI;

var services = new ServiceCollection();

// Register a chat client (e.g., OpenAI)
services.AddOpenAIChatClient("sk-...", "gpt-4-vision");

// Register core + AI converters
services.AddMarkItDotNet();
services.AddMarkItDotNetAI();

var provider = services.BuildServiceProvider();
var markdownService = provider.GetRequiredService<MarkdownService>();

// Use AI converters transparently
var result = await markdownService.ConvertAsync("screenshot.png");
Console.WriteLine(result.Markdown);
```

### AI Converters

- **AiImageConverter** — Uses LLM vision to describe images and extract text
- **AiPdfConverter** — Uses LLM vision to OCR PDFs (complements plain text extraction)
- **AiAudioConverter** — Uses LLM audio APIs to transcribe audio files (MP3, WAV, M4A, OGG)

Configure behavior via `AiOptions`:

```csharp
services.AddMarkItDotNetAI(options =>
{
    options.ImageDescriptionPrompt = "Describe this image in detail...";
    options.MaxRetries = 3;
});
```

## Local Audio Transcription (Whisper)

The `ElBruno.MarkItDotNet.Whisper` package uses [ElBruno.Whisper](https://www.nuget.org/packages/ElBruno.Whisper) for offline speech-to-text powered by ONNX Runtime. No cloud API needed.

```csharp
using ElBruno.Whisper;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Whisper;

// Create Whisper client (downloads model on first run ~75MB)
using var whisperClient = await WhisperClient.CreateAsync();

// Register the plugin
var registry = new ConverterRegistry();
registry.RegisterPlugin(new WhisperConverterPlugin(whisperClient));

var service = new MarkdownService(registry);
var result = await service.ConvertAsync("recording.wav");
Console.WriteLine(result.Markdown);
```

Or with DI:

```csharp
services.AddMarkItDotNet();
services.AddMarkItDotNetWhisper(options =>
{
    options.Model = KnownWhisperModels.WhisperBaseEn; // Optional: pick model size
});
```

## API Reference

### MarkdownService

The main service for converting files to Markdown. Use this in DI scenarios or when you need advanced control over converters.

```csharp
public class MarkdownService
{
    public MarkdownService(ConverterRegistry registry);
    
    // Convert a file at the given path
    public Task<ConversionResult> ConvertAsync(string filePath);
    
    // Convert from a stream with explicit file extension
    public Task<ConversionResult> ConvertAsync(Stream stream, string fileExtension);
    
    // Stream conversion for large files
    public IAsyncEnumerable<string> ConvertStreamingAsync(Stream stream, string fileExtension);
}
```

### ConversionResult

Represents the outcome of a file conversion. Always check `Success` before accessing `Markdown`.

```csharp
public class ConversionResult
{
    public string Markdown { get; }          // Converted content (empty if failed)
    public string SourceFormat { get; }      // Source format (e.g., ".pdf")
    public bool Success { get; }             // Whether conversion succeeded
    public string? ErrorMessage { get; }     // Error details if Success is false
}
```

### IMarkdownConverter

Contract for implementing custom converters.

```csharp
public interface IMarkdownConverter
{
    // Check if this converter handles the given file extension
    bool CanHandle(string fileExtension);
    
    // Perform the conversion (extension includes the leading dot)
    Task<string> ConvertAsync(Stream fileStream, string fileExtension);
}
```

### IStreamingMarkdownConverter

Extended contract for converters that support streaming (chunk-by-chunk processing).

```csharp
public interface IStreamingMarkdownConverter : IMarkdownConverter
{
    // Converts content to Markdown, yielding chunks asynchronously
    IAsyncEnumerable<string> ConvertStreamingAsync(
        Stream fileStream,
        string fileExtension,
        CancellationToken cancellationToken = default);
}
```

### IConverterPlugin

Contract for plugin packages that bundle one or more converters.

```csharp
public interface IConverterPlugin
{
    // Human-readable name of the plugin (e.g., "Excel", "AI")
    string Name { get; }
    
    // Returns all converters provided by this plugin
    IEnumerable<IMarkdownConverter> GetConverters();
}
```

### ConverterRegistry

Manages and resolves converters by file extension.

```csharp
public class ConverterRegistry
{
    public void Register(IMarkdownConverter converter);
    public void RegisterPlugin(IConverterPlugin plugin);
    public IMarkdownConverter? Resolve(string extension);
    public IReadOnlyList<IMarkdownConverter> GetAll();
}
```

## Custom Converters

You can implement custom converters for unsupported file formats by implementing `IConverterPlugin` or `IMarkdownConverter`:

### Quick Custom Converter

Implement `IMarkdownConverter` for a single format:

```csharp
using ElBruno.MarkItDotNet;
using System.Text;

public class CsvConverter : IMarkdownConverter
{
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension)
    {
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var csv = await reader.ReadToEndAsync();
        
        var lines = csv.Split('\n');
        if (lines.Length == 0) return string.Empty;
        
        var sb = new StringBuilder();
        
        // Header row
        var headers = lines[0].Split(',');
        sb.Append("| ");
        sb.Append(string.Join(" | ", headers));
        sb.AppendLine(" |");
        sb.Append("|");
        sb.Append(string.Concat(headers.Select(_ => " --- |")));
        sb.AppendLine();
        
        // Data rows
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cells = lines[i].Split(',');
            sb.Append("| ");
            sb.Append(string.Join(" | ", cells));
            sb.AppendLine(" |");
        }
        
        return sb.ToString();
    }
}
```

Register with DI:

```csharp
services.AddMarkItDotNet();
var registry = provider.GetRequiredService<ConverterRegistry>();
registry.Register(new CsvConverter());
```

### Satellite Plugin Package

For reusable plugins, implement `IConverterPlugin`:

```csharp
using ElBruno.MarkItDotNet;

public class MyCustomPlugin : IConverterPlugin
{
    public string Name => "MyCustom";

    public IEnumerable<IMarkdownConverter> GetConverters() =>
    [
        new MyFormatConverter1(),
        new MyFormatConverter2()
    ];
}
```

Register in DI:

```csharp
services.AddSingleton<IConverterPlugin>(new MyCustomPlugin());
```

The registry automatically discovers and loads all registered plugins.

## 📦 Samples

See [Samples Guide](docs/samples.md) for detailed walkthroughs.

### Simple Samples

| Sample | Description | Run Command |
|--------|-------------|-------------|
| [BasicConversion](src/samples/BasicConversion) | Text, JSON, and HTML conversion with DI | `dotnet run --project src/samples/BasicConversion/BasicConversion.csproj` |
| [CsvConversion](src/samples/CsvConversion) | CSV and TSV → Markdown tables | `dotnet run --project src/samples/CsvConversion/CsvConversion.csproj` |
| [XmlYamlConversion](src/samples/XmlYamlConversion) | XML and YAML → fenced code blocks | `dotnet run --project src/samples/XmlYamlConversion/XmlYamlConversion.csproj` |
| [PdfConversion](src/samples/PdfConversion) | PDF → Markdown with page metadata + streaming | `dotnet run --project src/samples/PdfConversion/PdfConversion.csproj` |
| [DocxConversion](src/samples/DocxConversion) | DOCX → Markdown with headings, tables, links | `dotnet run --project src/samples/DocxConversion/DocxConversion.csproj` |
| [RtfEpubConversion](src/samples/RtfEpubConversion) | RTF and EPUB → Markdown | `dotnet run --project src/samples/RtfEpubConversion/RtfEpubConversion.csproj` |
| [ExcelConversion](src/samples/ExcelConversion) | Excel .xlsx → Markdown tables (Excel package) | `dotnet run --project src/samples/ExcelConversion/ExcelConversion.csproj` |
| [PowerPointConversion](src/samples/PowerPointConversion) | PPTX slides + notes → Markdown (PowerPoint package) | `dotnet run --project src/samples/PowerPointConversion/PowerPointConversion.csproj` |
| [AiImageDescription](src/samples/AiImageDescription) | Image OCR/captioning via IChatClient (AI package) | `dotnet run --project src/samples/AiImageDescription/AiImageDescription.csproj` |
| [StreamingConversion](src/samples/StreamingConversion) | IAsyncEnumerable streaming for large PDFs | `dotnet run --project src/samples/StreamingConversion/StreamingConversion.csproj` |
| [CustomConverter](src/samples/CustomConverter) | Build a custom IMarkdownConverter (.ini files) | `dotnet run --project src/samples/CustomConverter/CustomConverter.csproj` |
| [PluginPackage](src/samples/PluginPackage) | Build and register a custom IConverterPlugin | `dotnet run --project src/samples/PluginPackage/PluginPackage.csproj` |
| [AllFormats](src/samples/AllFormats) | Converts all supported formats in one app | `dotnet run --project src/samples/AllFormats/AllFormats.csproj` |
| [UrlConversion](src/samples/UrlConversion) | Web page URL → Markdown | `dotnet run --project src/samples/UrlConversion/UrlConversion.csproj` |
| [WhisperTranscription](src/samples/WhisperTranscription) | Local audio transcription via Whisper ONNX | `dotnet run --project src/samples/WhisperTranscription/WhisperTranscription.csproj` |

### End-to-End Samples

| Sample | Description | Run Command |
|--------|-------------|-------------|
| [MarkItDotNet.WebApi](src/samples/MarkItDotNet.WebApi) | ASP.NET Core Minimal API with file upload + streaming | `dotnet run --project src/samples/MarkItDotNet.WebApi/MarkItDotNet.WebApi.csproj` |
| [BatchProcessor](src/samples/BatchProcessor) | Watches folder and batch-converts files to .md | `dotnet run --project src/samples/BatchProcessor/BatchProcessor.csproj` |
| [RagPipeline](src/samples/RagPipeline) | RAG ingestion: files → Markdown → chunked JSON | `dotnet run --project src/samples/RagPipeline/RagPipeline.csproj` |

## Documentation

- [Samples Guide](docs/samples.md) — detailed walkthroughs for all sample projects
- [Architecture](docs/architecture.md) — design decisions, plugin system, converter pipeline, and internal structure
- [Plugins Guide](docs/plugins.md) — how to create custom plugin packages
- [Building & Testing](docs/building-and-testing.md) — how to build from source and run tests
- [Image Generation Prompts](docs/image-generation-prompts.md) — AI prompts for branding assets
- [Acknowledgements](docs/acknowledgements.md) — open-source libraries that power this project

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## 👋 About the Author

**Made with ❤️ by [Bruno Capuano (ElBruno)](https://github.com/elbruno)**

- 📝 **Blog**: [elbruno.com](https://elbruno.com)
- 📺 **YouTube**: [youtube.com/elbruno](https://youtube.com/elbruno)
- 🔗 **LinkedIn**: [linkedin.com/in/elbruno](https://linkedin.com/in/elbruno)
- 𝕏 **Twitter**: [twitter.com/elbruno](https://twitter.com/elbruno)
- 🎙️ **Podcast**: [notienenombre.com](https://notienenombre.com)
