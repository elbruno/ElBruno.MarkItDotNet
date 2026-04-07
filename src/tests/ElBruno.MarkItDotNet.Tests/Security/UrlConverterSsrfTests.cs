using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests.Security;

/// <summary>
/// Tests that the UrlConverter blocks SSRF attacks by denying requests to
/// private/internal IP addresses and non-HTTP(S) schemes.
/// </summary>
public class UrlConverterSsrfTests
{
    private readonly UrlConverter _converter = new();

    [Fact]
    public async Task ConvertUrlAsync_Localhost_ReturnsBlockedMessage()
    {
        var result = await _converter.ConvertUrlAsync("http://localhost");

        result.Should().Contain("*Blocked URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_LoopbackIpV4_ReturnsBlockedMessage()
    {
        var result = await _converter.ConvertUrlAsync("http://127.0.0.1");

        result.Should().Contain("*Blocked URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_LoopbackIpV6_ReturnsBlockedMessage()
    {
        var result = await _converter.ConvertUrlAsync("http://[::1]");

        result.Should().Contain("*Blocked URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_LinkLocalMetadata_ReturnsBlockedMessage()
    {
        // Cloud metadata endpoint (AWS/Azure/GCP)
        var result = await _converter.ConvertUrlAsync("http://169.254.169.254");

        result.Should().Contain("*Blocked URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_FileScheme_ReturnsInvalidMessage()
    {
        var result = await _converter.ConvertUrlAsync("file:///etc/passwd");

        result.Should().Contain("*Invalid URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_FtpScheme_ReturnsInvalidMessage()
    {
        var result = await _converter.ConvertUrlAsync("ftp://files.example.com");

        result.Should().Contain("*Invalid URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_JavascriptScheme_ReturnsInvalidMessage()
    {
        var result = await _converter.ConvertUrlAsync("javascript:alert(1)");

        result.Should().Contain("*Invalid URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_DataScheme_ReturnsInvalidMessage()
    {
        var result = await _converter.ConvertUrlAsync("data:text/html,<h1>XSS</h1>");

        result.Should().Contain("*Invalid URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_UnresolvableHost_ReturnsBlockedMessage()
    {
        var result = await _converter.ConvertUrlAsync("http://this-host-definitely-does-not-exist-abc123xyz.invalid");

        // Should return blocked due to DNS resolution failure
        result.Should().Contain("*Blocked URL:");
        result.Should().Contain("Unable to resolve hostname");
    }

    [Fact]
    public async Task ConvertUrlAsync_LoopbackWithPort_ReturnsBlockedMessage()
    {
        var result = await _converter.ConvertUrlAsync("http://127.0.0.1:8080");

        result.Should().Contain("*Blocked URL:");
    }
}
