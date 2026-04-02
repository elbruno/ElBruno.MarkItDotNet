namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts YAML (.yaml, .yml) files to Markdown fenced code blocks with syntax highlighting.
/// </summary>
public class YamlConverter : IMarkdownConverter
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".yaml",
        ".yml"
    };

    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        SupportedExtensions.Contains(fileExtension);

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

        return $"```yaml\n{content.TrimEnd()}\n```";
    }
}
