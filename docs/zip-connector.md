# ZIP Archive Connector

`ZipArchiveConnector` exposes files in a ZIP archive through the `IDocumentSource` contract. It reads each entry on demand and does not extract archive contents to disk.

Install the connectors package:

```bash
dotnet add package ElBruno.MarkItDotNet.Connectors
```

## Dependency injection

```csharp
services.AddZipArchiveConnector(options =>
{
    options.ArchivePath = "documents.zip";
    options.MaxArchiveSizeBytes = 100 * 1024 * 1024;
    options.MaxEntrySizeBytes = 25 * 1024 * 1024;
    options.MaxTotalUncompressedBytes = 250 * 1024 * 1024;
    options.MaxEntries = 10_000;
});

var source = serviceProvider.GetRequiredService<IDocumentSource>();
await foreach (var document in source.GetDocumentsAsync(cancellationToken))
{
    await using var content = await document.OpenReadAsync(cancellationToken);
    // Process the entry stream.
}
```

The same connector can be constructed directly with `ZipArchiveConnectorOptions` and an `ILogger<ZipArchiveConnector>`.

## Limits and behavior

The defaults cap the compressed archive at 100 MiB, a single uncompressed entry at 25 MiB, the sum of discovered uncompressed entries at 250 MiB, and the archive at 10,000 entries. An entry larger than `MaxEntrySizeBytes` is skipped with a warning. Archives that exceed the compressed-size, entry-count, or total uncompressed-size limits are rejected. Reads are also capped at `MaxEntrySizeBytes` in case an entry expands beyond its declared size.

Directory entries are not returned. Each `SourceDocument` includes archive path and entry path metadata, and opening its content keeps the archive stream alive until the returned stream is disposed.
