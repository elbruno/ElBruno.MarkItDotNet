using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ElBruno.MarkItDotNet;

/// <summary>
/// Main entry point for converting files to Markdown.
/// Delegates to registered <see cref="IMarkdownConverter"/> implementations via the <see cref="ConverterRegistry"/>.
/// </summary>
public class MarkdownService
{
    private readonly ConverterRegistry _registry;
    private readonly MarkItDotNetOptions _options;

    /// <summary>
    /// Creates a new <see cref="MarkdownService"/> with the given converter registry.
    /// </summary>
    public MarkdownService(ConverterRegistry registry)
        : this(registry, null, null)
    {
    }

    /// <summary>
    /// Creates a new <see cref="MarkdownService"/> with the given converter registry
    /// and optional DI-resolved plugins that will be auto-registered into the registry.
    /// </summary>
    public MarkdownService(ConverterRegistry registry, IEnumerable<IConverterPlugin>? plugins)
        : this(registry, plugins, null)
    {
    }

    /// <summary>
    /// Creates a new <see cref="MarkdownService"/> with the given converter registry,
    /// optional DI-resolved plugins, and configuration options.
    /// </summary>
    public MarkdownService(ConverterRegistry registry, IEnumerable<IConverterPlugin>? plugins, MarkItDotNetOptions? options)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _options = options ?? new MarkItDotNetOptions();

        if (plugins is not null)
        {
            var existingPlugins = new HashSet<string>(
                registry.GetPlugins().Select(p => p.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (var plugin in plugins)
            {
                if (!existingPlugins.Contains(plugin.Name))
                {
                    registry.RegisterPlugin(plugin);
                    existingPlugins.Add(plugin.Name);
                }
            }
        }
    }

    /// <summary>
    /// Converts a file at the given path to Markdown.
    /// </summary>
    /// <param name="filePath">Path to the file to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> with the outcome.</returns>
    public async Task<ConversionResult> ConvertAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        var converter = _registry.Resolve(extension);
        if (converter is null)
        {
            return ConversionResult.Failure(
                $"File format '{extension}' is not supported.", extension);
        }

        try
        {
            if (_options.MaxFileSizeBytes > 0)
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > _options.MaxFileSizeBytes)
                {
                    return ConversionResult.Failure(
                        $"File exceeds maximum allowed size of {_options.MaxFileSizeBytes} bytes.", extension);
                }
            }

            var sw = Stopwatch.StartNew();
            using var stream = File.OpenRead(filePath);
            var markdown = await converter.ConvertAsync(stream, extension, cancellationToken).ConfigureAwait(false);
            sw.Stop();

            var metadata = new ConversionMetadata
            {
                WordCount = CountWords(markdown),
                ProcessingTime = sw.Elapsed
            };

            return ConversionResult.Succeeded(markdown, extension, metadata);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ConversionResult.Failure(SanitizeErrorMessage(ex.Message), extension);
        }
    }

    /// <summary>
    /// Converts a stream to Markdown using the converter for the given file extension.
    /// </summary>
    /// <param name="stream">The input stream containing file content.</param>
    /// <param name="fileExtension">File extension including the leading dot (e.g., ".txt").</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> with the outcome.</returns>
    public async Task<ConversionResult> ConvertAsync(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

        var extension = fileExtension.ToLowerInvariant();

        var converter = _registry.Resolve(extension);
        if (converter is null)
        {
            return ConversionResult.Failure(
                $"File format '{extension}' is not supported.", extension);
        }

        try
        {
            if (_options.MaxFileSizeBytes > 0 && stream.CanSeek && stream.Length > _options.MaxFileSizeBytes)
            {
                return ConversionResult.Failure(
                    $"File exceeds maximum allowed size of {_options.MaxFileSizeBytes} bytes.", extension);
            }

            var sw = Stopwatch.StartNew();
            var markdown = await converter.ConvertAsync(stream, extension, cancellationToken).ConfigureAwait(false);
            sw.Stop();

            var metadata = new ConversionMetadata
            {
                WordCount = CountWords(markdown),
                ProcessingTime = sw.Elapsed
            };

            return ConversionResult.Succeeded(markdown, extension, metadata);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ConversionResult.Failure(SanitizeErrorMessage(ex.Message), extension);
        }
    }

    /// <summary>
    /// Converts a file at the given path to Markdown, yielding chunks asynchronously.
    /// If the resolved converter implements <see cref="IStreamingMarkdownConverter"/>,
    /// chunks are streamed page-by-page; otherwise the full result is yielded as a single chunk.
    /// </summary>
    /// <param name="filePath">Path to the file to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async IAsyncEnumerable<string> ConvertStreamingAsync(
        string filePath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var converter = _registry.Resolve(extension)
            ?? throw new NotSupportedException($"File format '{extension}' is not supported.");

        using var stream = File.OpenRead(filePath);

        await foreach (var chunk in ConvertStreamingCoreAsync(converter, stream, extension, cancellationToken).ConfigureAwait(false))
        {
            yield return chunk;
        }
    }

    /// <summary>
    /// Converts a stream to Markdown, yielding chunks asynchronously.
    /// If the resolved converter implements <see cref="IStreamingMarkdownConverter"/>,
    /// chunks are streamed page-by-page; otherwise the full result is yielded as a single chunk.
    /// </summary>
    /// <param name="stream">The input stream containing file content.</param>
    /// <param name="fileExtension">File extension including the leading dot (e.g., ".pdf").</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async IAsyncEnumerable<string> ConvertStreamingAsync(
        Stream stream,
        string fileExtension,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

        var extension = fileExtension.ToLowerInvariant();
        var converter = _registry.Resolve(extension)
            ?? throw new NotSupportedException($"File format '{extension}' is not supported.");

        await foreach (var chunk in ConvertStreamingCoreAsync(converter, stream, extension, cancellationToken).ConfigureAwait(false))
        {
            yield return chunk;
        }
    }

    private static async IAsyncEnumerable<string> ConvertStreamingCoreAsync(
        IMarkdownConverter converter,
        Stream stream,
        string extension,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (converter is IStreamingMarkdownConverter streaming)
        {
            await foreach (var chunk in streaming.ConvertStreamingAsync(stream, extension, cancellationToken).ConfigureAwait(false))
            {
                yield return chunk;
            }
        }
        else
        {
            var markdown = await converter.ConvertAsync(stream, extension, cancellationToken).ConfigureAwait(false);
            yield return markdown;
        }
    }

    /// <summary>
    /// Converts a web page at the given URL to Markdown.
    /// </summary>
    /// <param name="url">The HTTP/HTTPS URL to fetch and convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> with the outcome.</returns>
    public async Task<ConversionResult> ConvertUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var converter = _registry.Resolve(".url");
        if (converter is null)
        {
            return ConversionResult.Failure("URL converter is not registered.", ".url");
        }

        try
        {
            var sw = Stopwatch.StartNew();
            string markdown;

            if (converter is Converters.UrlConverter urlConverter)
            {
                markdown = await urlConverter.ConvertUrlAsync(url, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(url));
                markdown = await converter.ConvertAsync(stream, ".url", cancellationToken).ConfigureAwait(false);
            }

            sw.Stop();

            var metadata = new ConversionMetadata
            {
                WordCount = CountWords(markdown),
                ProcessingTime = sw.Elapsed
            };

            return ConversionResult.Succeeded(markdown, ".url", metadata);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ConversionResult.Failure(SanitizeErrorMessage(ex.Message), ".url");
        }
    }

    private static string SanitizeErrorMessage(string message)
    {
        // Remove file path information to prevent information leakage
        var sanitized = Regex.Replace(
            message,
            @"[A-Za-z]:\\[^\s""']+|/[^\s""']+",
            "[path]");
        return sanitized;
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var count = 0;
        var inWord = false;

        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }
}
