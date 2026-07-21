// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// Defines a source of documents that can be enumerated asynchronously.
/// </summary>
public interface IDocumentSource
    : IAsyncEnumerable<SourceDocument>
{
    /// <summary>
    /// Enumerates source documents as an async stream.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel enumeration.</param>
    /// <returns>An async stream of source documents.</returns>
    IAsyncEnumerable<SourceDocument> GetDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of documents currently available from the source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel counting.</param>
    /// <returns>Total number of discoverable documents.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the source is accessible with the current configuration.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel validation.</param>
    /// <returns><c>true</c> when the source can be accessed; otherwise <c>false</c>.</returns>
    Task<bool> ValidateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Allows direct <c>await foreach</c> enumeration over the source.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel enumeration.</param>
    /// <returns>An async enumerator of source documents.</returns>
    async IAsyncEnumerator<SourceDocument> IAsyncEnumerable<SourceDocument>.GetAsyncEnumerator(
        CancellationToken cancellationToken)
    {
        await foreach (var item in GetDocumentsAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }
}
