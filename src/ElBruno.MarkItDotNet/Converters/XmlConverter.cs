using System.Xml.Linq;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts XML (.xml) files to Markdown fenced code blocks with syntax highlighting.
/// Formats well-formed XML with indentation. Malformed XML is returned in a plain code block.
/// </summary>
public class XmlConverter : IMarkdownConverter
{
    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".xml", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        try
        {
            var settings = new System.Xml.XmlReaderSettings
            {
                DtdProcessing = System.Xml.DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreWhitespace = true
            };
            using var stringReader = new StringReader(content);
            using var xmlReader = System.Xml.XmlReader.Create(stringReader, settings);
            var doc = XDocument.Load(xmlReader);
            var formatted = doc.ToString().Trim();
            return $"```xml\n{formatted}\n```";
        }
        catch (System.Xml.XmlException)
        {
            return $"```\n{content}\n```";
        }
    }
}
