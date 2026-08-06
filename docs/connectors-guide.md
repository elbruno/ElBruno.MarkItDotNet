# Connectors Guide

This guide covers the Phase 3 connectors foundation used for ingestion workflows.

## Packages

- `ElBruno.MarkItDotNet.Connectors`
- `ElBruno.MarkItDotNet.Connectors.AzureBlob`

Install:

```bash
dotnet add package ElBruno.MarkItDotNet.Connectors
dotnet add package ElBruno.MarkItDotNet.Connectors.AzureBlob
```

## Core Contract

All connectors implement `IDocumentSource`:

- Async enumeration via `IAsyncEnumerable<SourceDocument>`
- `ValidateAsync()` to verify source accessibility
- `CountAsync()` to count discoverable documents

`SourceDocument` provides:

- Stable identity and source location
- Metadata dictionary
- Deferred content stream access via `OpenReadAsync()`

## File System Connector

```csharp
using ElBruno.MarkItDotNet.Connectors;

var source = new FileSystemConnector(new FileSystemConnectorOptions
{
    RootPath = @"C:\Documents",
    Recursive = true,
    MaxDepth = 3,
    IncludePatterns = ["*.pdf", "*.docx"],
    MaxFileSizeBytes = 50_000_000
}, logger);

if (await source.ValidateAsync())
{
    var total = await source.CountAsync();
    await foreach (var doc in source)
    {
        await using var stream = await doc.OpenReadAsync();
        // Convert stream to markdown
    }
}
```

## Azure Blob Connector

```csharp
using ElBruno.MarkItDotNet.Connectors.AzureBlob;

var source = new AzureBlobConnector(new AzureBlobConnectorOptions
{
    AccountName = "mystorageaccount",
    ContainerName = "documents",
    BlobPrefix = "incoming/",
    PageSize = 100,
    MaxRetries = 3,
    NetworkTimeout = TimeSpan.FromSeconds(30)
}, logger);

if (await source.ValidateAsync())
{
    var total = await source.CountAsync();
    await foreach (var doc in source)
    {
        await using var stream = await doc.OpenReadAsync();
        // Convert stream to markdown
    }
}
```

You can also provide `ConnectionString` or explicit `ServiceUri` instead of `AccountName`.

## Dependency Injection

```csharp
using ElBruno.MarkItDotNet.Connectors;
using ElBruno.MarkItDotNet.Connectors.AzureBlob;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMarkItDotNetConnectors()
    .AddFileSystemConnector(options =>
    {
        options.RootPath = @"C:\Documents";
        options.IncludePatterns = ["*.pdf"];
    })
    .AddAzureBlobConnector(options =>
    {
        options.AccountName = "mystorageaccount";
        options.ContainerName = "documents";
        options.BlobPrefix = "incoming/";
    });
```

When both are registered, resolve `IEnumerable<IDocumentSource>` to process all configured sources.

## Sample

Run the end-to-end connectors sample:

```bash
dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj
```

The sample supports multi-source ingestion:

1. `FileSystemConnector` is always enabled.
2. `AzureBlobConnector` can be enabled from `appsettings.json` or CLI.
3. Output is grouped per source (`output/filesystem` and `output/azureblob`).

Dry-run mode:

```bash
dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj -- --dry-run
```

Enable Azure Blob source:

```bash
dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj -- --enable-azure --azure-container documents --azure-account mystorageaccount
```

Defaults are configured in `src/samples/ConnectorsDemo/appsettings.json` (`ConnectorsDemo` section). CLI arguments override configuration values.
