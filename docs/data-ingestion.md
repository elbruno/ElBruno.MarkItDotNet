# Microsoft.Extensions.DataIngestion adapter

`ElBruno.MarkItDotNet.DataIngestion` bridges structured MarkItDotNet CoreModel documents into Microsoft's `IngestionDocument` representation. It maps sections, headings, paragraphs, tables, and figures to native ingestion elements. Code, lists, quotes, and other block types are represented as Markdown paragraphs with their original Markdown and block type metadata retained.

Install the optional prerelease package:

```bash
dotnet add package ElBruno.MarkItDotNet.DataIngestion --prerelease
dotnet add package ElBruno.MarkItDotNet.Markdown
```

Prerelease releases use the **Publish to NuGet** manual workflow with `channel: adapter-preview` and a version such as `0.9.3-preview.1`. This channel publishes only the adapter and its matching CoreModel prerelease. Stable releases do not publish the adapter. Both channels use the existing `publish.yml` trusted publisher identity.

The adapter depends only on `Microsoft.Extensions.DataIngestion.Abstractions`, not on Microsoft's Markdig reader or its pipeline utilities. This avoids duplicating Markdown parsing and keeps the prerelease dependency out of the CoreModel package.

If you want to parse Markdown first, install `ElBruno.MarkItDotNet.Markdown`, then convert the resulting document before passing it to a Microsoft ingestion chunker or processor:

```csharp
using ElBruno.MarkItDotNet.DataIngestion;
using ElBruno.MarkItDotNet.Markdown;

var model = MarkdownDocumentParser.Parse(markdown, "docs/guide.md");
var ingestionDocument = model.ToIngestionDocument();
```

The resulting identifier is the CoreModel document ID. Document-level and block-level metadata is retained with the `markitdotnet.` metadata prefix, including block IDs, source paths and spans, source format, and custom metadata. Original Markdown is retained where available; blocks created programmatically are rendered by `MarkdownRenderer`.

The Microsoft DataIngestion packages are currently prerelease. This adapter is versioned as a prerelease and is intentionally distributed separately from the stable core packages.
