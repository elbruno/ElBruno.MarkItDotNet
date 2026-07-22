using System.Xml.Linq;
using Xunit;

namespace ElBruno.MarkItDotNet.Cli.Tests;

public sealed class ToolShimTests
{
    private static readonly string CsprojPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "ElBruno.MarkItDotNet.Cli", "ElBruno.MarkItDotNet.Cli.csproj"));

    private static string LoadToolCommandName()
    {
        var doc = XDocument.Load(CsprojPath);
        var element = doc.Descendants("ToolCommandName").FirstOrDefault()
            ?? throw new InvalidOperationException($"<ToolCommandName> element not found in {CsprojPath}");
        return element.Value;
    }

    [Fact]
    public void ToolCommandName_Is_MarkitdownDotnet()
    {
        var toolCommandName = LoadToolCommandName();
        Assert.True(
            toolCommandName == "markitdown-dotnet",
            $"Expected ToolCommandName to be 'markitdown-dotnet' but found '{toolCommandName}' in {CsprojPath}");
    }

    [Fact]
    public void ToolCommandName_Does_Not_Conflict_With_Python_Markitdown()
    {
        var toolCommandName = LoadToolCommandName();
        Assert.False(
            toolCommandName == "markitdown",
            "ToolCommandName must not be 'markitdown' — it conflicts with the Microsoft Python markitdown CLI (https://github.com/microsoft/markitdown)");
    }
}
