using FluentAssertions;
using IngestionWorkflowSample;
using Xunit;

namespace ElBruno.MarkItDotNet.IngestionWorkflow.Tests;

public class WorkflowOptionsParserTests
{
    [Fact]
    public void Parse_LoadsDefaultsFromAppSettings()
    {
        using var fixture = new TempDirectoryFixture();
        File.WriteAllText(
            Path.Combine(fixture.Path, "appsettings.json"),
            """
            {
              "IngestionWorkflow": {
                "InputPath": "seed-input",
                "OutputPath": "seed-output",
                "MaxDepth": 4,
                "PassThreshold": 0.81,
                "DryRun": true,
                "SeedInput": false,
                "ChunkingStrategy": "token",
                "ChunkSize": 256,
                "ChunkOverlap": 24,
                "AzureSearch": {
                  "Enabled": true,
                  "Endpoint": "https://sample.search.windows.net",
                  "IndexName": "ingestion-index",
                  "BatchSize": 50
                }
              }
            }
            """
        );

        var options = WorkflowOptionsParser.Parse([], fixture.Path);

        options.InputPath.Should().Be(Path.GetFullPath("seed-input", fixture.Path));
        options.OutputPath.Should().Be(Path.GetFullPath("seed-output", fixture.Path));
        options.MaxDepth.Should().Be(4);
        options.PassThreshold.Should().BeApproximately(0.81, 0.001);
        options.DryRun.Should().BeTrue();
        options.SeedInput.Should().BeFalse();
        options.ChunkingStrategy.Should().Be("token");
        options.ChunkSize.Should().Be(256);
        options.ChunkOverlap.Should().Be(24);
        options.AzureSearch.Enabled.Should().BeTrue();
        options.AzureSearch.Endpoint.Should().Be("https://sample.search.windows.net");
        options.AzureSearch.IndexName.Should().Be("ingestion-index");
        options.AzureSearch.BatchSize.Should().Be(50);
    }

    [Fact]
    public void Parse_AppliesCommandLineOverrides()
    {
        using var fixture = new TempDirectoryFixture();
        File.WriteAllText(
            Path.Combine(fixture.Path, "appsettings.json"),
            """
            {
              "IngestionWorkflow": {
                "InputPath": "default-input",
                "OutputPath": "default-output",
                "PassThreshold": 0.70
              }
            }
            """
        );

        var options = WorkflowOptionsParser.Parse(
            [
                "--input", "cli-input",
                "--output=cli-output",
                "--threshold", "0.92",
                "--dry-run",
                "--skip-seed",
                "--pattern", "*.md",
                "--chunk-strategy", "heading",
                "--chunk-size", "300",
                "--chunk-overlap", "40",
                "--enable-azure-search",
                "--search-endpoint", "https://cli.search.windows.net",
                "--search-index", "cli-index"
            ],
            fixture.Path
        );

        options.InputPath.Should().Be(Path.GetFullPath("cli-input", fixture.Path));
        options.OutputPath.Should().Be(Path.GetFullPath("cli-output", fixture.Path));
        options.PassThreshold.Should().BeApproximately(0.92, 0.001);
        options.DryRun.Should().BeTrue();
        options.SeedInput.Should().BeFalse();
        options.IncludePatterns.Should().Contain("*.md");
        options.ChunkingStrategy.Should().Be("heading");
        options.ChunkSize.Should().Be(300);
        options.ChunkOverlap.Should().Be(40);
        options.AzureSearch.Enabled.Should().BeTrue();
        options.AzureSearch.Endpoint.Should().Be("https://cli.search.windows.net");
        options.AzureSearch.IndexName.Should().Be("cli-index");
    }

    [Fact]
    public void Parse_ThrowsForInvalidThreshold()
    {
        using var fixture = new TempDirectoryFixture();

        Action act = () => WorkflowOptionsParser.Parse(["--threshold", "1.5"], fixture.Path);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*--threshold must be between 0.0 and 1.0*");
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
