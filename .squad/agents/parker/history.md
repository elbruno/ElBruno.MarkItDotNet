# Parker — History

## Project Context
- **Project:** ElBruno.MarkItDotNet — .NET library converting file formats to Markdown
- **User:** Bruno Capuano
- **Stack:** .NET 8/10, C#, xUnit, GitHub Actions
- **Testing strategy:** Unit tests per converter, golden file comparisons, cross-platform tests
- **Reference:** ElBruno.LocalLLMs has tests at src/tests/ with unit and integration test projects

## Learnings
- Dallas's converter commit (9a838bf) already enhanced existing core test files (ConversionResult, ConverterRegistry, MarkdownService, PlainTextConverter) with expanded coverage — avoid duplicating that work in future
- All 5 converter test suites (Json, Html, Docx, Pdf, Image) written contract-first against IMarkdownConverter — tests compiled and passed on first build
- DocumentFormat.OpenXml can create in-memory .docx files via `WordprocessingDocument.Create(MemoryStream, ...)` — excellent for unit tests without disk fixtures
- Minimal PDF files can be created with raw PDF syntax for PdfPig-based tests, but xref offsets must be approximate — PdfPig is tolerant
- ImageConverter tests use raw binary headers (PNG IHDR, GIF89a, BMP) to test dimension detection — no image library needed
- ReverseMarkdown-based HtmlConverter strips `<script>` and `<style>` tags by default with `RemoveComments = true` and `GithubFlavored = true`
- TestData files (sample.txt, sample.json, sample.html) need `<None Update="TestData\**\*" CopyToOutputDirectory="PreserveNewest" />` in csproj to be available at test runtime
- 141 tests, 0 failures across all converters and core infrastructure on net8.0

