# Structured Markdown Import

The optional `ElBruno.MarkItDotNet.Markdown` package parses Markdown into the
`ElBruno.MarkItDotNet.CoreModel.Document` hierarchy using Markdig.

```bash
dotnet add package ElBruno.MarkItDotNet.Markdown
```

```csharp
using ElBruno.MarkItDotNet.Markdown;

var markdown = await File.ReadAllTextAsync("guide.md");
var document = MarkdownDocumentParser.Parse(markdown, "guide.md");

Console.WriteLine(document.Metadata.Title);
Console.WriteLine(document.Sections.Count);
```

The parser enables Markdig's advanced extensions and maps headings, paragraphs,
tables, code blocks, block quotes, and lists into CoreModel blocks. Task list
items retain their checked state. Source paths and character spans are attached
where available. Original block Markdown is retained so `MarkdownRenderer`
preserves inline formatting and syntax it does not model directly.

The parser is an explicit API. It does not replace the core `.md` pass-through
converter, so existing conversion behavior and package dependencies remain
unchanged.
