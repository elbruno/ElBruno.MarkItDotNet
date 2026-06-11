# Architecture

## Overview

ElBruno.MarkItDotNet uses a **plugin-based converter system** where each file format has a dedicated converter that implements a common interface. A registry resolves converters by file extension, and a service orchestrates the conversion workflow. Satellite packages extend the core via the plugin system using dependency injection.

## Core Components

```
┌──────────────────────────────────────────────────────────────────┐
│                      MarkdownService                              │
│              (main entry point / orchestrator)                    │
├──────────────────────────────────────────────────────────────────┤
│                    ConverterRegistry                              │
│         (resolves converters by extension and plugins)            │
├────────┬────────┬────────┬────────┬─────────┬────────┬──────────┤
│  Core Converters                                  │  Plugins     │
├────────┴────────┴────────┴────────┴─────────┴────────┴──────────┤
│ Text │JSON │HTML │DOCX│PDF │RTF│EPUB│CSV│XML│YAML│Image        │
│      │    │Rever│Open│Pdf │Rtf│VersOne│         │              │
│ Plain│For-│seMarkxml   │Pig │Pipe│Epub        │              │
│ read │mat│down         │    │    │            │              │
├────────────────────────────────────────────────────────────────────┤
│ Plugins (Satellite Packages via Dependency Injection)              │
├────────────────────────────────────────────────────────────────────┤
│ ExcelPlugin         │ PowerPointPlugin     │ AiConverterPlugin    │
│ (ExcelConverter)    │ (PowerPointConverter)│ (Ai*Converters)      │
└────────────────────────────────────────────────────────────────────┘
         All implement IConverterPlugin
```

## Converter Interfaces

### IMarkdownConverter

The core contract. Every converter implements two methods:

- `CanHandle(string fileExtension)` — returns `true` if the converter supports the given extension
- `ConvertAsync(Stream fileStream, string fileExtension)` — converts the stream content to Markdown

### IStreamingMarkdownConverter

Extended contract for converters that support chunk-by-chunk streaming (e.g., page-by-page for PDFs):

- `ConvertStreamingAsync(Stream fileStream, string fileExtension, CancellationToken)` — yields Markdown chunks asynchronously
- Extends `IMarkdownConverter`, so it's backward-compatible with `ConvertAsync()`

### IConverterPlugin

Contract for satellite packages that bundle one or more converters:

- `Name` — human-readable name (e.g., "Excel", "AI")
- `GetConverters()` — returns all converters provided by the plugin

## Components in Detail

### ConverterRegistry

Manages all registered converters (core + plugins) and resolves the right one for a given extension:

- `Register(IMarkdownConverter)` — add a single converter
- `RegisterPlugin(IConverterPlugin)` — add all converters from a plugin
- `Resolve(string extension)` — find the converter for an extension
- `GetAll()` — list all registered converters

### MarkdownService

The top-level service that wraps the registry and provides convenience methods:

- `ConvertAsync(string filePath)` — open and convert a file
- `ConvertAsync(Stream stream, string extension)` — convert from a stream
- `ConvertStreamingAsync(Stream stream, string extension)` — streaming conversion
- Wraps results in `ConversionResult` (success/failure)

### MarkdownConverter (Façade)

A backward-compatible static-like entry point that pre-registers all core converters (no plugins). Use this for quick scripts; use `MarkdownService` + DI for production apps.

### ConversionResult

A result type with `Success`, `Markdown`, `SourceFormat`, and `ErrorMessage`. Always check `Success` before reading `Markdown`.

## Converter Details

| Converter | Extensions | Strategy | Location | Dependencies |
|-----------|-----------|----------|----------|---|
| PlainTextConverter | .txt, .md, .log | Direct stream read | Core | None |
| JsonConverter | .json | Pretty-print + fenced code block | Core | None |
| HtmlConverter | .html, .htm | HTML → Markdown via ReverseMarkdown | Core | `ReverseMarkdown` |
| DocxConverter | .docx | Extract paragraphs via OpenXml SDK | Core | `DocumentFormat.OpenXml` |
| PdfConverter | .pdf | Extract text per page via PdfPig | Core | `PdfPig` |
| RtfConverter | .rtf | RTF → Markdown via RtfPipe | Core | `RtfPipe` |
| EpubConverter | .epub | Extract content via VersOne.Epub | Core | `VersOne.Epub` |
| CsvConverter | .csv | CSV → Markdown table | Core | None |
| XmlConverter | .xml | XML → pretty Markdown code block | Core | None |
| YamlConverter | .yaml, .yml | YAML → pretty Markdown code block | Core | None |
| ImageConverter | .png, .jpg, .gif, .bmp, .webp, .svg | Metadata placeholder (no OCR) | Core | None |
| ExcelConverter | .xlsx | Sheets → Markdown tables | Excel Plugin | `ClosedXML` |
| PowerPointConverter | .pptx | Slides + notes → Markdown | PowerPoint Plugin | `DocumentFormat.OpenXml` |
| AiImageConverter | All image formats | LLM vision (OCR, description) | AI Plugin | `Microsoft.Extensions.AI` |
| AiPdfConverter | .pdf | LLM vision (OCR with vision) | AI Plugin | `Microsoft.Extensions.AI` |
| AiAudioConverter | .mp3, .wav, .m4a, .ogg | LLM audio API (transcription) | AI Plugin | `Microsoft.Extensions.AI` |

## Plugin System

### Satellite Packages

Satellite packages (Excel, PowerPoint, AI) are standard NuGet packages that:

1. Implement `IConverterPlugin` to bundle their converters
2. Provide a `ServiceCollectionExtensions.AddMarkItDotNet*()` method for DI registration
3. Are loaded via dependency injection — no manual discovery needed

### Plugin Registration

Plugins are registered via DI:

```csharp
services.AddMarkItDotNet();                    // Core converters
services.AddMarkItDotNetExcel();              // Excel plugin
services.AddMarkItDotNetPowerPoint();         // PowerPoint plugin
services.AddMarkItDotNetAI();                 // AI plugin
```

The `ConverterRegistry` singleton discovers plugins registered as `IConverterPlugin` instances in the DI container.

## Dependency Injection

`services.AddMarkItDotNet()` registers:

- `ConverterRegistry` as singleton (with all core converters)
- `MarkdownService` as transient
- `MarkItDotNetOptions` from configuration (optional)

Satellite packages register additional `IConverterPlugin` instances that are auto-discovered by `ConverterRegistry`.

## Hosted-Agent Web UI Bridge Pattern (Sample)

The `src/samples/MarkItDotNet.FoundryHostedAgent` sample demonstrates a browser-facing integration pattern for hosted agents:

- A **Blazor Server UI** accepts file uploads and allows runtime configuration of the hosted-agent endpoint URL.
- A thin HTTP client service (`HostedAgentClient`) converts uploaded bytes to the hosted-agent invocation contract (`fileName`, `extension`, `contentBase64`) and calls `POST /invocations`.
- The same sample also keeps a local `POST /invocations` endpoint for protocol compatibility and local loopback testing.

This pattern separates UI concerns from conversion protocol concerns and supports local development (localhost endpoint) and cloud-hosted endpoints (for example, Azure-hosted agent URLs) without code changes.

## Extending with Custom Converters

### Single Converter

1. Implement `IMarkdownConverter`
2. Register via `ConverterRegistry.Register()` or DI
3. The registry automatically resolves your converter for matching extensions

### Reusable Plugin Package

1. Implement `IConverterPlugin` to bundle converters
2. Provide `ServiceCollectionExtensions.AddMyPlugin()` for DI registration
3. Register via `services.AddSingleton<IConverterPlugin>(new MyPlugin())`

## Design Decisions

- **Stream-first API** — converters take `Stream`, not file paths, enabling in-memory and cloud scenarios
- **Extension-based routing** — simple, predictable, no content sniffing
- **Plugin system via DI** — satellite packages are discovered automatically through dependency injection
- **Optional streaming** — `IStreamingMarkdownConverter` for memory-efficient processing of large files
- **AI-powered extensions** — vision and audio APIs in a separate package, keeping core lightweight
- **No OCR built-in** — plain `ImageConverter` provides metadata only; OCR available via AI package
- **Multi-target net8.0 + net10.0** — supports current LTS and latest .NET
