# Evaluation Guide

This guide describes how to evaluate conversion quality with `ElBruno.MarkItDotNet.Evals`.

## Package

```bash
dotnet add package ElBruno.MarkItDotNet.Evals
```

## Core API

- `IEvaluationEngine` — evaluates a `ConversionResult`
- `EvaluationReport` — score, issues, and metrics
- `EvaluationOptions.PassThreshold` — pass/fail cutoff

## Basic Usage

```csharp
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Evals;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddMarkItDotNet();
services.AddMarkItDotNetEvals(options => options.PassThreshold = 0.70);

using var provider = services.BuildServiceProvider();
var converter = provider.GetRequiredService<MarkdownService>();
var evaluator = provider.GetRequiredService<IEvaluationEngine>();
var options = provider.GetRequiredService<EvaluationOptions>();

using var stream = File.OpenRead("report.json");
var result = await converter.ConvertAsync(stream, ".json");

var sourceText = await File.ReadAllTextAsync("report.json");
var report = evaluator.Evaluate(result, sourceText);

var status = report.Passes(options.PassThreshold) ? "PASS" : "FAIL";
Console.WriteLine($"{status} score={report.Score:F2}");
```

## Understanding Report Metrics

`EvaluationReport.Metrics` includes:

- `contentLength` — size of converted markdown
- `headingDensity` — normalized heading richness score
- `retentionRatio` — rough token overlap against source text
- `passThreshold` — configured threshold used by the sample

## Benchmark Sample

Run the strategy-comparison benchmark sample:

```bash
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj
```

Dry-run mode:

```bash
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj -- --dry-run
```

The sample runs multiple input strategies per scenario and reports:

- Pass/fail and score
- Duration in milliseconds
- Memory delta in bytes
- Retention, heading density, and content length metrics

### Exportable Reports

By default, reports are exported to JSON and CSV in the sample `output/` folder.

```bash
# Custom threshold + output path
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj -- --threshold 0.8 --output ./benchmark-output

# Disable one format if needed
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj -- --no-csv
```

Configuration defaults are defined in `src/samples/EvaluationDemo/appsettings.json` and can be overridden from the command line.
