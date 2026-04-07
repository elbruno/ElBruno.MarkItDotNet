using System.Text;
using ReverseMarkdown;
using VersOne.Epub;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts EPub (.epub) files to Markdown by extracting chapters and converting HTML content.
/// Uses VersOne.Epub for reading and ReverseMarkdown for HTML-to-Markdown conversion.
/// </summary>
public class EpubConverter : IMarkdownConverter
{
    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".epub", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        // VersOne.Epub requires a seekable stream
        Stream seekableStream;
        bool ownsStream;

        if (fileStream.CanSeek)
        {
            seekableStream = fileStream;
            ownsStream = false;
        }
        else
        {
            const long MaxEpubSizeBytes = 100 * 1024 * 1024; // 100 MB
            var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            if (ms.Length > MaxEpubSizeBytes)
            {
                await ms.DisposeAsync().ConfigureAwait(false);
                throw new InvalidOperationException(
                    $"EPUB file exceeds maximum allowed size of {MaxEpubSizeBytes / (1024 * 1024)} MB.");
            }
            ms.Position = 0;
            seekableStream = ms;
            ownsStream = true;
        }

        try
        {
            var book = await EpubReader.ReadBookAsync(seekableStream).ConfigureAwait(false);
            var converter = new Converter(new Config
            {
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                RemoveComments = true,
                GithubFlavored = true,
                SmartHrefHandling = true
            });

            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(book.Title))
            {
                sb.AppendLine($"# {book.Title}");
                sb.AppendLine();
            }

            var readingOrder = book.ReadingOrder;
            var chapterNumber = 0;

            foreach (var textContent in readingOrder)
            {
                chapterNumber++;
                var html = textContent.Content;

                if (string.IsNullOrWhiteSpace(html))
                {
                    continue;
                }

                var markdown = converter.Convert(html)?.Trim();
                if (string.IsNullOrWhiteSpace(markdown))
                {
                    continue;
                }

                sb.AppendLine($"## Chapter {chapterNumber}");
                sb.AppendLine();
                sb.AppendLine(markdown);
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        finally
        {
            if (ownsStream)
            {
                await seekableStream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
