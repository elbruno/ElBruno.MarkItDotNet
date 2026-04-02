using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class JsonConverterTests
{
    private readonly JsonConverter _converter = new();

    [Fact]
    public void CanHandle_Json_ReturnsTrue()
    {
        _converter.CanHandle(".json").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".JSON").Should().BeTrue();
        _converter.CanHandle(".Json").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".html")]
    [InlineData(".xml")]
    [InlineData(".csv")]
    public void CanHandle_NonJsonExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_SimpleObject_WrapsInJsonCodeBlock()
    {
        var json = """{"name":"test","value":42}""";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var result = await _converter.ConvertAsync(stream, ".json");

        result.Should().StartWith("```json\n");
        result.Should().EndWith("\n```");
        result.Should().Contain("\"name\"");
        result.Should().Contain("\"value\"");
    }

    [Fact]
    public async Task ConvertAsync_PrettyPrintsCompactJson()
    {
        var json = """{"a":"b","c":"d"}""";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var result = await _converter.ConvertAsync(stream, ".json");

        // Pretty-printed JSON should have indentation
        result.Should().Contain("  ");
    }

    [Fact]
    public async Task ConvertAsync_EmptyContent_ReturnsEmpty()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".json");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_WhitespaceOnly_ReturnsEmpty()
    {
        using var stream = new MemoryStream("   \n  "u8.ToArray());

        var result = await _converter.ConvertAsync(stream, ".json");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_Array_WrapsInCodeBlock()
    {
        var json = """[1, 2, 3]""";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var result = await _converter.ConvertAsync(stream, ".json");

        result.Should().StartWith("```json\n");
        result.Should().Contain("1");
        result.Should().Contain("2");
        result.Should().Contain("3");
    }

    [Fact]
    public async Task ConvertAsync_NestedObject_PreservesStructure()
    {
        var json = """{"outer":{"inner":"value"}}""";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var result = await _converter.ConvertAsync(stream, ".json");

        result.Should().Contain("outer");
        result.Should().Contain("inner");
        result.Should().Contain("value");
    }

    [Fact]
    public async Task ConvertAsync_InvalidJson_ReturnsNoteWithRawContent()
    {
        var invalidJson = "not valid json {{{";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));

        var result = await _converter.ConvertAsync(stream, ".json");

        result.Should().Contain("not valid json");
        result.Should().Contain("```");
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".json");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertAsync_WithTestDataFile()
    {
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.json");
        if (!File.Exists(testDataPath))
            return;

        using var stream = File.OpenRead(testDataPath);
        var result = await _converter.ConvertAsync(stream, ".json");

        result.Should().StartWith("```json\n");
        result.Should().Contain("MarkItDotNet");
        result.Should().EndWith("\n```");
    }
}
