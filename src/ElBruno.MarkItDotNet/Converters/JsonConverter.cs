using System.Text.Json;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts JSON (.json) files to Markdown fenced code blocks with syntax highlighting.
/// Reformats compact JSON to pretty-printed output.
/// </summary>
public class JsonConverter : IMarkdownConverter
{
    private static readonly JsonSerializerOptions PrettyPrintOptions = new()
    {
        WriteIndented = true
    };

    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".json", StringComparison.OrdinalIgnoreCase);

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

        string formatted;
        try
        {
            using var doc = JsonDocument.Parse(content);
            formatted = JsonSerializer.Serialize(doc.RootElement, PrettyPrintOptions).ReplaceLineEndings("\n");
        }
        catch (JsonException)
        {
            // Invalid JSON — return raw content with a note
            return $"> **Note:** The following content could not be parsed as valid JSON.\n\n```\n{content.ReplaceLineEndings("\n")}\n```";
        }

        return $"```json\n{formatted}\n```";
    }
}
