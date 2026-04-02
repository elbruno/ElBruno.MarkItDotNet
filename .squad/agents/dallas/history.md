# Dallas — History

## Project Context
- **Project:** ElBruno.MarkItDotNet — .NET library converting file formats to Markdown
- **User:** Bruno Capuano
- **Stack:** .NET 8/10, C#, xUnit, NuGet, GitHub Actions
- **Architecture:** IMarkdownConverter interface with CanHandle(extension) + ConvertAsync(stream), ConverterRegistry resolves converters, MarkdownService is the main entry point
- **Formats (v1):** txt, html, pdf, docx, json, images (OCR optional)

## Learnings
- **Architecture:** IMarkdownConverter → ConverterRegistry → MarkdownService pipeline. ConverterRegistry is the central resolver; MarkdownService wraps it with ConversionResult error handling. MarkdownConverter kept as backward-compatible sync façade.
- **ConversionResult pattern:** Static factories `Succeeded()` / `Failure()` — never throw from MarkdownService; always return a result object. Exceptions only propagate from the façade.
- **DI wiring:** `ServiceCollectionExtensions.AddMarkItDotNet()` registers registry as singleton, builds it with all built-in converters. Options passed via Action<T> pattern.
- **Extension normalization:** All extension matching is lowercased at registry/service level. Converters use OrdinalIgnoreCase.
- **Key files:** `src/ElBruno.MarkItDotNet/IMarkdownConverter.cs`, `ConverterRegistry.cs`, `MarkdownService.cs`, `ConversionResult.cs`, `ServiceCollectionExtensions.cs`, `Converters/PlainTextConverter.cs`
- **Package added:** Microsoft.Extensions.DependencyInjection.Abstractions 9.0.6
- **Tests:** 23 total (3 backward-compat + 6 registry + 6 service + 6 plaintext + 2 conversionresult)
- **Phases 3-6 converters:** Built all 5 remaining converters following PlainTextConverter pattern:
  - JsonConverter (.json): System.Text.Json for pretty-printing, fenced code blocks with ```json
  - HtmlConverter (.html/.htm): ReverseMarkdown 4.6.0 for HTML→Markdown
  - DocxConverter (.docx): DocumentFormat.OpenXml 3.3.0 — headings, bold/italic, lists, tables
  - PdfConverter (.pdf): PdfPig 0.1.14 — page-by-page extraction with --- separators
  - ImageConverter (.jpg/.jpeg/.png/.gif/.bmp/.webp/.svg): markdown image refs + dimension detection from file headers
- **Package note:** PdfPig NuGet package is named `PdfPig` (not `UglyToad.PdfPig`); the `UglyToad.PdfPig` ID only has pre-release versions.
- **All converters registered** in both ServiceCollectionExtensions (DI) and MarkdownConverter (façade). 41 tests pass after changes.
