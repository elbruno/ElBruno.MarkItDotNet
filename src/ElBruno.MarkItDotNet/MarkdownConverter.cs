using ElBruno.MarkItDotNet.Converters;

namespace ElBruno.MarkItDotNet;

/// <summary>
/// Simple façade for converting files to Markdown.
/// For advanced usage and DI scenarios, prefer <see cref="MarkdownService"/>.
/// </summary>
public class MarkdownConverter
{
    private readonly MarkdownService _service;

    /// <summary>
    /// Creates a new converter with all built-in converters registered.
    /// </summary>
    public MarkdownConverter()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());
        registry.Register(new JsonConverter());
        registry.Register(new HtmlConverter());
        registry.Register(new DocxConverter());
        registry.Register(new PdfConverter());
        registry.Register(new ImageConverter());
        registry.Register(new CsvConverter());
        registry.Register(new XmlConverter());
        registry.Register(new YamlConverter());
        registry.Register(new RtfConverter());
        registry.Register(new EpubConverter());
        registry.Register(new UrlConverter());
        registry.Register(new MarkdownPassthroughConverter());
        _service = new MarkdownService(registry);
    }

    /// <summary>
    /// Converts the content of a file to Markdown.
    /// </summary>
    /// <param name="filePath">Path to the file to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The Markdown representation of the file content.</returns>
    public string ConvertToMarkdown(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var result = _service.ConvertAsync(filePath, cancellationToken).GetAwaiter().GetResult();
        if (!result.Success)
        {
            throw new NotSupportedException(result.ErrorMessage);
        }

        return result.Markdown;
    }
}
