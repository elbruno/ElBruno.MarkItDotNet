// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Search.Documents.Indexes;

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Wraps <see cref="SearchIndexClient"/> for Azure AI Search index lifecycle management.
/// </summary>
public class SearchIndexManager
{
    private readonly SearchIndexClient _indexClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchIndexManager"/> class.
    /// </summary>
    /// <param name="indexClient">The Azure AI Search index management client.</param>
    public SearchIndexManager(SearchIndexClient indexClient)
    {
        ArgumentNullException.ThrowIfNull(indexClient);
        _indexClient = indexClient;
    }

    /// <summary>
    /// Creates or updates an Azure AI Search index with the <see cref="SearchDocument"/> schema.
    /// </summary>
    /// <param name="indexName">The name of the index to create or update.</param>
    /// <param name="vectorDimensions">The number of dimensions for vector embeddings.</param>
    /// <param name="ct">A cancellation token.</param>
    public async Task CreateOrUpdateIndexAsync(
        string indexName,
        int vectorDimensions = 1536,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var index = SearchIndexSchemaBuilder.Build(indexName, vectorDimensions);
        await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes an Azure AI Search index.
    /// </summary>
    /// <param name="indexName">The name of the index to delete.</param>
    /// <param name="ct">A cancellation token.</param>
    public async Task DeleteIndexAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        await _indexClient.DeleteIndexAsync(indexName, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks whether an Azure AI Search index exists.
    /// </summary>
    /// <param name="indexName">The name of the index to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns><c>true</c> if the index exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        try
        {
            await _indexClient.GetIndexAsync(indexName, ct).ConfigureAwait(false);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
