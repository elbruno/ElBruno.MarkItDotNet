using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class XmlConverterTests
{
    private readonly XmlConverter _converter = new();

    [Fact]
    public void CanHandle_Xml_ReturnsTrue()
    {
        _converter.CanHandle(".xml").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".XML").Should().BeTrue();
        _converter.CanHandle(".Xml").Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".json")]
    [InlineData(".html")]
    public void CanHandle_NonXmlExtension_ReturnsFalse(string extension)
    {
        _converter.CanHandle(extension).Should().BeFalse();
    }

    [Fact]
    public async Task ConvertAsync_ValidXml_FormatsInCodeBlock()
    {
        var xml = "<root><item>Hello</item></root>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));

        var result = await _converter.ConvertAsync(stream, ".xml");

        result.Should().StartWith("```xml\n");
        result.Should().EndWith("\n```");
        result.Should().Contain("<root>");
        result.Should().Contain("<item>Hello</item>");
    }

    [Fact]
    public async Task ConvertAsync_MalformedXml_ReturnsPlainCodeBlock()
    {
        var badXml = "<root><unclosed>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(badXml));

        var result = await _converter.ConvertAsync(stream, ".xml");

        result.Should().StartWith("```\n");
        result.Should().EndWith("\n```");
        result.Should().Contain("<root><unclosed>");
    }

    [Fact]
    public async Task ConvertAsync_EmptyContent_ReturnsEmpty()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".xml");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".xml");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
