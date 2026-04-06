// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Default implementation of <see cref="ISearchDocumentMapper"/> that maps
/// <see cref="ChunkResult"/> instances with optional document and citation context
/// to <see cref="SearchDocument"/> instances.
/// </summary>
public class DefaultSearchDocumentMapper : ISearchDocumentMapper
{
    /// <inheritdoc />
    public SearchDocument Map(ChunkResult chunk, Document? document = null, CitationReference? citation = null)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        var metadata = chunk.Metadata.Count > 0
            ? JsonSerializer.Serialize(chunk.Metadata)
            : null;

        var citationText = citation is not null
            ? CitationFormatter.Format(citation)
            : null;

        return new SearchDocument
        {
            Id = chunk.Id,
            Content = chunk.Content,
            Title = document?.Metadata.Title,
            HeadingPath = chunk.HeadingPath ?? citation?.HeadingPath,
            FilePath = citation?.FilePath ?? document?.Source?.FilePath,
            PageNumber = citation?.PageNumber,
            ChunkIndex = chunk.Index,
            DocumentId = document?.Id,
            Metadata = metadata,
            CitationText = citationText,
            LastUpdated = DateTimeOffset.UtcNow,
        };
    }
}
