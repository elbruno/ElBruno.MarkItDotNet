using ConnectorsDemoSample;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.ConnectorsDemo.Tests;

public class ConnectorDemoOptionsParserTests
{
    [Fact]
    public void Parse_LoadsDefaultsFromAppSettings()
    {
        using var fixture = new TempDirectoryFixture();
        File.WriteAllText(
            Path.Combine(fixture.Path, "appsettings.json"),
            """
            {
              "ConnectorsDemo": {
                "InputPath": "fs-input",
                "OutputPath": "fs-output",
                "DryRun": true,
                "SeedInput": false,
                "AzureBlob": {
                  "Enabled": true,
                  "ContainerName": "docs",
                  "AccountName": "samplesa"
                }
              }
            }
            """
        );

        var options = ConnectorDemoOptionsParser.Parse([], fixture.Path);

        options.InputPath.Should().Be(Path.GetFullPath("fs-input", fixture.Path));
        options.OutputPath.Should().Be(Path.GetFullPath("fs-output", fixture.Path));
        options.DryRun.Should().BeTrue();
        options.SeedInput.Should().BeFalse();
        options.AzureBlob.Enabled.Should().BeTrue();
        options.AzureBlob.ContainerName.Should().Be("docs");
        options.AzureBlob.AccountName.Should().Be("samplesa");
    }

    [Fact]
    public void Parse_AppliesCommandLineOverrides()
    {
        using var fixture = new TempDirectoryFixture();

        var options = ConnectorDemoOptionsParser.Parse(
            [
                "--input", "cli-input",
                "--output=cli-output",
                "--dry-run",
                "--skip-seed",
                "--pattern", "*.md",
                "--enable-azure",
                "--azure-container", "container-a",
                "--azure-account", "account-a"
            ],
            fixture.Path);

        options.InputPath.Should().Be(Path.GetFullPath("cli-input", fixture.Path));
        options.OutputPath.Should().Be(Path.GetFullPath("cli-output", fixture.Path));
        options.DryRun.Should().BeTrue();
        options.SeedInput.Should().BeFalse();
        options.IncludePatterns.Should().Contain("*.md");
        options.AzureBlob.Enabled.Should().BeTrue();
        options.AzureBlob.ContainerName.Should().Be("container-a");
        options.AzureBlob.AccountName.Should().Be("account-a");
    }

    [Fact]
    public void Parse_ThrowsForInvalidMaxDepth()
    {
        using var fixture = new TempDirectoryFixture();

        Action act = () => ConnectorDemoOptionsParser.Parse(["--max-depth=-1"], fixture.Path);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*--max-depth must be greater than or equal to 0*");
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
