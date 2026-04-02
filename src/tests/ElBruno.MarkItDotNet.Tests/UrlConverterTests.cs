using System.Net;
using System.Text;
using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class UrlConverterTests
{
    #region Mock HTTP Handler

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;

        public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "text/html")
            });
        }
    }

    private static UrlConverter CreateConverter(string html, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new MockHttpMessageHandler(html, statusCode);
        var httpClient = new HttpClient(handler);
        return new UrlConverter(httpClient);
    }

    #endregion

    #region CanHandle

    [Fact]
    public void CanHandle_UrlExtension_ReturnsTrue()
    {
        var converter = new UrlConverter();
        converter.CanHandle(".url").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        var converter = new UrlConverter();
        converter.CanHandle(".URL").Should().BeTrue();
        converter.CanHandle(".Url").Should().BeTrue();
    }

    [Theory]
    [InlineData(".html")]
    [InlineData(".htm")]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".json")]
    public void CanHandle_NonUrlExtension_ReturnsFalse(string extension)
    {
        var converter = new UrlConverter();
        converter.CanHandle(extension).Should().BeFalse();
    }

    #endregion

    #region ConvertAsync (stream-based)

    [Fact]
    public async Task ConvertAsync_StreamContainingUrl_FetchesAndConverts()
    {
        var html = "<html><head><title>Test Page</title></head><body><p>Hello world</p></body></html>";
        var converter = CreateConverter(html);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("https://example.com"));

        var result = await converter.ConvertAsync(stream, ".url");

        result.Should().Contain("# Test Page");
        result.Should().Contain("Hello world");
        result.Should().Contain("*Source: [example.com](https://example.com)*");
    }

    [Fact]
    public async Task ConvertAsync_EmptyStream_ReturnsEmpty()
    {
        var converter = CreateConverter("<html></html>");
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));

        var result = await converter.ConvertAsync(stream, ".url");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_WhitespaceOnlyStream_ReturnsEmpty()
    {
        var converter = CreateConverter("<html></html>");
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("   \n  "));

        var result = await converter.ConvertAsync(stream, ".url");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var converter = new UrlConverter();
        var act = () => converter.ConvertAsync(null!, ".url");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region ConvertUrlAsync — Title Extraction

    [Fact]
    public async Task ConvertUrlAsync_ExtractsTitle_PrependsAsH1()
    {
        var html = "<html><head><title>My Page Title</title></head><body><p>Content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().StartWith("# My Page Title");
    }

    [Fact]
    public async Task ConvertUrlAsync_NoTitle_OmitsH1()
    {
        var html = "<html><body><p>Content only</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().NotStartWith("# ");
        result.Should().Contain("Content only");
    }

    [Fact]
    public async Task ConvertUrlAsync_HtmlEncodedTitle_DecodesCorrectly()
    {
        var html = "<html><head><title>Tom &amp; Jerry&#39;s Page</title></head><body><p>Fun</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().Contain("# Tom & Jerry's Page");
    }

    #endregion

    #region ConvertUrlAsync — Source Footer

    [Fact]
    public async Task ConvertUrlAsync_IncludesSourceFooter()
    {
        var html = "<html><body><p>Content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://www.example.com/page");

        result.Should().Contain("---");
        result.Should().Contain("*Source: [www.example.com](https://www.example.com/page)*");
    }

    #endregion

    #region ConvertUrlAsync — HTML Cleaning

    [Fact]
    public async Task ConvertUrlAsync_StripsScriptTags()
    {
        var html = "<html><body><script>alert('xss')</script><p>Safe content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().NotContain("alert");
        result.Should().NotContain("<script");
        result.Should().Contain("Safe content");
    }

    [Fact]
    public async Task ConvertUrlAsync_StripsStyleTags()
    {
        var html = "<html><body><style>body { color: red; }</style><p>Styled content</p></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().NotContain("color: red");
        result.Should().Contain("Styled content");
    }

    [Fact]
    public async Task ConvertUrlAsync_StripsNavFooterHeaderTags()
    {
        var html = """
            <html><body>
            <nav><a href="/">Home</a></nav>
            <header><h1>Site Header</h1></header>
            <main><p>Main content here</p></main>
            <footer><p>Copyright 2024</p></footer>
            </body></html>
            """;
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().Contain("Main content here");
        result.Should().NotContain("Copyright 2024");
    }

    #endregion

    #region ConvertUrlAsync — HTML Structure Preservation

    [Fact]
    public async Task ConvertUrlAsync_PreservesHeadings()
    {
        var html = "<html><body><h2>Section One</h2><p>Text</p><h3>Subsection</h3></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().Contain("## Section One");
        result.Should().Contain("### Subsection");
    }

    [Fact]
    public async Task ConvertUrlAsync_PreservesLinks()
    {
        var html = """<html><body><a href="https://github.com">GitHub</a></body></html>""";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().Contain("[GitHub](https://github.com)");
    }

    [Fact]
    public async Task ConvertUrlAsync_PreservesLists()
    {
        var html = "<html><body><ul><li>Item A</li><li>Item B</li></ul></body></html>";
        var converter = CreateConverter(html);

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().Contain("Item A");
        result.Should().Contain("Item B");
    }

    #endregion

    #region ConvertUrlAsync — Error Handling

    [Fact]
    public async Task ConvertUrlAsync_InvalidUrl_ReturnsInvalidMessage()
    {
        var converter = CreateConverter("<html></html>");

        var result = await converter.ConvertUrlAsync("not-a-url");

        result.Should().Contain("*Invalid URL: not-a-url*");
    }

    [Fact]
    public async Task ConvertUrlAsync_FtpUrl_ReturnsInvalidMessage()
    {
        var converter = CreateConverter("<html></html>");

        var result = await converter.ConvertUrlAsync("ftp://files.example.com/doc.txt");

        result.Should().Contain("*Invalid URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_HttpError_ReturnsFailureMessage()
    {
        var converter = CreateConverter("Not Found", HttpStatusCode.NotFound);

        var result = await converter.ConvertUrlAsync("https://example.com/missing");

        result.Should().Contain("*Failed to fetch URL:");
    }

    [Fact]
    public async Task ConvertUrlAsync_EmptyHtmlResponse_ReturnsEmpty()
    {
        var converter = CreateConverter("   ");

        var result = await converter.ConvertUrlAsync("https://example.com");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertUrlAsync_NullOrWhitespace_ThrowsArgumentException()
    {
        var converter = new UrlConverter();

        var act1 = () => converter.ConvertUrlAsync(null!);
        var act2 = () => converter.ConvertUrlAsync("  ");

        await act1.Should().ThrowAsync<ArgumentException>();
        await act2.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Constructor

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        var act = () => new UrlConverter(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
