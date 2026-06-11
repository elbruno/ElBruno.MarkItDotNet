# Samples Guide

## Overview

ElBruno.MarkItDotNet ships with **17 sample projects** organized into two tiers. **Simple samples** (1–13) are self-contained console apps that each demonstrate a single feature or format conversion — ideal for learning one concept at a time. **End-to-end samples** (14–17) show how to integrate the library into real-world architectures: a REST API, a batch-processing tool, a RAG ingestion pipeline, and a hosted-agent service for Foundry deployment.

All samples live under `src/samples/` and are included in the solution file.

## Sample Directory

| # | Sample | Path | Description |
|---|--------|------|-------------|
| 1 | BasicConversion | `src/samples/BasicConversion` | Text, JSON, and HTML conversion with DI setup |
| 2 | CsvConversion | `src/samples/CsvConversion` | CSV and TSV to Markdown tables |
| 3 | XmlYamlConversion | `src/samples/XmlYamlConversion` | XML and YAML to fenced code blocks |
| 4 | PdfConversion | `src/samples/PdfConversion` | PDF to Markdown with page metadata and streaming |
| 5 | DocxConversion | `src/samples/DocxConversion` | DOCX to Markdown with headings, tables, and links |
| 6 | RtfEpubConversion | `src/samples/RtfEpubConversion` | RTF and EPUB to Markdown |
| 7 | ExcelConversion | `src/samples/ExcelConversion` | Excel .xlsx to Markdown tables (satellite package) |
| 8 | PowerPointConversion | `src/samples/PowerPointConversion` | PPTX slides and notes to Markdown (satellite package) |
| 9 | AiImageDescription | `src/samples/AiImageDescription` | Image OCR/captioning via IChatClient (AI package) |
| 10 | StreamingConversion | `src/samples/StreamingConversion` | IAsyncEnumerable streaming for large PDFs |
| 11 | CustomConverter | `src/samples/CustomConverter` | Build a custom IMarkdownConverter (.ini files) |
| 12 | PluginPackage | `src/samples/PluginPackage` | Build and register a custom IConverterPlugin |
| 13 | AllFormats | `src/samples/AllFormats` | Converts all supported formats in one app |
| 14 | MarkItDotNet.WebApi | `src/samples/MarkItDotNet.WebApi` | ASP.NET Core Minimal API with file upload |
| 15 | BatchProcessor | `src/samples/BatchProcessor` | Watches a folder and batch-converts files to .md |
| 16 | RagPipeline | `src/samples/RagPipeline` | RAG ingestion: files → Markdown → chunked JSON |
| 17 | MarkItDotNet.FoundryHostedAgent | `src/samples/MarkItDotNet.FoundryHostedAgent` | Blazor web UI + hosted agent endpoint + Aspire AppHost reference for Foundry |

---

## Prerequisites

All samples require **.NET 8.0 SDK** or later. Beyond that:

| Requirement | Samples |
|-------------|---------|
| **ElBruno.MarkItDotNet.Excel** package | ExcelConversion, AllFormats, WebApi, BatchProcessor |
| **ElBruno.MarkItDotNet.PowerPoint** package | PowerPointConversion, AllFormats, WebApi, BatchProcessor |
| **ElBruno.MarkItDotNet.AI** package + an `IChatClient` implementation (e.g., OpenAI, Azure OpenAI) | AiImageDescription |
| **PdfPig** (bundled with core) | PdfConversion, StreamingConversion |

The satellite package references are already declared in each sample's `.csproj` — just restore and run.

---

## Simple Samples

### BasicConversion

**What it demonstrates:** The simplest possible setup — register the core library with DI, then convert plain text, JSON, and HTML content to Markdown. Also shows error handling for unsupported formats and how to inspect the DI container.

**How to run:**

```bash
dotnet run --project src/samples/BasicConversion/BasicConversion.csproj
```

**Key code pattern:** Creates a `ServiceCollection`, calls `AddMarkItDotNet()`, resolves `MarkdownService`, and calls `ConvertAsync(stream, extension)` for each format.

**Expected output:** Conversion results for `.txt`, `.json`, and `.html` inputs with status, source format, and rendered Markdown.

---

### CsvConversion

**What it demonstrates:** Converting CSV and TSV tabular data into Markdown tables, including edge cases like quoted fields containing commas and special characters.

**How to run:**

```bash
dotnet run --project src/samples/CsvConversion/CsvConversion.csproj
```

**Key code pattern:** Passes in-memory CSV/TSV strings as streams with the appropriate extension. The built-in `CsvConverter` detects delimiters and produces pipe-separated Markdown tables.

**Expected output:** Markdown tables rendered from CSV data, plus metadata (word count, processing time).

---

### XmlYamlConversion

**What it demonstrates:** Converting XML and YAML structured data into Markdown fenced code blocks with syntax highlighting hints. Shows that both `.yaml` and `.yml` extensions are supported.

**How to run:**

```bash
dotnet run --project src/samples/XmlYamlConversion/XmlYamlConversion.csproj
```

**Key code pattern:** In-memory XML/YAML content converted via `ConvertAsync()`. The converters wrap content in fenced code blocks (` ```xml ` / ` ```yaml `), preserving indentation.

**Expected output:** Fenced code blocks with original XML/YAML content, plus format and word count metadata.

---

### PdfConversion

**What it demonstrates:** Full PDF-to-Markdown conversion including page metadata, plus streaming page-by-page conversion for large documents. Creates a 2-page PDF in-memory using PdfPig.

**How to run:**

```bash
dotnet run --project src/samples/PdfConversion/PdfConversion.csproj
```

**Key code pattern:** Uses a helper `CreateSamplePdf()` to build a PDF with PdfPig, then converts it two ways — `ConvertAsync()` for full output and `ConvertStreamingAsync()` for page-by-page chunks.

**Expected output:** Full Markdown from both pages, then a streaming view showing each page as a separate chunk.

---

### DocxConversion

**What it demonstrates:** Converting a rich Word document to Markdown, preserving headings (H1, H2), bold/italic text, bulleted lists, tables, and hyperlinks. Builds the DOCX programmatically.

**How to run:**

```bash
dotnet run --project src/samples/DocxConversion/DocxConversion.csproj
```

**Key code pattern:** Uses `DocumentFormat.OpenXml` to create a DOCX in-memory with various formatting elements, then converts via `ConvertAsync()`.

**Expected output:** Structured Markdown with headers, emphasis, lists, and table formatting.

---

### RtfEpubConversion

**What it demonstrates:** Converting RTF content with bold, italic, and underline formatting to Markdown. Also verifies that the EPUB converter is registered and available.

**How to run:**

```bash
dotnet run --project src/samples/RtfEpubConversion/RtfEpubConversion.csproj
```

**Key code pattern:** Passes raw RTF markup (`{\rtf1\ansi...}`) as a stream. Checks the `ConverterRegistry` to confirm `.epub` support is registered.

**Expected output:** Markdown with formatting preserved from RTF, plus confirmation that the EPUB converter is registered.

---

### ExcelConversion

**What it demonstrates:** Converting multi-sheet Excel workbooks to Markdown tables using the `ElBruno.MarkItDotNet.Excel` satellite package.

**How to run:**

```bash
dotnet run --project src/samples/ExcelConversion/ExcelConversion.csproj
```

**Key code pattern:** Registers `AddMarkItDotNetExcel()` alongside the core library. Creates a workbook with ClosedXML containing a "Sales Data" sheet and a "Summary" sheet, then converts with `ConvertAsync()`.

**Expected output:** Markdown tables for each sheet, with column headers and data rows.

---

### PowerPointConversion

**What it demonstrates:** Converting PowerPoint presentations to Markdown, extracting slide titles, content, and speaker notes using the `ElBruno.MarkItDotNet.PowerPoint` satellite package.

**How to run:**

```bash
dotnet run --project src/samples/PowerPointConversion/PowerPointConversion.csproj
```

**Key code pattern:** Registers `AddMarkItDotNetPowerPoint()`. Creates a 2-slide PPTX in-memory with `DocumentFormat.OpenXml`, including titles, bullet points, and speaker notes.

**Expected output:** Markdown with slide titles as headings and bullet points as list items.

---

### AiImageDescription

**What it demonstrates:** AI-powered image analysis using the `ElBruno.MarkItDotNet.AI` package with an `IChatClient` implementation. Uses a mock client for demo purposes.

**How to run:**

```bash
dotnet run --project src/samples/AiImageDescription/AiImageDescription.csproj
```

**Key code pattern:** Creates a `MockChatClient` that returns canned image analysis, registers the `AiConverterPlugin`, and converts a minimal PNG. In production, replace `MockChatClient` with `OpenAIChatClient` or `AzureOpenAIChatClient`.

**Expected output:** Markdown describing the image contents (from the mock: dimensions, color info, and details).

> **Note:** To use real AI capabilities, replace the mock client with a real `IChatClient` backed by OpenAI, Azure OpenAI, or another provider.

---

### StreamingConversion

**What it demonstrates:** Memory-efficient processing of large PDFs using `IAsyncEnumerable<string>`. Yields Markdown chunks page-by-page without loading the entire output into memory.

**How to run:**

```bash
dotnet run --project src/samples/StreamingConversion/StreamingConversion.csproj
```

**Key code pattern:** Creates a 3-page PDF, then iterates with `await foreach (var chunk in converter.ConvertStreamingAsync(...))`, displaying each chunk as it arrives.

**Expected output:** Three separate chunks, one per page, displayed incrementally with chunk index and preview.

---

### CustomConverter

**What it demonstrates:** How to build a custom `IMarkdownConverter` for an unsupported format. Implements an `.ini` file converter that parses sections and key-value pairs into Markdown tables.

**How to run:**

```bash
dotnet run --project src/samples/CustomConverter/CustomConverter.csproj
```

**Key code pattern:** Implements `IniConverter : IMarkdownConverter` with `CanHandle(".ini")` and a `ConvertAsync()` that parses INI sections into `## headings` with key-value tables. Registers via `registry.Register(new IniConverter())`.

**Expected output:** The original `.ini` content, followed by Markdown with section headings and tables of key-value pairs.

---

### PluginPackage

**What it demonstrates:** How to bundle multiple related converters into a reusable `IConverterPlugin`. Creates a `ConfigFilesPlugin` that handles `.env` and `.properties` files.

**How to run:**

```bash
dotnet run --project src/samples/PluginPackage/PluginPackage.csproj
```

**Key code pattern:** Implements `ConfigFilesPlugin : IConverterPlugin` with `GetConverters()` returning an `EnvConverter` and a `PropertiesConverter`. Registers with `registry.RegisterPlugin(new ConfigFilesPlugin())`.

**Expected output:** Conversion results for both `.env` and `.properties` files, rendered as Markdown tables with variable/property names and values.

---

### AllFormats

**What it demonstrates:** A comprehensive integration test that converts all supported text-based formats (`.txt`, `.json`, `.html`, `.csv`, `.xml`, `.yaml`, `.rtf`) in a single run with a summary report.

**How to run:**

```bash
dotnet run --project src/samples/AllFormats/AllFormats.csproj
```

**Key code pattern:** Registers core + Excel + PowerPoint plugins. Iterates through format definitions, converts each in-memory, and collects results into a summary table showing pass/fail, word count, and timing.

**Expected output:** Per-format conversion status with word counts, followed by a summary table with totals.

---

## End-to-End Samples

### MarkItDotNet.WebApi

**What it demonstrates:** A production-ready ASP.NET Core Minimal API that accepts file uploads and returns Markdown. Includes both synchronous and streaming endpoints, plus Swagger documentation.

**How to run:**

```bash
dotnet run --project src/samples/MarkItDotNet.WebApi/MarkItDotNet.WebApi.csproj
```

Then open `https://localhost:<port>/swagger` to explore the API.

**Key code pattern:** Registers `AddMarkItDotNet()`, `AddMarkItDotNetExcel()`, and `AddMarkItDotNetPowerPoint()` in the DI container. Exposes two endpoints:

- `POST /api/convert` — Accepts a file upload, returns JSON with `{ success, markdown, sourceFormat, errorMessage, metadata }`.
- `POST /api/convert/streaming` — Streams Markdown chunks as Server-Sent Events (`text/event-stream`).

**Expected output:** JSON response with converted Markdown, or a stream of SSE events for large files.

---

### BatchProcessor

**What it demonstrates:** A command-line tool that scans a directory, batch-converts all supported files to `.md`, and writes results to an output directory. Uses streaming for PDFs.

**How to run:**

```bash
dotnet run --project src/samples/BatchProcessor/BatchProcessor.csproj -- <input-dir> <output-dir>
```

If no arguments are provided, it creates sample files automatically.

**Key code pattern:** Registers all converters (core + Excel + PowerPoint). Iterates files in the input directory, uses `ConvertStreamingAsync` for PDFs and `ConvertAsync` for others, writes `.md` files, and prints a results table with per-file stats.

**Expected output:** Per-file conversion status with format, word count, and processing time, plus aggregate totals.

---

### RagPipeline

**What it demonstrates:** A complete RAG (Retrieval-Augmented Generation) ingestion pipeline that converts mixed-format documents to Markdown, chunks them into segments, and produces structured JSON ready for embedding and vector storage.

**How to run:**

```bash
dotnet run --project src/samples/RagPipeline/RagPipeline.csproj
```

**Key code pattern:** Six-step pipeline:

1. **Setup** — DI container with `MarkdownService`
2. **Prepare** — Create sample documents (`.txt`, `.html`, `.json`) simulating a knowledge base
3. **Convert** — Normalize all formats to Markdown via `ConvertAsync()`
4. **Chunk** — Split Markdown into 500-character max segments using a `ChunkMarkdown()` helper
5. **Serialize** — Generate JSON array of `DocumentChunk` records with metadata
6. **Report** — Print statistics (document count, chunk count, words, characters, average chunk size)

**Expected output:** Step-by-step progress, serialized JSON chunks, and a statistics summary. The JSON output is ready to feed into an embedding API and vector database.

---

### MarkItDotNet.FoundryHostedAgent

**What it demonstrates:** A Blazor Server app that lets you upload files from the browser, set the hosted agent URL in a textbox, call a hosted agent `invocations` endpoint, and display converted Markdown results. Includes deployment assets for Microsoft Foundry and an Aspire 13 AppHost reference.

**How to run:**

```bash
dotnet run --project src/samples/MarkItDotNet.FoundryHostedAgent/MarkItDotNet.FoundryHostedAgent.csproj
```

**Key code pattern:**

- Browser UI in `Components/Pages/Home.razor` collects file + agent URL
- `HostedAgentClient` forwards file content to the configured agent endpoint using the hosted-agent payload contract
- `POST /invocations` remains available for Foundry-hosted-agent protocol compatibility
- Aspire local orchestration remains available through `apphost.cs`

**Expected output:** Browser-based Markdown preview for uploaded files, plus `200 OK` responses from `/health` and `/invocations`, with deployment-ready `agent.yaml`, `Dockerfile`, and `apphost.cs` assets in the sample folder.
