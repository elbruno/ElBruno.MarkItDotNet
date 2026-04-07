namespace ElBruno.MarkItDotNet;

/// <summary>
/// A stream wrapper that throws when the read limit is exceeded.
/// Used to enforce file size limits on non-seekable streams.
/// </summary>
internal sealed class LimitedStream : Stream
{
    private readonly Stream _inner;
    private readonly long _maxBytes;
    private long _totalBytesRead;

    /// <summary>
    /// Initializes a new instance of <see cref="LimitedStream"/>.
    /// </summary>
    /// <param name="inner">The underlying stream to wrap.</param>
    /// <param name="maxBytes">Maximum number of bytes allowed to be read.</param>
    public LimitedStream(Stream inner, long maxBytes)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _maxBytes = maxBytes > 0 ? maxBytes : throw new ArgumentOutOfRangeException(nameof(maxBytes));
    }

    /// <inheritdoc />
    public override bool CanRead => _inner.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = _inner.Read(buffer, offset, count);
        _totalBytesRead += bytesRead;

        if (_totalBytesRead > _maxBytes)
        {
            throw new InvalidOperationException(
                $"Stream exceeds the maximum allowed size of {_maxBytes} bytes.");
        }

        return bytesRead;
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var bytesRead = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        _totalBytesRead += bytesRead;

        if (_totalBytesRead > _maxBytes)
        {
            throw new InvalidOperationException(
                $"Stream exceeds the maximum allowed size of {_maxBytes} bytes.");
        }

        return bytesRead;
    }

    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var bytesRead = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        _totalBytesRead += bytesRead;

        if (_totalBytesRead > _maxBytes)
        {
            throw new InvalidOperationException(
                $"Stream exceeds the maximum allowed size of {_maxBytes} bytes.");
        }

        return bytesRead;
    }

    /// <inheritdoc />
    public override void Flush() => _inner.Flush();

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }

        base.Dispose(disposing);
    }
}
