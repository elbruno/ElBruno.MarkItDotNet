using ElBruno.MarkItDotNet.Connectors;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Wraps an <see cref="IDocumentSource"/> and filters documents using a configurable predicate
/// applied to document metadata (name, source path, metadata keys) before opening streams.
/// An optional post-conversion policy can further gate documents by their Markdown content.
/// </summary>
public sealed class PolicyFilteredDocumentSource : IDocumentSource
{
    private readonly IDocumentSource _inner;
    private readonly Func<SourceDocument, bool>? _metadataPredicate;
    private readonly ISecurityPolicy? _contentPolicy;
    private readonly MarkdownService? _converter;

    /// <summary>
    /// Creates a filter that applies only a metadata predicate (no stream opened for excluded docs).
    /// </summary>
    /// <param name="inner">The underlying document source.</param>
    /// <param name="metadataPredicate">
    /// Returns <c>true</c> to include a document, <c>false</c> to skip it.
    /// Evaluated against name, source path, and metadata dictionary before opening the stream.
    /// </param>
    public PolicyFilteredDocumentSource(
        IDocumentSource inner,
        Func<SourceDocument, bool> metadataPredicate)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _metadataPredicate = metadataPredicate ?? throw new ArgumentNullException(nameof(metadataPredicate));
    }

    /// <summary>
    /// Creates a filter that applies a metadata predicate and, for passing documents,
    /// also converts and evaluates a content policy.
    /// Documents whose converted Markdown fails the policy are excluded from the stream.
    /// </summary>
    /// <param name="inner">The underlying document source.</param>
    /// <param name="converter">Used to convert document streams to Markdown for policy evaluation.</param>
    /// <param name="contentPolicy">Policy evaluated against the converted Markdown.</param>
    /// <param name="metadataPredicate">Optional metadata pre-filter (null = pass all through to content check).</param>
    public PolicyFilteredDocumentSource(
        IDocumentSource inner,
        MarkdownService converter,
        ISecurityPolicy contentPolicy,
        Func<SourceDocument, bool>? metadataPredicate = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _contentPolicy = contentPolicy ?? throw new ArgumentNullException(nameof(contentPolicy));
        _metadataPredicate = metadataPredicate;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<SourceDocument> GetDocumentsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        await foreach (var doc in _inner.GetDocumentsAsync(cancellationToken).ConfigureAwait(false))
        {
            // 1. Metadata filter (cheap — no stream open)
            if (_metadataPredicate is not null && !_metadataPredicate(doc))
                continue;

            // 2. Content policy (requires conversion — only when configured)
            if (_contentPolicy is not null && _converter is not null)
            {
                var ext = Path.GetExtension(doc.Name).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext))
                    ext = Path.GetExtension(doc.Source).ToLowerInvariant();

                if (!string.IsNullOrEmpty(ext))
                {
                    await using var stream = await doc.OpenReadAsync(cancellationToken).ConfigureAwait(false);
                    var conversion = await _converter.ConvertAsync(stream, ext, cancellationToken).ConfigureAwait(false);

                    if (conversion.Success)
                    {
                        var policyResult = await _contentPolicy.EvaluateAsync(
                            conversion.Markdown, cancellationToken).ConfigureAwait(false);

                        if (!policyResult.Passed) continue;
                    }
                }
            }

            yield return doc;
        }
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = 0;
        await foreach (var _ in GetDocumentsAsync(cancellationToken).ConfigureAwait(false))
            count++;
        return count;
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
        => await _inner.ValidateAsync(cancellationToken).ConfigureAwait(false);
}
