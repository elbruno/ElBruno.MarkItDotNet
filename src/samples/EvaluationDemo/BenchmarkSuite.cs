using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Evals;

namespace EvaluationDemoSample;

public sealed record EvaluationScenario(string Name, string Extension, string SourceText);

public sealed record StrategyResult(
    string Scenario,
    string Strategy,
    bool Passed,
    double Score,
    int IssueCount,
    double RetentionRatio,
    double HeadingDensity,
    double ContentLength,
    double DurationMs,
    long MemoryDeltaBytes);

public sealed record BenchmarkReport(
    DateTimeOffset GeneratedAtUtc,
    double PassThreshold,
    IReadOnlyCollection<StrategyResult> Results,
    string RecommendedStrategy);

public interface IEvaluationInputStrategy
{
    string Name { get; }
    string Transform(string sourceText, string extension);
}

public sealed class BaselineStrategy : IEvaluationInputStrategy
{
    public string Name => "baseline";
    public string Transform(string sourceText, string extension) => sourceText;
}

public sealed class WhitespaceNormalizedStrategy : IEvaluationInputStrategy
{
    public string Name => "whitespace-normalized";
    public string Transform(string sourceText, string extension)
    {
        var lines = sourceText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0);
        return string.Join(Environment.NewLine, lines);
    }
}

public sealed class HeadingBoostStrategy : IEvaluationInputStrategy
{
    public string Name => "heading-boost";
    public string Transform(string sourceText, string extension)
    {
        if (sourceText.TrimStart().StartsWith("#", StringComparison.Ordinal))
        {
            return sourceText;
        }

        return $"# Imported {extension.TrimStart('.')}{Environment.NewLine}{Environment.NewLine}{sourceText}";
    }
}

public static class BenchmarkSuiteRunner
{
    public static async Task<BenchmarkReport> RunAsync(
        IReadOnlyCollection<EvaluationScenario> scenarios,
        IReadOnlyCollection<IEvaluationInputStrategy> strategies,
        MarkdownService markdownService,
        IEvaluationEngine evaluationEngine,
        double passThreshold)
    {
        var results = new List<StrategyResult>();

        foreach (var scenario in scenarios)
        {
            foreach (var strategy in strategies)
            {
                var transformed = strategy.Transform(scenario.SourceText, scenario.Extension);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(transformed));
                var beforeBytes = GC.GetTotalMemory(forceFullCollection: false);
                var timer = Stopwatch.StartNew();
                var conversion = await markdownService.ConvertAsync(stream, scenario.Extension);
                var report = evaluationEngine.Evaluate(conversion, transformed);
                timer.Stop();
                var afterBytes = GC.GetTotalMemory(forceFullCollection: false);

                report.Metrics.TryGetValue("retentionRatio", out var retentionRatio);
                report.Metrics.TryGetValue("headingDensity", out var headingDensity);
                report.Metrics.TryGetValue("contentLength", out var contentLength);

                results.Add(
                    new StrategyResult(
                        Scenario: scenario.Name,
                        Strategy: strategy.Name,
                        Passed: report.Passes(passThreshold),
                        Score: report.Score,
                        IssueCount: report.Issues.Count,
                        RetentionRatio: retentionRatio,
                        HeadingDensity: headingDensity,
                        ContentLength: contentLength,
                        DurationMs: timer.Elapsed.TotalMilliseconds,
                        MemoryDeltaBytes: afterBytes - beforeBytes));
            }
        }

        var recommended = results
            .GroupBy(item => item.Strategy)
            .OrderByDescending(group => group.Average(item => item.Score))
            .ThenBy(group => group.Average(item => item.DurationMs))
            .Select(group => group.Key)
            .FirstOrDefault() ?? "n/a";

        return new BenchmarkReport(DateTimeOffset.UtcNow, passThreshold, results, recommended);
    }

    public static async Task ExportJsonAsync(BenchmarkReport report, string outputPath)
    {
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
    }

    public static async Task ExportCsvAsync(BenchmarkReport report, string outputPath)
    {
        var lines = new List<string>
        {
            "scenario,strategy,passed,score,issueCount,retentionRatio,headingDensity,contentLength,durationMs,memoryDeltaBytes"
        };

        lines.AddRange(
            report.Results.Select(result =>
                string.Join(
                    ",",
                    Escape(result.Scenario),
                    Escape(result.Strategy),
                    result.Passed ? "true" : "false",
                    result.Score.ToString("F4", System.Globalization.CultureInfo.InvariantCulture),
                    result.IssueCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    result.RetentionRatio.ToString("F4", System.Globalization.CultureInfo.InvariantCulture),
                    result.HeadingDensity.ToString("F4", System.Globalization.CultureInfo.InvariantCulture),
                    result.ContentLength.ToString("F0", System.Globalization.CultureInfo.InvariantCulture),
                    result.DurationMs.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    result.MemoryDeltaBytes.ToString(System.Globalization.CultureInfo.InvariantCulture))));

        await File.WriteAllLinesAsync(outputPath, lines, Encoding.UTF8);
    }

    private static string Escape(string text)
    {
        if (text.Contains(',') || text.Contains('"'))
        {
            return $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return text;
    }
}
