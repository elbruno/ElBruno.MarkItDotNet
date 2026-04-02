using System.Net.Http;
using System.Text.RegularExpressions;
using ReverseMarkdown;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts web pages (URLs) to Markdown by fetching HTML and converting via ReverseMarkdown.
/// Pass a .url file containing the URL, or use MarkdownService.ConvertUrlAsync() directly.
/// </summary>
public class UrlConverter : IMarkdownConverter
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".url"
    };

    private readonly HttpClient _httpClient;

    public UrlConverter() : this(new HttpClient()) { }

    public UrlConverter(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public bool CanHandle(string fileExtension) =>
        SupportedExtensions.Contains(fileExtension);

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
            return $"*Invalid URL: {url}*";
        }

        try
        {
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
                result.AppendLine($"# {title}");
                result.AppendLine();
            }

            result.AppendLine(markdown.Trim());
            result.AppendLine();
            result.AppendLine("---");
            result.AppendLine($"*Source: [{uri.Host}]({url})*");

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
        catch (Exception ex)
        {
            return $"*Error converting URL: {ex.Message}*";
        }
    }

    private static string ExtractTitle(string html)
    {
        var match = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value).Trim() : string.Empty;
    }

    private static string CleanHtml(string html)
    {
        var patterns = new[]
        {
            @"<script[^>]*>.*?</script>",
            @"<style[^>]*>.*?</style>",
            @"<nav[^>]*>.*?</nav>",
            @"<footer[^>]*>.*?</footer>",
            @"<header[^>]*>.*?</header>",
            @"<aside[^>]*>.*?</aside>",
            @"<noscript[^>]*>.*?</noscript>"
        };

        var result = html;
        foreach (var pattern in patterns)
        {
            result = Regex.Replace(result, pattern, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        return result;
    }
}
