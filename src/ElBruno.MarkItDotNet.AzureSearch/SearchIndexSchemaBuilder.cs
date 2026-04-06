// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Builds a <see cref="SearchIndex"/> definition for the <see cref="SearchDocument"/> shape
/// with appropriate field types and vector search configuration.
/// </summary>
public static class SearchIndexSchemaBuilder
{
    /// <summary>The name of the HNSW algorithm configuration used for vector search.</summary>
    public const string HnswAlgorithmConfigName = "hnsw-config";

    /// <summary>The name of the vector search profile.</summary>
    public const string VectorSearchProfileName = "vector-profile";

    /// <summary>
    /// Builds a <see cref="SearchIndex"/> with fields matching the <see cref="SearchDocument"/> shape
    /// and HNSW vector search configuration.
    /// </summary>
    /// <param name="indexName">The name of the Azure AI Search index.</param>
    /// <param name="vectorDimensions">The number of dimensions for vector embeddings (default 1536 for OpenAI ada-002).</param>
    /// <returns>A configured <see cref="SearchIndex"/> ready for creation.</returns>
    public static SearchIndex Build(string indexName, int vectorDimensions = 1536)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vectorDimensions, 0);

        var fields = new FieldBuilder().Build(typeof(SearchDocument));

        // Replace the ContentVector field with the correct dimensions
        for (var i = 0; i < fields.Count; i++)
        {
            if (fields[i].Name == nameof(SearchDocument.ContentVector))
            {
                fields[i] = new VectorSearchField(nameof(SearchDocument.ContentVector), vectorDimensions, VectorSearchProfileName);
                break;
            }
        }

        var index = new SearchIndex(indexName, fields)
        {
            VectorSearch = new VectorSearch
            {
                Algorithms =
                {
                    new HnswAlgorithmConfiguration(HnswAlgorithmConfigName),
                },
                Profiles =
                {
                    new VectorSearchProfile(VectorSearchProfileName, HnswAlgorithmConfigName),
                },
            },
        };

        return index;
    }
}
