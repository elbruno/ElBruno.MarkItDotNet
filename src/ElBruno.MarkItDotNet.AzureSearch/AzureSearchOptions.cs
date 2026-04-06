// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Configuration options for Azure AI Search integration.
/// </summary>
public class AzureSearchOptions
{
    /// <summary>Gets or sets the Azure AI Search service endpoint URL.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional API key for authentication. When null, Azure Identity (DefaultAzureCredential) is used.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Gets or sets the name of the search index.</summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of vector dimensions (default 1536 for OpenAI ada-002).</summary>
    public int VectorDimensions { get; set; } = 1536;

    /// <summary>Gets or sets the maximum number of documents per upload batch.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Validates the options and throws if required values are missing or invalid.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new InvalidOperationException($"{nameof(Endpoint)} is required.");
        }

        if (string.IsNullOrWhiteSpace(IndexName))
        {
            throw new InvalidOperationException($"{nameof(IndexName)} is required.");
        }

        if (VectorDimensions <= 0)
        {
            throw new InvalidOperationException($"{nameof(VectorDimensions)} must be greater than zero.");
        }

        if (BatchSize <= 0)
        {
            throw new InvalidOperationException($"{nameof(BatchSize)} must be greater than zero.");
        }
    }
}
