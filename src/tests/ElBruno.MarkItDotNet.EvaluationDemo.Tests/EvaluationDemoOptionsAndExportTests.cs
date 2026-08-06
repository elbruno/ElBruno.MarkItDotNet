using EvaluationDemoSample;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.EvaluationDemo.Tests;

public class EvaluationDemoOptionsAndExportTests
{
    [Fact]
    public void Parse_LoadsDefaultsFromAppSettings()
    {
        using var fixture = new TempDirectoryFixture();
        File.WriteAllText(
            Path.Combine(fixture.Path, "appsettings.json"),
            """
            {
              "EvaluationDemo": {
                "PassThreshold": 0.82,
                "DryRun": true,
                "ExportJson": true,
                "ExportCsv": false,
                "OutputPath": "bench-output",
                "JsonFileName": "result.json"
              }
            }
            """
        );

        var options = EvaluationDemoOptionsParser.Parse([], fixture.Path);

        options.PassThreshold.Should().BeApproximately(0.82, 0.001);
        options.DryRun.Should().BeTrue();
        options.ExportJson.Should().BeTrue();
        options.ExportCsv.Should().BeFalse();
        options.OutputPath.Should().Be(Path.GetFullPath("bench-output", fixture.Path));
        options.JsonFileName.Should().Be("result.json");
    }

    [Fact]
    public void Parse_AppliesCommandLineOverrides()
    {
        using var fixture = new TempDirectoryFixture();

        var options = EvaluationDemoOptionsParser.Parse(
            ["--threshold", "0.91", "--output", "cli-out", "--no-csv", "--dry-run"],
            fixture.Path);

        options.PassThreshold.Should().BeApproximately(0.91, 0.001);
        options.OutputPath.Should().Be(Path.GetFullPath("cli-out", fixture.Path));
        options.ExportCsv.Should().BeFalse();
        options.DryRun.Should().BeTrue();
    }

    [Fact]
    public async Task ExportCsv_WritesHeaderAndRows()
    {
        using var fixture = new TempDirectoryFixture();
        var report = new BenchmarkReport(
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            PassThreshold: 0.7,
            Results:
            [
                new StrategyResult("Scenario A", "baseline", true, 0.9, 0, 0.85, 0.3, 100, 12.4, 2048),
                new StrategyResult("Scenario A", "heading-boost", false, 0.6, 2, 0.45, 0.1, 98, 10.2, 1024)
            ],
            RecommendedStrategy: "baseline");

        var csvPath = Path.Combine(fixture.Path, "report.csv");
        await BenchmarkSuiteRunner.ExportCsvAsync(report, csvPath);

        var lines = await File.ReadAllLinesAsync(csvPath);
        lines.Should().HaveCount(3);
        lines[0].Should().Contain("scenario,strategy,passed,score");
        lines[1].Should().Contain("Scenario A");
    }

    private sealed class TempDirectoryFixture : IDisposable
    {
        public TempDirectoryFixture()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
