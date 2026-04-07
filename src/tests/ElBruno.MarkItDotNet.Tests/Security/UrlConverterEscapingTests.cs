using System.Net;
using System.Text;
using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests.Security;

/// <summary>
/// Tests that the UrlConverter properly escapes Markdown-sensitive characters
/// in titles and URLs to prevent Markdown injection.
/// </summary>
public class UrlConverterEscapingTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;

        public MockHttpMessageHandler(string responseContent)
        {
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "text/html")
            });
        }
    }

    private static UrlConverter CreateConverter(string html)
    {
        var handler = new MockHttpMessageHandler(html);
        var httpClient = new HttpClient(handler);
        return new UrlConverter(httpClient);
    }

    [Fact]
    public async Task ConvertUrlAsync_TitleWithSquareBrackets_EscapesBrackets()
    {
        var html = "<html><head><title>[Injected](http://evil.com)</title></head><body><p>Content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().NotContain("[Injected](http://evil.com)");
        result.Should().Contain(@"\[Injected\]\(http://evil.com\)");
    }

    [Fact]
    public async Task ConvertUrlAsync_TitleWithParentheses_EscapesParentheses()
    {
        var html = "<html><head><title>Page (with) parens</title></head><body><p>Content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().Contain(@"Page \(with\) parens");
    }

    [Fact]
    public async Task ConvertUrlAsync_SourceFooter_EscapesHostAndUrl()
    {
        var html = "<html><body><p>Content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com/page(1)");

        result.Should().Contain(@"example.com");
        result.Should().Contain(@"\(1\)");
    }

    [Fact]
    public async Task ConvertUrlAsync_InvalidUrl_EscapesOutputInMessage()
    {
        var converter = CreateConverter("<html></html>");

        var result = await converter.ConvertUrlAsync("not-a-url [click](http://evil.com)");

        result.Should().NotContain("[click](http://evil.com)");
    }

    [Fact]
    public async Task ConvertUrlAsync_MaliciousTitle_PreventsMarkdownLink()
    {
        var html = "<html><head><title>](http://evil.com) # Hijack</title></head><body><p>Content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        // The ] and ( should be escaped, preventing a link
        result.Should().Contain(@"\]");
        result.Should().Contain(@"\(");
    }
}
