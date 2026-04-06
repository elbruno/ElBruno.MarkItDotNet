// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents;

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Wraps <see cref="SearchClient"/> for batch document uploads to Azure AI Search.
/// </summary>
public class SearchIndexUploader
{
    private readonly SearchClient _searchClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchIndexUploader"/> class.
    /// </summary>
    /// <param name="searchClient">The Azure AI Search client for the target index.</param>
    public SearchIndexUploader(SearchClient searchClient)
    {
        ArgumentNullException.ThrowIfNull(searchClient);
        _searchClient = searchClient;
    }

    /// <summary>
    /// Uploads documents to Azure AI Search in batches.
    /// </summary>
    /// <param name="documents">The documents to upload.</param>
    /// <param name="batchSize">The maximum number of documents per batch (default 100).</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An <see cref="UploadResult"/> summarizing successes and failures.</returns>
    public async Task<UploadResult> UploadAsync(
        IEnumerable<SearchDocument> documents,
        int batchSize = 100,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);

        var totalCount = 0;
        var errors = new List<string>();

        var batch = new List<SearchDocument>(batchSize);

        foreach (var doc in documents)
        {
            batch.Add(doc);
            totalCount++;

            if (batch.Count >= batchSize)
            {
                await UploadBatchAsync(batch, errors, ct).ConfigureAwait(false);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await UploadBatchAsync(batch, errors, ct).ConfigureAwait(false);
        }

        return new UploadResult
        {
            SuccessCount = totalCount - errors.Count,
            FailureCount = errors.Count,
            Errors = errors.AsReadOnly(),
        };
    }

    private async Task UploadBatchAsync(
        List<SearchDocument> batch,
        List<string> errors,
        CancellationToken ct)
    {
        try
        {
            var response = await _searchClient.UploadDocumentsAsync(batch, cancellationToken: ct).ConfigureAwait(false);

            foreach (var result in response.Value.Results)
            {
                if (!result.Succeeded)
                {
                    errors.Add($"Document '{result.Key}' failed: {result.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Batch upload failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Represents the result of a batch document upload operation.
/// </summary>
public record UploadResult
{
    /// <summary>Gets the number of documents successfully uploaded.</summary>
    public int SuccessCount { get; init; }

    /// <summary>Gets the number of documents that failed to upload.</summary>
    public int FailureCount { get; init; }

    /// <summary>Gets the error messages for failed uploads.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];
}
