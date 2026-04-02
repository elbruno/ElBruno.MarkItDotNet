# Decision: Converter NuGet Package Choices

**Author:** Dallas (Backend Developer)  
**Date:** 2025-07-18  
**Status:** Implemented  

## Context
Phases 3-6 required adding HTML, DOCX, PDF, and Image converters to the library. Each needed a NuGet dependency for parsing the source format.

## Decisions

### ReverseMarkdown 4.6.0 — HTML to Markdown
- **Why:** Purpose-built for HTML→Markdown conversion. Handles headings, lists, links, images, tables, bold/italic, and code blocks out of the box.
- **Config:** UnknownTags=PassThrough, RemoveComments=true, GithubFlavored=true for GitHub-compatible output.
- **Alternatives considered:** Manual regex/HtmlAgilityPack — too error-prone for complex HTML.

### DocumentFormat.OpenXml 3.3.0 — DOCX parsing
- **Why:** Microsoft's official SDK for Open XML formats. Zero native dependencies, pure .NET. Supports reading Word styles (headings), run properties (bold/italic), tables, and numbering (lists).
- **Alternatives considered:** NPOI — heavier dependency, less direct access to style info.

### PdfPig 0.1.14 — PDF text extraction
- **Why:** Pure .NET PDF reader, no native dependencies. Extracts text page-by-page. Lightweight and well-suited for text extraction use case.
- **Note:** The NuGet package ID is `PdfPig`, NOT `UglyToad.PdfPig` (which only has pre-release builds).
- **Alternatives considered:** iTextSharp (AGPL licensed), PdfSharp (limited text extraction).

### ImageConverter — No external dependency
- **Why:** v1 only generates markdown image references (`![Image](filename)`) and reads image dimensions from file headers (PNG, JPEG, GIF, BMP) using raw byte parsing. No heavy imaging libraries needed.
- **OCR:** Stubbed behind `MarkItDotNetOptions.EnableOcr` for v2 implementation.

## Impact
- Three new NuGet dependencies added to ElBruno.MarkItDotNet.csproj
- All converters follow the IMarkdownConverter pattern (CanHandle + ConvertAsync)
- Registered in both DI (ServiceCollectionExtensions) and sync façade (MarkdownConverter)
