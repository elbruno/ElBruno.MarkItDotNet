using System.Text;
using ConnectorsDemoSample;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Connectors;
using ElBruno.MarkItDotNet.Connectors.AzureBlob;
using Microsoft.Extensions.DependencyInjection;

ConnectorDemoOptions options;
try
{
    options = ConnectorDemoOptionsParser.Parse(args, AppContext.BaseDirectory);
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine();
    PrintUsage();
    return;
}

if (options.ShowHelp)
{
    PrintUsage();
    return;
}

Console.WriteLine("=== Phase 3: Connectors Demo ===");
Console.WriteLine($"Input path  : {options.InputPath}");
Console.WriteLine($"Output path : {options.OutputPath}");
Console.WriteLine($"Mode        : {(options.DryRun ? "DRY-RUN" : "WRITE OUTPUTS")}");
Console.WriteLine($"Seed input  : {(options.SeedInput ? "YES" : "NO")}");
Console.WriteLine($"Azure Blob  : {(options.AzureBlob.Enabled ? "ENABLED (optional source)" : "DISABLED")}");
Console.WriteLine();

Directory.CreateDirectory(options.InputPath);
if (!options.DryRun)
{
    Directory.CreateDirectory(options.OutputPath);
}

if (options.SeedInput)
{
    SeedInputFiles(options.InputPath);
}

var services = new ServiceCollection();
services.AddMarkItDotNet();
using var provider = services.BuildServiceProvider();
var markdownService = provider.GetRequiredService<MarkdownService>();

var sources = new List<(string Name, IDocumentSource Source)>
{
    (
        "filesystem",
        new FileSystemConnector(
            new FileSystemConnectorOptions
            {
                RootPath = options.InputPath,
                Recursive = options.Recursive,
                MaxDepth = options.MaxDepth,
                IncludePatterns = options.IncludePatterns,
                MaxFileSizeBytes = options.MaxFileSizeBytes
            },
            new ConsoleLogger<FileSystemConnector>())
    )
};

if (options.AzureBlob.Enabled)
{
    if (string.IsNullOrWhiteSpace(options.AzureBlob.ContainerName))
    {
        Console.WriteLine("Azure Blob source was enabled, but no container name is configured.");
        Console.WriteLine("Use --azure-container <name> or set ConnectorsDemo:AzureBlob:ContainerName in appsettings.json.");
        return;
    }

    try
    {
        sources.Add(
            (
                "azureblob",
                new AzureBlobConnector(
                    new AzureBlobConnectorOptions
                    {
                        ConnectionString = options.AzureBlob.ConnectionString,
                        AccountName = options.AzureBlob.AccountName,
                        ServiceUri = options.AzureBlob.ServiceUri,
                        ContainerName = options.AzureBlob.ContainerName,
                        BlobPrefix = options.AzureBlob.BlobPrefix,
                        PageSize = options.AzureBlob.PageSize,
                        MaxRetries = options.AzureBlob.MaxRetries,
                        NetworkTimeout = options.AzureBlob.NetworkTimeoutSeconds.HasValue
                            ? TimeSpan.FromSeconds(options.AzureBlob.NetworkTimeoutSeconds.Value)
                            : null
                    },
                    new ConsoleLogger<AzureBlobConnector>())
            )
        );
    }
    catch (Exception ex) when (ex is InvalidOperationException or UriFormatException)
    {
        Console.WriteLine($"Azure Blob configuration is invalid: {ex.Message}");
        return;
    }
}

var discoveredBySource = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
var converted = 0;
var failed = 0;
var skippedSources = 0;

foreach (var (sourceName, source) in sources)
{
    if (!await source.ValidateAsync())
    {
        Console.WriteLine($"[WARN] Source '{sourceName}' is not accessible and will be skipped.");
        skippedSources++;
        continue;
    }

    var total = await source.CountAsync();
    discoveredBySource[sourceName] = total;
    Console.WriteLine($"Discovered {total} document(s) from {sourceName}.");

    await foreach (var sourceDocument in source)
    {
        await using var stream = await sourceDocument.OpenReadAsync();
        var extension = sourceDocument.Metadata.TryGetValue(SourceMetadataKeys.Extension, out var extValue)
            ? extValue
            : Path.GetExtension(sourceDocument.Name);

        var result = await markdownService.ConvertAsync(stream, extension);
        if (!result.Success)
        {
            failed++;
            Console.WriteLine($"[FAIL] [{sourceName}] {sourceDocument.Name}: {result.ErrorMessage}");
            continue;
        }

        var relativePath = sourceDocument.Metadata.TryGetValue(SourceMetadataKeys.RelativePath, out var relativeValue)
            ? relativeValue
            : sourceDocument.Name;
        var outputFile = $"{relativePath}.md";
        var outputPath = Path.Combine(options.OutputPath, sourceName, outputFile);

        if (!options.DryRun)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, result.Markdown, Encoding.UTF8);
        }

        converted++;
        Console.WriteLine($"[OK] [{sourceName}] {sourceDocument.Name} -> {(options.DryRun ? $"[DRY-RUN] {outputPath}" : outputPath)}");
    }
}

Console.WriteLine();
Console.WriteLine("=== Summary ===");
foreach (var (sourceName, count) in discoveredBySource)
{
    Console.WriteLine($"Discovered ({sourceName}) : {count}");
}

Console.WriteLine($"Converted                 : {converted}");
Console.WriteLine($"Failed                    : {failed}");
Console.WriteLine($"Skipped sources           : {skippedSources}");
Console.WriteLine($"Output root               : {options.OutputPath}");

static void SeedInputFiles(string rootPath)
{
    var nested = Path.Combine(rootPath, "nested");
    Directory.CreateDirectory(nested);

    File.WriteAllText(
        Path.Combine(rootPath, "sample.txt"),
        "This is a sample text document for connector ingestion.");

    File.WriteAllText(
        Path.Combine(rootPath, "sample.json"),
        """
        {
          "title": "Connector Demo",
          "phase": "Phase 3",
          "formats": ["txt", "json", "html"]
        }
        """);

    File.WriteAllText(
        Path.Combine(nested, "sample.html"),
        """
        <html><body><h1>Connector Demo</h1><p>Nested HTML document.</p></body></html>
        """);
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/samples/ConnectorsDemo/ConnectorsDemo.csproj [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --input <path>                  Input folder for FileSystemConnector");
    Console.WriteLine("  --output <path>                 Output folder for generated markdown");
    Console.WriteLine("  --pattern <glob>                Extra filesystem include pattern (repeatable)");
    Console.WriteLine("  --max-depth <number>            Max filesystem traversal depth");
    Console.WriteLine("  --max-file-size <bytes>         Max filesystem file size");
    Console.WriteLine("  --dry-run                       Process without writing output files");
    Console.WriteLine("  --skip-seed                     Skip creating demo local input files");
    Console.WriteLine("  --enable-azure                  Enable optional Azure Blob source");
    Console.WriteLine("  --azure-container <name>        Azure Blob container name");
    Console.WriteLine("  --azure-connection-string <cs>  Azure Blob connection string");
    Console.WriteLine("  --azure-account <name>          Azure Storage account name");
    Console.WriteLine("  --azure-service-uri <uri>       Azure Blob service URI");
    Console.WriteLine("  --azure-prefix <prefix>         Blob prefix filter");
    Console.WriteLine("  --help                          Show this help");
}

internal sealed class ConsoleLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        Microsoft.Extensions.Logging.EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (logLevel < Microsoft.Extensions.Logging.LogLevel.Warning)
        {
            return;
        }

        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose()
        {
        }
    }
}
