using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ReverseMarkdown;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts web pages (URLs) to Markdown by fetching HTML and converting via ReverseMarkdown.
/// Pass a .url file containing the URL, or use MarkdownService.ConvertUrlAsync() directly.
/// </summary>
public partial class UrlConverter : IMarkdownConverter
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".url"
    };

    private static readonly HttpClient SharedHttpClient = CreateDefaultHttpClient();

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="UrlConverter"/> with a shared <see cref="HttpClient"/>
    /// configured with a 30-second timeout.
    /// </summary>
    public UrlConverter() : this(SharedHttpClient) { }

    /// <summary>
    /// Initializes a new instance of <see cref="UrlConverter"/> with a custom <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for fetching web pages.</param>
    public UrlConverter(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        SupportedExtensions.Contains(fileExtension);

    /// <inheritdoc />
    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var url = (await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();

        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        return await ConvertUrlAsync(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts a URL directly to Markdown by fetching and converting the page HTML.
    /// </summary>
    public async Task<string> ConvertUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return $"*Invalid URL: {EscapeMarkdown(url)}*";
        }

        try
        {
            await ValidateUrlSafetyAsync(uri).ConfigureAwait(false);

            var html = await _httpClient.GetStringAsync(uri, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var title = ExtractTitle(html);

            var cleanedHtml = CleanHtml(html);

            var converter = new Converter(new Config
            {
                UnknownTags = Config.UnknownTagsOption.Bypass,
                RemoveComments = true,
                GithubFlavored = true,
                SmartHrefHandling = true
            });

            var markdown = converter.Convert(cleanedHtml);

            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            var result = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(title))
            {
                result.AppendLine($"# {EscapeMarkdown(title)}");
                result.AppendLine();
            }

            result.AppendLine(markdown.Trim());
            result.AppendLine();
            result.AppendLine("---");
            result.AppendLine($"*Source: [{EscapeMarkdown(uri.Host)}]({EscapeMarkdown(url)})*");

            return result.ToString().TrimEnd() + Environment.NewLine;
        }
        catch (HttpRequestException ex)
        {
            return $"*Failed to fetch URL: {ex.Message}*";
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            return $"*Blocked URL: {ex.Message}*";
        }
        catch (Exception ex)
        {
            return $"*Error converting URL: {ex.Message}*";
        }
    }

    private static async Task ValidateUrlSafetyAsync(Uri uri)
    {
        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(uri.Host).ConfigureAwait(false);
        }
        catch (SocketException)
        {
            throw new InvalidOperationException("Unable to resolve hostname.");
        }

        foreach (var address in addresses)
        {
            if (IsPrivateIpAddress(address))
            {
                throw new InvalidOperationException("Access to private or internal network addresses is not allowed.");
            }
        }
    }

    private static bool IsPrivateIpAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] switch
            {
                10 => true,                                             // 10.0.0.0/8
                172 => bytes[1] >= 16 && bytes[1] <= 31,               // 172.16.0.0/12
                192 => bytes[1] == 168,                                 // 192.168.0.0/16
                169 => bytes[1] == 254,                                 // 169.254.0.0/16 (link-local / cloud metadata)
                127 => true,                                            // 127.0.0.0/8
                _ => false
            };
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.Equals(IPAddress.IPv6Loopback))
                return true;

            var bytes = address.GetAddressBytes();
            // fc00::/7 — unique local
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;

            // fe80::/10 — link-local
            if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80)
                return true;
        }

        return false;
    }

    private static string EscapeMarkdown(string text)
    {
        return text
            .Replace("[", @"\[")
            .Replace("]", @"\]")
            .Replace("(", @"\(")
            .Replace(")", @"\)");
    }

    private static HttpClient CreateDefaultHttpClient()
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private static string ExtractTitle(string html)
    {
        var match = TitleTagRegex().Match(html);
        return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value).Trim() : string.Empty;
    }

    private static string CleanHtml(string html)
    {
        var result = html;
        result = ScriptTagRegex().Replace(result, string.Empty);
        result = StyleTagRegex().Replace(result, string.Empty);
        result = NavTagRegex().Replace(result, string.Empty);
        result = FooterTagRegex().Replace(result, string.Empty);
        result = HeaderTagRegex().Replace(result, string.Empty);
        result = AsideTagRegex().Replace(result, string.Empty);
        result = NoscriptTagRegex().Replace(result, string.Empty);
        return result;
    }

    [GeneratedRegex(@"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TitleTagRegex();

    [GeneratedRegex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex(@"<style[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StyleTagRegex();

    [GeneratedRegex(@"<nav[^>]*>.*?</nav>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NavTagRegex();

    [GeneratedRegex(@"<footer[^>]*>.*?</footer>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FooterTagRegex();

    [GeneratedRegex(@"<header[^>]*>.*?</header>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HeaderTagRegex();

    [GeneratedRegex(@"<aside[^>]*>.*?</aside>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AsideTagRegex();

    [GeneratedRegex(@"<noscript[^>]*>.*?</noscript>", RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NoscriptTagRegex();
}
