# EvaluationDemo Sample

This sample runs a benchmark-style comparison across multiple input strategies:

1. Baseline input
2. Whitespace-normalized input
3. Heading-boosted input

For each strategy and scenario it records:
- pass/fail and quality score
- latency (`DurationMs`)
- memory delta (`MemoryDeltaBytes`)
- core evaluation metrics (retention, heading density, content length)

It also exports benchmark reports to JSON and CSV.

## Run

```bash
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj
```

## Useful options

```bash
# Dry-run (no report files written)
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj -- --dry-run

# Override threshold and output folder
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj -- --threshold 0.8 --output ./benchmark-output

# Export only JSON
dotnet run --project src/samples/EvaluationDemo/EvaluationDemo.csproj -- --no-csv
```

Defaults are configured in `appsettings.json` (`EvaluationDemo` section), and CLI options override those defaults.
