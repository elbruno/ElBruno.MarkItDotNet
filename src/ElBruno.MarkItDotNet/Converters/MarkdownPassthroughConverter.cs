namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Handles Markdown (.md, .markdown) files by returning their content as-is.
/// This prevents a <see cref="System.NotSupportedException"/> when Markdown files
/// are passed through bulk conversion pipelines.
/// </summary>
public class MarkdownPassthroughConverter : IMarkdownConverter
{
    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
        fileExtension.Equals(".markdown", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }
}
