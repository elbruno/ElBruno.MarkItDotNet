// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Maps chunks and their associated metadata to <see cref="SearchDocument"/> instances
/// suitable for indexing in Azure AI Search.
/// </summary>
public interface ISearchDocumentMapper
{
    /// <summary>
    /// Maps a <see cref="ChunkResult"/> with optional document and citation context
    /// to a <see cref="SearchDocument"/>.
    /// </summary>
    /// <param name="chunk">The chunk to map.</param>
    /// <param name="document">The source document for additional metadata.</param>
    /// <param name="citation">An optional citation reference for source traceability.</param>
    /// <returns>A <see cref="SearchDocument"/> ready for indexing.</returns>
    SearchDocument Map(ChunkResult chunk, Document? document = null, CitationReference? citation = null);
}
