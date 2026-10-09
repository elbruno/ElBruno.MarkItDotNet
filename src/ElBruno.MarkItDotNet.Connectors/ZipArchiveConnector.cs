// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// Reads files from a ZIP archive as an <see cref="IDocumentSource"/> without extracting them to disk.
/// </summary>
public sealed class ZipArchiveConnector : IDocumentSource
{
    private readonly ZipArchiveConnectorOptions _options;
    private readonly ILogger<ZipArchiveConnector> _logger;
    private readonly string _archivePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipArchiveConnector"/> class.
    /// </summary>
    /// <param name="options">Connector options.</param>
    /// <param name="logger">Logger for warning and diagnostic messages.</param>
    public ZipArchiveConnector(
        ZipArchiveConnectorOptions options,
        ILogger<ZipArchiveConnector> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ArchivePath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxArchiveSizeBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxEntrySizeBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxTotalUncompressedBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxEntries);

        _options = options;
        _logger = logger;
        _archivePath = Path.GetFullPath(options.ArchivePath);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SourceDocument> GetDocumentsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EntryDescriptor[] entries;
        using (var archive = OpenArchive())
        {
            entries = GetEligibleEntries(archive, cancellationToken);
        }

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var displayName = Path.GetFileName(entry.Name.Replace('\\', '/'));
            var source = $"{_archivePath}!/{entry.Name}";
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [SourceMetadataKeys.ArchivePath] = _archivePath,
                [SourceMetadataKeys.ArchiveEntryPath] = entry.Name,
                [SourceMetadataKeys.FileName] = displayName,
                [SourceMetadataKeys.FileSizeBytes] = entry.Length.ToString(),
                [SourceMetadataKeys.LastModifiedUtc] = entry.LastModifiedUtc.ToString("O")
            };

            yield return new SourceDocument(
                id: $"{entry.Index}:{entry.Name}",
                name: displayName,
                source: source,
                metadata: metadata,
                openReadAsync: token => OpenEntryAsync(entry.Index, entry.Name, entry.Length, token));

            await Task.Yield();
        }
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
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

    /// <inheritdoc />
    public Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var archive = OpenArchive();
            _ = GetEligibleEntries(archive, cancellationToken);
            return Task.FromResult(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            _logger.LogWarning(ex, "ZIP archive '{ArchivePath}' is not accessible or valid.", _archivePath);
            return Task.FromResult(false);
        }
    }

    private ZipArchive OpenArchive()
    {
        var fileInfo = new FileInfo(_archivePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"ZIP archive '{_archivePath}' does not exist.", _archivePath);
        }

        if (fileInfo.Length > _options.MaxArchiveSizeBytes)
        {
            throw new InvalidDataException(
                $"ZIP archive size {fileInfo.Length} bytes exceeds the configured limit of {_options.MaxArchiveSizeBytes} bytes.");
        }

        return ZipFile.OpenRead(_archivePath);
    }

    private EntryDescriptor[] GetEligibleEntries(ZipArchive archive, CancellationToken cancellationToken)
    {
        if (archive.Entries.Count > _options.MaxEntries)
        {
            throw new InvalidDataException(
                $"Archive contains {archive.Entries.Count} entries, exceeding the configured limit of {_options.MaxEntries}.");
        }

        var entries = new List<EntryDescriptor>();
        long totalUncompressedBytes = 0;
        for (var index = 0; index < archive.Entries.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = archive.Entries[index];
            if (IsDirectory(entry))
            {
                continue;
            }

            if (entry.Length > _options.MaxEntrySizeBytes)
            {
                _logger.LogWarning(
                    "Skipping ZIP entry '{EntryName}' with size {EntrySize} bytes, exceeding the configured limit of {MaxEntrySize} bytes.",
                    entry.FullName,
                    entry.Length,
                    _options.MaxEntrySizeBytes);
                continue;
            }

            if (entry.Length > _options.MaxTotalUncompressedBytes - totalUncompressedBytes)
            {
                throw new InvalidDataException(
                    $"Archive entries exceed the configured total uncompressed size limit of {_options.MaxTotalUncompressedBytes} bytes.");
            }

            totalUncompressedBytes += entry.Length;
            entries.Add(new EntryDescriptor(index, entry.FullName, entry.Length, entry.LastWriteTime.UtcDateTime));
        }

        return entries.ToArray();
    }

    private ValueTask<Stream> OpenEntryAsync(
        int entryIndex,
        string entryName,
        long entryLength,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        FileStream? archiveStream = null;
        ZipArchive? archive = null;
        try
        {
            archiveStream = new FileStream(
                _archivePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);
            archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);

            if (archive.Entries.Count > _options.MaxEntries
                || entryIndex >= archive.Entries.Count
                || !string.Equals(archive.Entries[entryIndex].FullName, entryName, StringComparison.Ordinal)
                || archive.Entries[entryIndex].Length != entryLength)
            {
                throw new InvalidDataException("The ZIP archive changed after its entries were enumerated.");
            }

            var entryStream = archive.Entries[entryIndex].Open();
            return ValueTask.FromResult<Stream>(
                new LimitedEntryStream(entryStream, archive, entryLength, _options.MaxEntrySizeBytes));
        }
        catch
        {
            archive?.Dispose();
            if (archive is null)
            {
                archiveStream?.Dispose();
            }

            throw;
        }
    }

    private static bool IsDirectory(ZipArchiveEntry entry) =>
        entry.FullName.EndsWith("/", StringComparison.Ordinal)
        || entry.FullName.EndsWith('\\');

    private sealed record EntryDescriptor(int Index, string Name, long Length, DateTime LastModifiedUtc);

    private sealed class LimitedEntryStream(
        Stream entryStream,
        ZipArchive archive,
        long length,
        long maxBytes) : Stream
    {
        private long _bytesRead;

        public override bool CanRead => entryStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => length;

        public override long Position
        {
            get => _bytesRead;
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (offset > buffer.Length - count)
            {
                throw new ArgumentException("Offset and count exceed the buffer bounds.");
            }

            var bytesRead = entryStream.Read(buffer, offset, LimitReadSize(count));
            return RecordRead(bytesRead);
        }

        public override int Read(Span<byte> buffer)
        {
            var bytesRead = entryStream.Read(buffer[..LimitReadSize(buffer.Length)]);
            return RecordRead(bytesRead);
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var bytesRead = await entryStream.ReadAsync(
                buffer[..LimitReadSize(buffer.Length)],
                cancellationToken).ConfigureAwait(false);
            return RecordRead(bytesRead);
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    entryStream.Dispose();
                }
                finally
                {
                    archive.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                await entryStream.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                archive.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private int LimitReadSize(int requested)
        {
            if (requested == 0)
            {
                return 0;
            }

            var remainingBytes = maxBytes - _bytesRead;
            return remainingBytes <= 0
                ? 1
                : (int)Math.Min(requested, remainingBytes + 1);
        }

        private int RecordRead(int bytesRead)
        {
            if (bytesRead > maxBytes - _bytesRead)
            {
                throw new InvalidDataException(
                    $"ZIP entry exceeded the configured uncompressed size limit of {maxBytes} bytes.");
            }

            _bytesRead += bytesRead;
            return bytesRead;
        }
    }
}
