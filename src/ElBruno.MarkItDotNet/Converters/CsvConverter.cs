using System.Text;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts CSV (.csv) and TSV (.tsv) files to Markdown tables.
/// Uses tab separator for .tsv and comma for .csv. Handles quoted fields.
/// </summary>
public class CsvConverter : IMarkdownConverter
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".csv",
        ".tsv"
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

        var separator = fileExtension.Equals(".tsv", StringComparison.OrdinalIgnoreCase) ? '\t' : ',';
        var lines = SplitLines(content);
        if (lines.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        var headers = ParseFields(lines[0], separator);
        sb.Append('|');
        foreach (var header in headers)
        {
            sb.Append(' ').Append(EscapePipe(header.Trim())).Append(" |");
        }
        sb.AppendLine();

        // Separator row
        sb.Append('|');
        for (var i = 0; i < headers.Count; i++)
        {
            sb.Append(" --- |");
        }
        sb.AppendLine();

        // Data rows
        for (var row = 1; row < lines.Count; row++)
        {
            var fields = ParseFields(lines[row], separator);
            sb.Append('|');
            for (var col = 0; col < headers.Count; col++)
            {
                var value = col < fields.Count ? fields[col].Trim() : string.Empty;
                sb.Append(' ').Append(EscapePipe(value)).Append(" |");
            }
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static string EscapePipe(string value) =>
        value.Replace("|", "\\|");

    private static List<string> SplitLines(string content)
    {
        var lines = new List<string>();
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (!string.IsNullOrEmpty(trimmed))
            {
                lines.Add(trimmed);
            }
        }
        return lines;
    }

    internal static List<string> ParseFields(string line, char separator)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == separator)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
