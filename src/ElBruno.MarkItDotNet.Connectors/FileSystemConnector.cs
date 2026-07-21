// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// File-system implementation of <see cref="IDocumentSource"/>.
/// Enumerates files lazily and exposes deferred stream opening.
/// </summary>
public sealed class FileSystemConnector : IDocumentSource
{
    private static readonly SearchOption TopDirectoryOnly = SearchOption.TopDirectoryOnly;

    private readonly FileSystemConnectorOptions _options;
    private readonly ILogger<FileSystemConnector> _logger;
    private readonly string _rootPath;
    private readonly List<string> _includePatterns;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemConnector"/> class.
    /// </summary>
    /// <param name="options">Connector options.</param>
    /// <param name="logger">Logger for warning and diagnostic messages.</param>
    public FileSystemConnector(
        FileSystemConnectorOptions options,
        ILogger<FileSystemConnector> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.RootPath);
        ArgumentOutOfRangeException.ThrowIfNegative(options.MaxDepth);
        if (options.MaxFileSizeBytes is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxFileSizeBytes));
        }

        _options = options;
        _logger = logger;
        _rootPath = Path.GetFullPath(options.RootPath);
        _includePatterns = BuildIncludePatterns(options.IncludePatterns);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SourceDocument> GetDocumentsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!await ValidateAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new DirectoryNotFoundException($"Root path '{_rootPath}' does not exist.");
        }

        var queue = new Queue<(string DirectoryPath, int Depth)>();
        queue.Enqueue((_rootPath, 0));

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (currentDirectory, currentDepth) = queue.Dequeue();

            foreach (var filePath in EnumerateFilesSafe(currentDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var document = TryCreateDocument(filePath, currentDepth);
                if (document is not null)
                {
                    yield return document;
                }
            }

            if (_options.Recursive && currentDepth < _options.MaxDepth)
            {
                foreach (var subDirectory in EnumerateDirectoriesSafe(currentDirectory))
                {
                    queue.Enqueue((subDirectory, currentDepth + 1));
                }
            }

            await Task.Yield();
        }
    }

    /// <summary>
    /// Validates that the configured root path exists and can be enumerated.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel validation.</param>
    /// <returns>True when the source root is accessible; otherwise false.</returns>
    public Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(_rootPath))
        {
            return Task.FromResult(false);
        }

        try
        {
            using var entries = Directory.EnumerateFileSystemEntries(_rootPath).GetEnumerator();
            _ = entries.MoveNext();
            return Task.FromResult(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Root path '{RootPath}' is not accessible.", _rootPath);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Counts documents that match the configured traversal and filter options.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel counting.</param>
    /// <returns>Total count of matching documents.</returns>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        if (!await ValidateAsync(cancellationToken).ConfigureAwait(false))
        {
            return 0;
        }

        var count = 0;
        await foreach (var _ in GetDocumentsAsync(cancellationToken).ConfigureAwait(false))
        {
            checked
            {
                count++;
            }
        }

        return count;
    }

    private SourceDocument? TryCreateDocument(string fullPath, int depth)
    {
        try
        {
            var fullFilePath = Path.GetFullPath(fullPath);
            var relativePath = Path.GetRelativePath(_rootPath, fullFilePath);
            var normalizedRelativePath = NormalizePath(relativePath);

            if (!MatchesIncludePatterns(normalizedRelativePath))
            {
                return null;
            }

            var fileInfo = new FileInfo(fullFilePath);
            if (_options.MaxFileSizeBytes.HasValue && fileInfo.Length > _options.MaxFileSizeBytes.Value)
            {
                return null;
            }

            if (!CanOpenFile(fullFilePath))
            {
                return null;
            }

            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [SourceMetadataKeys.SourcePath] = fullFilePath,
                [SourceMetadataKeys.RelativePath] = normalizedRelativePath,
                [SourceMetadataKeys.FileName] = fileInfo.Name,
                [SourceMetadataKeys.Extension] = fileInfo.Extension,
                [SourceMetadataKeys.FileSizeBytes] = fileInfo.Length.ToString(),
                [SourceMetadataKeys.CreatedUtc] = fileInfo.CreationTimeUtc.ToString("O"),
                [SourceMetadataKeys.LastModifiedUtc] = fileInfo.LastWriteTimeUtc.ToString("O"),
                [SourceMetadataKeys.Depth] = depth.ToString()
            };

            return new SourceDocument(
                id: normalizedRelativePath,
                name: fileInfo.Name,
                source: fullFilePath,
                metadata: metadata,
                openReadAsync: _ => ValueTask.FromResult<Stream>(
                    new FileStream(
                        fullFilePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 4096,
                        useAsync: true)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Skipping inaccessible file '{FilePath}'.", fullPath);
            return null;
        }
    }

    private IEnumerable<string> EnumerateFilesSafe(string directoryPath)
    {
        try
        {
            return Directory.EnumerateFiles(directoryPath, "*", TopDirectoryOnly);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Skipping inaccessible directory '{DirectoryPath}'.", directoryPath);
            return [];
        }
    }

    private IEnumerable<string> EnumerateDirectoriesSafe(string directoryPath)
    {
        try
        {
            return Directory.EnumerateDirectories(directoryPath, "*", TopDirectoryOnly);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Skipping inaccessible directory '{DirectoryPath}'.", directoryPath);
            return [];
        }
    }

    private bool MatchesIncludePatterns(string normalizedRelativePath)
    {
        if (_includePatterns.Count == 0)
        {
            return true;
        }

        var fileName = Path.GetFileName(normalizedRelativePath);

        foreach (var pattern in _includePatterns)
        {
            if (pattern.Contains('/'))
            {
                if (GlobMatch(pattern, normalizedRelativePath))
                {
                    return true;
                }
            }
            else if (GlobMatch(pattern, fileName))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanOpenFile(string fullPath)
    {
        try
        {
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 1,
                useAsync: false);
            return stream.CanRead;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Skipping inaccessible file '{FilePath}'.", fullPath);
            return false;
        }
    }

    private static List<string> BuildIncludePatterns(IReadOnlyList<string> includePatterns)
    {
        return includePatterns
            .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
            .Select(pattern => pattern.Trim())
            .Select(NormalizePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool GlobMatch(string pattern, string value)
    {
        var patternSegments = pattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var valueSegments = NormalizePath(value).Split('/', StringSplitOptions.RemoveEmptyEntries);

        return MatchPath(0, 0);

        bool MatchPath(int patternIndex, int valueIndex)
        {
            if (patternIndex == patternSegments.Length)
            {
                return valueIndex == valueSegments.Length;
            }

            if (patternSegments[patternIndex] == "**")
            {
                if (patternIndex == patternSegments.Length - 1)
                {
                    return true;
                }

                for (var i = valueIndex; i <= valueSegments.Length; i++)
                {
                    if (MatchPath(patternIndex + 1, i))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (valueIndex >= valueSegments.Length)
            {
                return false;
            }

            return MatchSegment(patternSegments[patternIndex], valueSegments[valueIndex])
                   && MatchPath(patternIndex + 1, valueIndex + 1);
        }
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static bool MatchSegment(string pattern, string value)
    {
        var p = 0;
        var v = 0;
        var star = -1;
        var valueAfterStar = -1;

        while (v < value.Length)
        {
            if (p < pattern.Length
                && (pattern[p] == '?'
                    || char.ToUpperInvariant(pattern[p]) == char.ToUpperInvariant(value[v])))
            {
                p++;
                v++;
                continue;
            }

            if (p < pattern.Length && pattern[p] == '*')
            {
                star = p++;
                valueAfterStar = v;
                continue;
            }

            if (star != -1)
            {
                p = star + 1;
                v = ++valueAfterStar;
                continue;
            }

            return false;
        }

        while (p < pattern.Length && pattern[p] == '*')
        {
            p++;
        }

        return p == pattern.Length;
    }
}
