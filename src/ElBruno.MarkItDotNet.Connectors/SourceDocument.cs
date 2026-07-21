// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// Represents a document discovered by a connector, with metadata and deferred content access.
/// </summary>
public sealed class SourceDocument
{
    private readonly Func<CancellationToken, ValueTask<Stream>> _openReadAsync;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceDocument"/> class.
    /// </summary>
    /// <param name="id">A stable identifier for the document within the source.</param>
    /// <param name="name">Display name for the document.</param>
    /// <param name="source">Source-specific location, such as a file path or URL.</param>
    /// <param name="metadata">Document metadata values.</param>
    /// <param name="openReadAsync">Factory used to open a readable content stream on demand.</param>
    public SourceDocument(
        string id,
        string name,
        string source,
        IReadOnlyDictionary<string, string> metadata,
        Func<CancellationToken, ValueTask<Stream>> openReadAsync)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(openReadAsync);

        Id = id;
        Name = name;
        Source = source;
        Metadata = metadata;
        _openReadAsync = openReadAsync;
    }

    /// <summary>
    /// Gets a stable identifier for the document within the source.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name for the document.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the source-specific location, such as a file path or URL.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets source-specific metadata extracted during discovery.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Opens a readable content stream for this document.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel stream creation.</param>
    /// <returns>A readable content stream.</returns>
    public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
    {
        return _openReadAsync(cancellationToken);
    }
}
