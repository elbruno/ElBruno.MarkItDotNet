using System.Text;
using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests.Security;

/// <summary>
/// Tests that converters handle corrupt/malformed file data gracefully
/// without crashing, leaking memory, or exposing internal details.
/// </summary>
public class CorruptFileTests
{
    [Fact]
    public async Task XmlConverter_CorruptData_ReturnsPlainCodeBlock()
    {
        var converter = new XmlConverter();
        var corrupt = "This is definitely not XML <<<<>>>&&&"u8.ToArray();
        using var stream = new MemoryStream(corrupt);

        var result = await converter.ConvertAsync(stream, ".xml");

        result.Should().StartWith("```");
        result.Should().Contain("not XML");
    }

    [Fact]
    public async Task XmlConverter_BinaryData_ReturnsPlainCodeBlock()
    {
        var converter = new XmlConverter();
        var binaryData = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD };
        using var stream = new MemoryStream(binaryData);

        var result = await converter.ConvertAsync(stream, ".xml");

        result.Should().StartWith("```");
    }

    [Fact]
    public async Task XmlConverter_EmptyContent_ReturnsEmpty()
    {
        var converter = new XmlConverter();
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await converter.ConvertAsync(stream, ".xml");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EpubConverter_CorruptData_ThrowsGracefully()
    {
        var converter = new EpubConverter();
        var corruptData = Encoding.UTF8.GetBytes("This is not a valid EPUB file");
        using var stream = new MemoryStream(corruptData);

        // EpubConverter should throw an exception for invalid EPUB, not crash
        var act = () => converter.ConvertAsync(stream, ".epub");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task EpubConverter_EmptyStream_ThrowsGracefully()
    {
        var converter = new EpubConverter();
        using var stream = new MemoryStream(Array.Empty<byte>());

        var act = () => converter.ConvertAsync(stream, ".epub");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task MarkdownService_CorruptXml_ReturnsFailureNotCrash()
    {
        var registry = new ConverterRegistry();
        registry.Register(new XmlConverter());
        var service = new MarkdownService(registry);

        // Create a corrupt XML file
        var corrupt = "<<<INVALID XML>>>"u8.ToArray();
        using var stream = new MemoryStream(corrupt);

        var result = await service.ConvertAsync(stream, ".xml");

        // XmlConverter handles this gracefully by returning a code block
        result.Success.Should().BeTrue();
        result.Markdown.Should().Contain("INVALID XML");
    }

    [Fact]
    public async Task PlainTextConverter_VeryLargeInput_HandlesGracefully()
    {
        var converter = new PlainTextConverter();
        // 1 MB of text — should not crash
        var largeContent = new string('A', 1024 * 1024);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(largeContent));

        var result = await converter.ConvertAsync(stream, ".txt");

        result.Length.Should().BeGreaterThanOrEqualTo(1024 * 1024);
    }

    [Fact]
    public async Task XmlConverter_DeeplyNested_HandlesGracefully()
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\"?>");
        for (var i = 0; i < 100; i++)
            sb.Append($"<level{i}>");
        sb.Append("deep");
        for (var i = 99; i >= 0; i--)
            sb.Append($"</level{i}>");

        var converter = new XmlConverter();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

        var result = await converter.ConvertAsync(stream, ".xml");

        result.Should().Contain("deep");
    }
}
