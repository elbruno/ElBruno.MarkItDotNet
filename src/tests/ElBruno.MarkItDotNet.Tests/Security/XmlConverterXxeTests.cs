using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests.Security;

/// <summary>
/// Tests that the XmlConverter blocks XXE (XML External Entity) attacks
/// by prohibiting DTD processing.
/// </summary>
public class XmlConverterXxeTests
{
    private readonly XmlConverter _converter = new();

    [Fact]
    public async Task ConvertAsync_XxePayload_BlocksExternalEntity()
    {
        var xxeXml = """
            <?xml version="1.0"?>
            <!DOCTYPE foo [
              <!ENTITY xxe SYSTEM "file:///etc/passwd">
            ]>
            <root>&xxe;</root>
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xxeXml));

        var result = await _converter.ConvertAsync(stream, ".xml");

        // DTD processing is prohibited, so XmlException is caught → returns raw content in code block
        result.Should().StartWith("```");
        result.Should().NotContain("root:x:");
        result.Should().Contain("xxe");
    }

    [Fact]
    public async Task ConvertAsync_BillionLaughs_BlocksBombPayload()
    {
        var billionLaughs = """
            <?xml version="1.0"?>
            <!DOCTYPE lolz [
              <!ENTITY lol "lol">
              <!ENTITY lol2 "&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;">
              <!ENTITY lol3 "&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;">
              <!ENTITY lol4 "&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;">
            ]>
            <root>&lol4;</root>
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(billionLaughs));

        var result = await _converter.ConvertAsync(stream, ".xml");

        // DTD processing is prohibited → graceful fallback to plain code block
        result.Should().StartWith("```");
        result.Should().Contain("lol");
    }

    [Fact]
    public async Task ConvertAsync_SsrfViaXxe_BlocksRemoteEntity()
    {
        var xxeSsrf = """
            <?xml version="1.0"?>
            <!DOCTYPE foo [
              <!ENTITY xxe SYSTEM "http://169.254.169.254/latest/meta-data/">
            ]>
            <root>&xxe;</root>
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xxeSsrf));

        var result = await _converter.ConvertAsync(stream, ".xml");

        // DTD prohibited → no network request is made; falls back to plain code block
        result.Should().StartWith("```");
        // The entity is NOT resolved — the raw XML is returned as-is in a code block
        // rather than the resolved cloud metadata content. This is the security property.
        result.Should().NotContain("```xml"); // Not parsed as valid XML
    }

    [Fact]
    public async Task ConvertAsync_ValidXml_ParsesNormally()
    {
        var validXml = """
            <?xml version="1.0"?>
            <root>
              <item>Hello World</item>
            </root>
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(validXml));

        var result = await _converter.ConvertAsync(stream, ".xml");

        result.Should().StartWith("```xml");
        result.Should().Contain("Hello World");
    }

    [Fact]
    public async Task ConvertAsync_ParameterEntityExpansion_IsBlocked()
    {
        var paramEntity = """
            <?xml version="1.0"?>
            <!DOCTYPE foo [
              <!ENTITY % pe SYSTEM "file:///etc/hostname">
              %pe;
            ]>
            <root>safe</root>
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(paramEntity));

        var result = await _converter.ConvertAsync(stream, ".xml");

        result.Should().StartWith("```");
    }
}
