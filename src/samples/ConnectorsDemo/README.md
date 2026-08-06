# ConnectorsDemo Sample

This sample demonstrates a multi-source ingestion flow:

1. File-system ingestion via `FileSystemConnector`
2. Optional Azure Blob ingestion via `AzureBlobConnector`
3. Markdown conversion with `MarkdownService`
4. Source-specific output folders (`output/filesystem`, `output/azureblob`)

## Run

```bash
dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj
```

## Useful options

```bash
# Dry-run (no markdown files written)
dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj -- --dry-run

# Enable Azure Blob source
dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj -- --enable-azure --azure-container documents --azure-account mystorageaccount

# Override local source path and output
dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj -- --input ./my-input --output ./my-output
```

Defaults are stored in `appsettings.json` (`ConnectorsDemo` section), and CLI options override those defaults.
