# Ingestion Workflows

This document describes how to compose a full Phase 3 ingestion flow with:

- Source discovery (`IDocumentSource`)
- Markdown conversion (`MarkdownService`)
- Security scanning (`ISecurityScanner`)
- Conversion quality evaluation (`IEvaluationEngine`)
- Chunking (`IChunkingStrategy`)
- Citation propagation (`CitationPropagator`)
- Optional Azure Search upload (`SearchIndexUploader`)

## Reference Sample

Run the complete workflow sample:

```bash
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj
```

Run in dry-run mode (processes documents without writing outputs):

```bash
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --dry-run
```

The sample:

1. Seeds input files (`txt`, `json`, `html`)
2. Enumerates files via `FileSystemConnector`
3. Converts each file to Markdown
4. Runs `MarkdownSecurityScanner` to detect risky patterns
5. Runs `ConversionEvaluationEngine` to score output quality
6. Builds CoreModel documents and chunks with a configurable strategy
7. Propagates citations from chunk sources
8. Optionally maps/uploads chunks to Azure AI Search
9. Writes markdown outputs and prints a final pipeline summary

## Configuration

Default values live in `src/samples/IngestionWorkflow/appsettings.json` under the `IngestionWorkflow` section:

- `InputPath`
- `OutputPath`
- `IncludePatterns`
- `MaxDepth`
- `MaxFileSizeBytes`
- `PassThreshold`
- `DryRun`
- `SeedInput`
- `ChunkingStrategy` (`heading`, `paragraph`, `token`)
- `ChunkSize`
- `ChunkOverlap`
- `AzureSearch` (`Enabled`, `Endpoint`, `IndexName`, `ApiKey`, `BatchSize`, `VectorDimensions`, `CreateIndexIfMissing`)

Any value can be overridden with CLI options:

```bash
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --input ./my-input --output ./my-output --threshold 0.8 --skip-seed
```

Enable Azure Search stage:

```bash
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --enable-azure-search --search-endpoint https://mysearch.search.windows.net --search-index ingestion-index
```

## Composition Pattern

```csharp
var services = new ServiceCollection();
services.AddMarkItDotNet();
services.AddMarkItDotNetSecurity();
services.AddMarkItDotNetEvals(options => options.PassThreshold = 0.70);
services.AddMarkItDotNetChunking(options => options.MaxChunkSize = 512);
services.AddMarkItDotNetAzureSearch(options =>
{
    options.Endpoint = "https://mysearch.search.windows.net";
    options.IndexName = "ingestion-index";
});

using var provider = services.BuildServiceProvider();
var converter = provider.GetRequiredService<MarkdownService>();
var scanner = provider.GetRequiredService<ISecurityScanner>();
var evaluator = provider.GetRequiredService<IEvaluationEngine>();
var chunker = provider.GetRequiredService<IChunkingStrategy>();
var uploader = provider.GetRequiredService<SearchIndexUploader>();
```

Use this pattern as a baseline for production ingestion workers and background processors.

## Troubleshooting

- **"Input source is not accessible."**: verify `InputPath` exists and is readable.
- **No files discovered**: check `IncludePatterns`, recursion settings, and max depth.
- **All evaluations fail**: lower `PassThreshold` or review Markdown quality issues shown per file.
- **Unexpected writes during testing**: use `--dry-run` to disable output file creation.
- **Azure upload skipped**: ensure `AzureSearch.Enabled=true` and both endpoint/index are configured.
