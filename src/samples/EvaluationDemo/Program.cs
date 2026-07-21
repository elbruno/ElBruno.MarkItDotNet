using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Evals;
using EvaluationDemoSample;
using Microsoft.Extensions.DependencyInjection;

EvaluationDemoOptions options;
try
{
    options = EvaluationDemoOptionsParser.Parse(args, AppContext.BaseDirectory);
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

Console.WriteLine("=== Phase 3: Evaluation Benchmark Demo ===");
Console.WriteLine($"Threshold : {options.PassThreshold:F2}");
Console.WriteLine($"Mode      : {(options.DryRun ? "DRY-RUN" : "WRITE REPORTS")}");
Console.WriteLine($"Output    : {options.OutputPath}");
Console.WriteLine();

if (!options.DryRun)
{
    Directory.CreateDirectory(options.OutputPath);
}

var services = new ServiceCollection();
services.AddMarkItDotNet();
services.AddMarkItDotNetEvals(eval => eval.PassThreshold = options.PassThreshold);
using var provider = services.BuildServiceProvider();

var markdownService = provider.GetRequiredService<MarkdownService>();
var evaluationEngine = provider.GetRequiredService<IEvaluationEngine>();

var scenarios = BuildScenarios();
var strategies = new IEvaluationInputStrategy[]
{
    new BaselineStrategy(),
    new WhitespaceNormalizedStrategy(),
    new HeadingBoostStrategy()
};

var report = await BenchmarkSuiteRunner.RunAsync(
    scenarios,
    strategies,
    markdownService,
    evaluationEngine,
    options.PassThreshold);

foreach (var scenarioGroup in report.Results.GroupBy(result => result.Scenario))
{
    Console.WriteLine($"Scenario: {scenarioGroup.Key}");
    foreach (var result in scenarioGroup.OrderBy(item => item.Strategy))
    {
        Console.WriteLine(
            $"  [{(result.Passed ? "PASS" : "FAIL")}] {result.Strategy,-23} " +
            $"Score={result.Score:F2} Duration={result.DurationMs:F2}ms MemoryDelta={result.MemoryDeltaBytes}B");
    }

    Console.WriteLine();
}

Console.WriteLine($"Recommended strategy: {report.RecommendedStrategy}");

var jsonPath = Path.Combine(options.OutputPath, options.JsonFileName);
var csvPath = Path.Combine(options.OutputPath, options.CsvFileName);

if (!options.DryRun)
{
    if (options.ExportJson)
    {
        await BenchmarkSuiteRunner.ExportJsonAsync(report, jsonPath);
    }

    if (options.ExportCsv)
    {
        await BenchmarkSuiteRunner.ExportCsvAsync(report, csvPath);
    }
}

Console.WriteLine($"JSON report: {(options.ExportJson ? (options.DryRun ? $"[DRY-RUN] {jsonPath}" : jsonPath) : "disabled")}");
Console.WriteLine($"CSV report : {(options.ExportCsv ? (options.DryRun ? $"[DRY-RUN] {csvPath}" : csvPath) : "disabled")}");

static IReadOnlyCollection<EvaluationScenario> BuildScenarios()
{
    return
    [
        new EvaluationScenario(
            Name: "Structured Markdown Input",
            Extension: ".md",
            SourceText:
            """
            # Product Update

            This sprint delivered connector support and test coverage improvements.

            ## Metrics
            - 23 connector tests passing
            - Full net8.0 suite green
            """
        ),
        new EvaluationScenario(
            Name: "JSON Metadata Document",
            Extension: ".json",
            SourceText:
            """
            {
              "title": "Connector Rollout",
              "phase": "Phase 3",
              "items": [
                "FileSystemConnector",
                "AzureBlobConnector",
                "EvaluationDemo"
              ]
            }
            """
        ),
        new EvaluationScenario(
            Name: "HTML Brief",
            Extension: ".html",
            SourceText:
            """
            <html><body>
            <h1>Release Notes</h1>
            <p>Connectors and evaluation samples are now included.</p>
            </body></html>
            """
        )
    ];
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --threshold <0..1>      Evaluation pass threshold");
    Console.WriteLine("  --output <path>         Output folder for benchmark reports");
    Console.WriteLine("  --no-json               Disable JSON export");
    Console.WriteLine("  --no-csv                Disable CSV export");
    Console.WriteLine("  --dry-run               Run benchmark without writing report files");
    Console.WriteLine("  --help                  Show this help");
}
