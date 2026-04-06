// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Represents the index document shape for Azure AI Search,
/// containing content, metadata, vector embeddings, and citation information.
/// </summary>
public record SearchDocument
{
    /// <summary>Gets the unique document key for Azure AI Search.</summary>
    [SimpleField(IsKey = true, IsFilterable = true)]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the text content of the chunk.</summary>
    [SearchableField(AnalyzerName = "en.microsoft")]
    public string Content { get; init; } = string.Empty;

    /// <summary>Gets the document title.</summary>
    [SearchableField(AnalyzerName = "en.microsoft")]
    public string? Title { get; init; }

    /// <summary>Gets the heading hierarchy path (e.g., "Chapter 1 > Section 1.1").</summary>
    [SearchableField(AnalyzerName = "en.microsoft")]
    public string? HeadingPath { get; init; }

    /// <summary>Gets the source file path.</summary>
    [SimpleField(IsFilterable = true, IsSortable = true)]
    public string? FilePath { get; init; }

    /// <summary>Gets the page number in the source document.</summary>
    [SimpleField(IsFilterable = true, IsSortable = true)]
    public int? PageNumber { get; init; }

    /// <summary>Gets the zero-based chunk index within the document.</summary>
    [SimpleField(IsFilterable = true, IsSortable = true)]
    public int ChunkIndex { get; init; }

    /// <summary>Gets the parent document identifier.</summary>
    [SimpleField(IsFilterable = true)]
    public string? DocumentId { get; init; }

    /// <summary>Gets the vector embedding for semantic search.</summary>
    [VectorSearchField(VectorSearchDimensions = 1536, VectorSearchProfileName = SearchIndexSchemaBuilder.VectorSearchProfileName)]
    public ReadOnlyMemory<float>? ContentVector { get; init; }

    /// <summary>Gets the filterable tags associated with this document.</summary>
    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public ICollection<string> Tags { get; init; } = [];

    /// <summary>Gets the serialized metadata JSON.</summary>
    [SearchableField]
    public string? Metadata { get; init; }

    /// <summary>Gets the human-readable citation text.</summary>
    [SearchableField]
    public string? CitationText { get; init; }

    /// <summary>Gets the timestamp of the last update.</summary>
    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;
}
