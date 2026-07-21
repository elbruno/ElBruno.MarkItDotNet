using System.Text;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.AzureSearch;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Connectors;
using ElBruno.MarkItDotNet.Evals;
using ElBruno.MarkItDotNet.Security;
using IngestionWorkflowSample;
using Microsoft.Extensions.DependencyInjection;

WorkflowOptions options;
try
{
    options = WorkflowOptionsParser.Parse(args, AppContext.BaseDirectory);
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

Console.WriteLine("=== Phase 3: Ingestion Workflow Demo ===");
Console.WriteLine($"Input path       : {options.InputPath}");
Console.WriteLine($"Output path      : {options.OutputPath}");
Console.WriteLine($"Mode             : {(options.DryRun ? "DRY-RUN" : "WRITE OUTPUTS")}");
Console.WriteLine($"Seed input       : {(options.SeedInput ? "YES" : "NO")}");
Console.WriteLine($"Eval threshold   : {options.PassThreshold:F2}");
Console.WriteLine($"Chunk strategy   : {options.ChunkingStrategy} (size={options.ChunkSize}, overlap={options.ChunkOverlap})");
Console.WriteLine($"Azure Search     : {(options.AzureSearch.Enabled ? "ENABLED" : "DISABLED")}");
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
services.AddMarkItDotNetSecurity();
services.AddMarkItDotNetEvals(eval => eval.PassThreshold = options.PassThreshold);
using var provider = services.BuildServiceProvider();

var markdownService = provider.GetRequiredService<MarkdownService>();
var securityScanner = provider.GetRequiredService<ISecurityScanner>();
var evaluationEngine = provider.GetRequiredService<IEvaluationEngine>();
var evaluationOptions = provider.GetRequiredService<EvaluationOptions>();

var source = new FileSystemConnector(
    new FileSystemConnectorOptions
    {
        RootPath = options.InputPath,
        Recursive = options.Recursive,
        MaxDepth = options.MaxDepth,
        IncludePatterns = options.IncludePatterns,
        MaxFileSizeBytes = options.MaxFileSizeBytes
    },
    new ConsoleLogger<FileSystemConnector>());

if (!await source.ValidateAsync())
{
    Console.WriteLine("Input source is not accessible.");
    return;
}

var discovered = await source.CountAsync();
Console.WriteLine($"Discovered {discovered} source document(s).");
Console.WriteLine();

var chunker = WorkflowPipeline.ResolveChunker(options.ChunkingStrategy);
var chunkingOptions = new ChunkingOptions
{
    MaxChunkSize = options.ChunkSize,
    OverlapSize = options.ChunkOverlap
};

var converted = 0;
var conversionFailed = 0;
var safeCount = 0;
var unsafeCount = 0;
var evalPass = 0;
var evalFail = 0;
var chunkCount = 0;
var citationCount = 0;
var uploadedCount = 0;
var uploadFailures = 0;

var uploadBuffer = new List<SearchDocument>();
ISearchDocumentMapper? searchMapper = null;
SearchIndexUploader? uploader = null;

if (options.AzureSearch.Enabled)
{
    if (string.IsNullOrWhiteSpace(options.AzureSearch.Endpoint) ||
        string.IsNullOrWhiteSpace(options.AzureSearch.IndexName))
    {
        Console.WriteLine("Azure Search is enabled but Endpoint/IndexName is missing. Upload stage will be skipped.");
    }
    else
    {
        var azureServices = new ServiceCollection();
        azureServices.AddMarkItDotNetAzureSearch(search =>
        {
            search.Endpoint = options.AzureSearch.Endpoint;
            search.IndexName = options.AzureSearch.IndexName;
            search.ApiKey = options.AzureSearch.ApiKey;
            search.BatchSize = options.AzureSearch.BatchSize;
            search.VectorDimensions = options.AzureSearch.VectorDimensions;
        });

        using var azureProvider = azureServices.BuildServiceProvider();
        searchMapper = azureProvider.GetRequiredService<ISearchDocumentMapper>();
        uploader = azureProvider.GetRequiredService<SearchIndexUploader>();

        if (!options.DryRun && options.AzureSearch.CreateIndexIfMissing)
        {
            var indexManager = azureProvider.GetRequiredService<SearchIndexManager>();
            var exists = await indexManager.IndexExistsAsync(options.AzureSearch.IndexName);
            if (!exists)
            {
                await indexManager.CreateOrUpdateIndexAsync(
                    options.AzureSearch.IndexName,
                    options.AzureSearch.VectorDimensions);
            }
        }
    }
}

await foreach (var document in source)
{
    await using var stream = await document.OpenReadAsync();
    var extension = document.Metadata.TryGetValue(SourceMetadataKeys.Extension, out var extValue)
        ? extValue
        : Path.GetExtension(document.Name);

    var conversion = await markdownService.ConvertAsync(stream, extension);
    if (!conversion.Success)
    {
        conversionFailed++;
        Console.WriteLine($"[CONVERT-FAIL] {document.Name}: {conversion.ErrorMessage}");
        continue;
    }

    converted++;
    var scan = securityScanner.Scan(conversion.Markdown);
    var report = evaluationEngine.Evaluate(conversion, conversion.Markdown);

    if (scan.IsSafe)
    {
        safeCount++;
    }
    else
    {
        unsafeCount++;
    }

    if (report.Passes(evaluationOptions.PassThreshold))
    {
        evalPass++;
    }
    else
    {
        evalFail++;
    }

    var relativePath = document.Metadata.TryGetValue(SourceMetadataKeys.RelativePath, out var relativePathValue)
        ? relativePathValue
        : document.Name;
    var outputPath = Path.Combine(options.OutputPath, $"{relativePath}.md");

    if (!options.DryRun)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, conversion.Markdown, Encoding.UTF8);
    }

    var documentId = Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(document.Source)))
        .ToLowerInvariant();
    var model = WorkflowPipeline.BuildDocument(documentId, document.Name, conversion.Markdown, document.Source);

    var chunks = chunker.Chunk(model, chunkingOptions);
    chunkCount += chunks.Count;

    var chunkInfos = WorkflowPipeline.ToChunkInfos(chunks);
    var citationSets = ElBruno.MarkItDotNet.Citations.CitationPropagator.PropagateToChunks(model, chunkInfos);
    citationCount += citationSets.Sum(set => set.Citations.Count);

    if (searchMapper is not null)
    {
        var mapped = WorkflowPipeline.MapSearchDocuments(chunks, citationSets, model, searchMapper);
        uploadBuffer.AddRange(mapped);
    }

    Console.WriteLine($"[OK] {document.Name}");
    Console.WriteLine($"  Output        : {(options.DryRun ? $"[DRY-RUN] {outputPath}" : outputPath)}");
    Console.WriteLine($"  Security      : {(scan.IsSafe ? "SAFE" : "UNSAFE")} (Score {scan.Score:F2}, Issues {scan.Issues.Count})");
    Console.WriteLine($"  Evaluation    : {(report.Passes(evaluationOptions.PassThreshold) ? "PASS" : "FAIL")} (Score {report.Score:F2})");
    Console.WriteLine($"  Chunking      : {chunks.Count} chunk(s) with '{chunker.Name}'");
    Console.WriteLine($"  Citations     : {citationSets.Sum(set => set.Citations.Count)} citation(s)");
    Console.WriteLine();
}

if (options.AzureSearch.Enabled && uploadBuffer.Count > 0)
{
    if (uploader is null)
    {
        uploadFailures += uploadBuffer.Count;
        Console.WriteLine($"[UPLOAD-SKIP] Azure Search uploader unavailable. Pending docs: {uploadBuffer.Count}");
    }
    else if (options.DryRun)
    {
        uploadedCount = uploadBuffer.Count;
        Console.WriteLine($"[UPLOAD-DRY-RUN] Would upload {uploadBuffer.Count} search document(s).");
    }
    else
    {
        var uploadResult = await uploader.UploadAsync(uploadBuffer, options.AzureSearch.BatchSize);
        uploadedCount = uploadResult.SuccessCount;
        uploadFailures = uploadResult.FailureCount;
        Console.WriteLine($"[UPLOAD] Uploaded={uploadResult.SuccessCount}, Failed={uploadResult.FailureCount}");
        foreach (var error in uploadResult.Errors)
        {
            Console.WriteLine($"  - {error}");
        }
    }
}

Console.WriteLine();
Console.WriteLine("=== Workflow Summary ===");
Console.WriteLine($"Converted        : {converted}");
Console.WriteLine($"Conversion fail  : {conversionFailed}");
Console.WriteLine($"Security safe    : {safeCount}");
Console.WriteLine($"Security unsafe  : {unsafeCount}");
Console.WriteLine($"Eval pass        : {evalPass}");
Console.WriteLine($"Eval fail        : {evalFail}");
Console.WriteLine($"Chunks generated : {chunkCount}");
Console.WriteLine($"Citations linked : {citationCount}");
Console.WriteLine($"Search uploaded  : {uploadedCount}");
Console.WriteLine($"Upload failures  : {uploadFailures}");

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/samples/IngestionWorkflow/IngestionWorkflow.csproj [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --input <path>                 Input folder (default from appsettings.json)");
    Console.WriteLine("  --output <path>                Output folder (default from appsettings.json)");
    Console.WriteLine("  --threshold <0..1>             Evaluation pass threshold");
    Console.WriteLine("  --max-depth <number>           Max folder traversal depth");
    Console.WriteLine("  --max-file-size <bytes>        Max file size accepted by connector");
    Console.WriteLine("  --pattern <glob>               Extra include pattern (repeatable)");
    Console.WriteLine("  --chunk-strategy <name>        Chunk strategy: heading | paragraph | token");
    Console.WriteLine("  --chunk-size <number>          Chunk max size");
    Console.WriteLine("  --chunk-overlap <number>       Chunk overlap size");
    Console.WriteLine("  --enable-azure-search          Enable Azure Search upload stage");
    Console.WriteLine("  --search-endpoint <url>        Azure Search endpoint");
    Console.WriteLine("  --search-index <name>          Azure Search index name");
    Console.WriteLine("  --search-api-key <key>         Azure Search API key (optional if MSI)");
    Console.WriteLine("  --dry-run                      Process without writing outputs or uploading");
    Console.WriteLine("  --skip-seed                    Skip creating demo input files");
    Console.WriteLine("  --help                         Show this help");
}

static void SeedInputFiles(string rootPath)
{
    var nested = Path.Combine(rootPath, "nested");
    Directory.CreateDirectory(nested);

    File.WriteAllText(
        Path.Combine(rootPath, "summary.txt"),
        """
        # Weekly Summary

        Connectors, security scan, and evaluation are all enabled in this workflow.
        """
    );

    File.WriteAllText(
        Path.Combine(rootPath, "metadata.json"),
        """
        {
          "title": "Ingestion Workflow",
          "phase": "Phase 3",
          "components": ["Connectors", "Security", "Evals"]
        }
        """
    );

    File.WriteAllText(
        Path.Combine(nested, "risk.html"),
        """
        <html><body>
          <h1>Risk Notes</h1>
          <p>Review links before publishing reports.</p>
          <a href="javascript:alert('xss')">dangerous link</a>
        </body></html>
        """
    );
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
