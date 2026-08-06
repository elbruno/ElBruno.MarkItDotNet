using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Cli.Tests;

public class FormatsCommandTests
{
    [Fact]
    public async Task Formats_ListsSupportedFormats()
    {
        var cli = new CliRunner();
        await cli.RunAsync("formats");

        cli.ExitCode.Should().Be(0);
        cli.Stdout.Should().Contain("Supported formats:");
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".html")]
    [InlineData(".docx")]
    [InlineData(".csv")]
    [InlineData(".tsv")]
    [InlineData(".xlsx")]
    [InlineData(".pptx")]
    public async Task Formats_IncludesExpectedExtension(string extension)
    {
        var cli = new CliRunner();
        await cli.RunAsync("formats");

        cli.ExitCode.Should().Be(0);
        cli.Stdout.Should().Contain(extension);
    }

    [Fact]
    public async Task Formats_IncludesMultipleConverters()
    {
        var cli = new CliRunner();
        await cli.RunAsync("formats");

        cli.ExitCode.Should().Be(0);

        // The output should list multiple converter type names
        var lines = cli.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // At least header + some converters
        lines.Length.Should().BeGreaterThanOrEqualTo(3);
    }
}
