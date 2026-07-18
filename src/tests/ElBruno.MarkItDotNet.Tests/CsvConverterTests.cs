using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class CsvConverterTests
{
    private readonly CsvConverter _converter = new();

    [Fact]
    public void CanHandle_Csv_ReturnsTrue()
    {
        _converter.CanHandle(".csv").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_Tsv_ReturnsTrue()
    {
        _converter.CanHandle(".tsv").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".CSV").Should().BeTrue();
        _converter.CanHandle(".TSV").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".json")]
    [InlineData(".xml")]
    public void CanHandle_NonCsvExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_SimpleCsv_ProducesMarkdownTable()
    {
        var csv = "Name,Age,City\nAlice,30,Seattle\nBob,25,Portland";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _converter.ConvertAsync(stream, ".csv");

        result.Should().Contain("| Name | Age | City |");
        result.Should().Contain("| --- | --- | --- |");
        result.Should().Contain("| Alice | 30 | Seattle |");
        result.Should().Contain("| Bob | 25 | Portland |");
    }

    [Fact]
    public async Task ConvertAsync_TsvFile_UsesTabSeparator()
    {
        var tsv = "Name\tAge\nAlice\t30";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tsv));

        var result = await _converter.ConvertAsync(stream, ".tsv");

        result.Should().Contain("| Name | Age |");
        result.Should().Contain("| Alice | 30 |");
    }

    [Fact]
    public async Task ConvertAsync_TsvFile_KeepsCommaInsideField()
    {
        var tsv = "Name\tNotes\nAlice\tcat,dog";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tsv));

        var result = await _converter.ConvertAsync(stream, ".tsv");

        result.Should().Contain("| Alice | cat,dog |");
    }

    [Fact]
    public async Task ConvertAsync_TsvFile_EscapesPipes()
    {
        var tsv = "Name\tNotes\nAlice\tDev|Ops";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tsv));

        var result = await _converter.ConvertAsync(stream, ".tsv");

        result.Should().Contain("| Alice | Dev\\|Ops |");
    }

    [Fact]
    public async Task ConvertAsync_TsvFile_IgnoresBlankLines_AndPreservesEmptyCells()
    {
        var tsv = "Name\tAge\tCity\r\nAlice\t30\tSeattle\r\n\r\nBob\t\tPortland\r\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tsv));

        var result = await _converter.ConvertAsync(stream, ".tsv");

        result.Should().Contain("| Alice | 30 | Seattle |");
        result.Should().Contain("| Bob |  | Portland |");
    }

    [Fact]
    public async Task ConvertAsync_TsvFile_HeaderOnly_ProducesHeaderAndSeparator()
    {
        var tsv = "Name\tAge\tCity";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tsv));

        var result = await _converter.ConvertAsync(stream, ".tsv");

        var lines = result.Split(Environment.NewLine, StringSplitOptions.None);
        lines.Should().HaveCount(2);
        lines[0].Should().Be("| Name | Age | City |");
        lines[1].Should().Be("| --- | --- | --- |");
    }

    [Fact]
    public async Task ConvertAsync_TsvFile_RaggedRows_UseHeaderColumnCount()
    {
        var tsv = "Name\tAge\nAlice\nBob\t25\tExtra";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tsv));

        var result = await _converter.ConvertAsync(stream, ".tsv");

        result.Should().Contain("| Alice |  |");
        result.Should().Contain("| Bob | 25 |");
        result.Should().NotContain("Extra");
    }

    [Fact]
    public async Task ConvertAsync_TsvFile_HandlesCrlfAndLfLineEndings()
    {
        var tsv = "Name\tAge\r\nAlice\t30\nBob\t25\r\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tsv));

        var result = await _converter.ConvertAsync(stream, ".tsv");

        result.Should().Contain("| Alice | 30 |");
        result.Should().Contain("| Bob | 25 |");
    }

    [Fact]
    public async Task ConvertAsync_QuotedFieldsWithCommas_ParsedCorrectly()
    {
        var csv = "Name,Description\nAlice,\"Has a cat, dog\"";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _converter.ConvertAsync(stream, ".csv");

        result.Should().Contain("| Alice | Has a cat, dog |");
    }

    [Fact]
    public async Task ConvertAsync_EmptyContent_ReturnsEmpty()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".csv");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".csv");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
