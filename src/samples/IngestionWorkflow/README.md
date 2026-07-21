# IngestionWorkflow Sample

This sample demonstrates an end-to-end Phase 3 ingestion pipeline:

1. Discover files with `FileSystemConnector`
2. Convert content with `MarkdownService`
3. Run security scanning with `MarkdownSecurityScanner`
4. Evaluate conversion quality with `ConversionEvaluationEngine`
5. Build chunk sets with configurable chunking strategies
6. Generate citations linked to chunk origins
7. Optionally upload mapped chunk documents to Azure AI Search
8. Write markdown outputs (or simulate with dry-run)

## Run

```bash
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj
```

## Useful options

```bash
# Dry run (no output files written)
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --dry-run

# Override input/output folders and threshold
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --input ./my-input --output ./my-output --threshold 0.8

# Skip demo seeding when you already have input files
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --skip-seed

# Switch chunking strategy and size
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --chunk-strategy token --chunk-size 256 --chunk-overlap 20

# Enable Azure Search upload stage
dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj -- --enable-azure-search --search-endpoint https://mysearch.search.windows.net --search-index ingestion-index
```

## Configuration

Default values are loaded from `appsettings.json` in this sample folder (`IngestionWorkflow` section). Command-line options override those defaults.
