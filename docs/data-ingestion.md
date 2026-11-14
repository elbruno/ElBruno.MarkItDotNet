# Microsoft.Extensions.DataIngestion adapter

`ElBruno.MarkItDotNet.DataIngestion` bridges structured MarkItDotNet CoreModel documents into Microsoft's `IngestionDocument` representation. It maps sections, headings, paragraphs, tables, and figures to native ingestion elements. Code, lists, quotes, and other block types are represented as Markdown paragraphs with their original Markdown and block type metadata retained.

Install the adapter package after its planned stable `0.9.4` release is published:

```bash
dotnet add package ElBruno.MarkItDotNet.DataIngestion --version 0.9.4
dotnet add package ElBruno.MarkItDotNet.Markdown
```

Prerelease builds remain available through the **Publish to NuGet** manual workflow with `channel: adapter-preview` and a version such as `0.9.3-preview.1`. This channel publishes only the adapter and its matching CoreModel prerelease. The stable adapter release is included in the stable package train. Both channels use the existing `publish.yml` trusted publisher identity.

The adapter depends only on `Microsoft.Extensions.DataIngestion.Abstractions`, not on Microsoft's Markdig reader or its pipeline utilities. This avoids duplicating Markdown parsing and keeps the prerelease dependency out of the CoreModel package.

## Dependency stability caveat

Although `ElBruno.MarkItDotNet.DataIngestion` `0.9.4` is a stable package version, its dependency graph is not fully stable: it depends on `Microsoft.Extensions.DataIngestion.Abstractions` `10.10.0-preview.1.26459.2`. The upstream `Microsoft.Extensions.DataIngestion.Abstractions` and `Microsoft.Extensions.DataIngestion` package feeds currently list prerelease versions only. Packing the stable adapter therefore emits NuGet warning `NU5104`, because a stable package depends on a prerelease package. Consumers also receive this prerelease dependency transitively.

The adapter's public `ToIngestionDocument` method returns Microsoft's `IngestionDocument` type and creates other Microsoft ingestion element types. The dependency is retained to preserve that public API; removing or replacing it would require a breaking contract change. Revisit this caveat when Microsoft publishes a stable compatible package.

If you want to parse Markdown first, install `ElBruno.MarkItDotNet.Markdown`, then convert the resulting document before passing it to a Microsoft ingestion chunker or processor:

```csharp
using ElBruno.MarkItDotNet.DataIngestion;
using ElBruno.MarkItDotNet.Markdown;

var model = MarkdownDocumentParser.Parse(markdown, "docs/guide.md");
var ingestionDocument = model.ToIngestionDocument();
```

The resulting identifier is the CoreModel document ID. Document-level and block-level metadata is retained with the `markitdotnet.` metadata prefix, including block IDs, source paths and spans, source format, and custom metadata. Original Markdown is retained where available; blocks created programmatically are rendered by `MarkdownRenderer`.

The Microsoft DataIngestion packages are currently prerelease. The adapter can be released with a stable package version, but its transitive dependency remains prerelease as described above.
