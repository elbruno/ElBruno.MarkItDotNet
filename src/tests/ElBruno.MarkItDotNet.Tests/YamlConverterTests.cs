using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class YamlConverterTests
{
    private readonly YamlConverter _converter = new();

    [Fact]
    public void CanHandle_Yaml_ReturnsTrue()
    {
        _converter.CanHandle(".yaml").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_Yml_ReturnsTrue()
    {
        _converter.CanHandle(".yml").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".YAML").Should().BeTrue();
        _converter.CanHandle(".YML").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".json")]
    [InlineData(".xml")]
    public void CanHandle_NonYamlExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_YamlContent_WrapsInFencedBlock()
    {
        var yaml = "name: test\nversion: 1.0\nitems:\n  - one\n  - two";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(yaml));

        var result = await _converter.ConvertAsync(stream, ".yaml");

        result.Should().StartWith("```yaml\n");
        result.Should().EndWith("\n```");
        result.Should().Contain("name: test");
        result.Should().Contain("version: 1.0");
    }

    [Fact]
    public async Task ConvertAsync_YmlExtension_WorksSameAsYaml()
    {
        var yaml = "key: value";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(yaml));

        var result = await _converter.ConvertAsync(stream, ".yml");

        result.Should().StartWith("```yaml\n");
        result.Should().Contain("key: value");
    }

    [Fact]
    public async Task ConvertAsync_EmptyContent_ReturnsEmpty()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".yaml");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".yaml");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
