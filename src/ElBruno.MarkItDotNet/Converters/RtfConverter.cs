using ReverseMarkdown;
using RtfPipe;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts RTF (.rtf) files to Markdown via an RTF→HTML→Markdown pipeline.
/// Uses RtfPipe for RTF-to-HTML and ReverseMarkdown for HTML-to-Markdown.
/// </summary>
public class RtfConverter : IMarkdownConverter
{
    static RtfConverter()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".rtf", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var rtfContent = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(rtfContent))
        {
            return string.Empty;
        }

        // RTF -> HTML via RtfPipe
        var html = Rtf.ToHtml(rtfContent);

        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // HTML -> Markdown via ReverseMarkdown
        var converter = new Converter(new Config
        {
            UnknownTags = Config.UnknownTagsOption.PassThrough,
            RemoveComments = true,
            GithubFlavored = true,
            SmartHrefHandling = true
        });

        var markdown = converter.Convert(html);
        return markdown?.Trim() ?? string.Empty;
    }
}
