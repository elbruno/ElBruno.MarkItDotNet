# PRD: ElBruno.MarkItDotNet

## Overview
ElBruno.MarkItDotNet is a .NET library inspired by the Python project "markitdown".
It converts multiple file formats into Markdown for use in AI pipelines, documentation workflows, and developer tools.

Target frameworks:
- .NET 8 (LTS)
- .NET 10

---

## Goals
- Convert common file formats into clean Markdown
- Provide extensible architecture for new converters
- Support AI-ready pipelines (RAG, embeddings, ingestion)
- Be easy to use from CLI, SDK, and web apps

---

## Non-Goals
- Full WYSIWYG rendering
- Editing Markdown (focus is conversion only)

---

## Supported Formats (v1)
- Plain text (.txt)
- HTML (.html)
- PDF (.pdf)
- Microsoft Word (.docx)
- Images (OCR optional)
- JSON / structured data

---

## Architecture

### Core Concepts
- IMarkdownConverter
- Converter Registry
- Pipeline Processing

### Interface
```csharp
public interface IMarkdownConverter
{
    bool CanHandle(string fileExtension);
    Task<string> ConvertAsync(Stream fileStream);
}
```

### Registry
```csharp
public class ConverterRegistry
{
    private readonly List<IMarkdownConverter> _converters;

    public IMarkdownConverter Resolve(string extension);
}
```

---

## Features

### v1
- File type detection
- Conversion to Markdown
- Plugin-based converters
- Basic formatting cleanup

### v2 (Future)
- OCR support (images, PDFs)
- Table extraction
- AI enrichment (summaries, metadata)
- Streaming conversion

---

## API Design

### Basic Usage
```csharp
var converter = new MarkdownService();
var markdown = await converter.ConvertAsync("file.pdf");
```

### Advanced
```csharp
services.AddMarkItDotNet(options =>
{
    options.EnableOcr = true;
});
```

---

## Extensibility
- Developers can register custom converters
- Support dependency injection

---

## CLI Tool (Optional)
Command:
```
markitdotnet convert file.pdf
```

---

## Output Quality Goals
- Clean Markdown
- Preserve structure (headings, lists, tables)
- Minimal noise

---

## Testing Strategy
- Unit tests per converter
- Golden file comparisons
- Cross-platform tests

---

## Packaging
- NuGet package: ElBruno.MarkItDotNet
- CLI global tool (optional)

---

## GitHub Actions
- Build & test on PR
- Publish NuGet on release
- Generate sample outputs

---

## Future Ideas
- Integration with Azure AI Search
- Integration with embeddings
- Blazor demo app

---

## Success Metrics
- # of supported formats
- Conversion accuracy
- Adoption (NuGet downloads)

---

## Inspiration
- Python markitdown project
