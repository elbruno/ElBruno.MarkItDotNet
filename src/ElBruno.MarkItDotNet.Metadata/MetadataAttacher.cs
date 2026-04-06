// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Helper methods for attaching extracted metadata back to documents and chunks.
/// </summary>
public static class MetadataAttacher
{
    /// <summary>
    /// Returns a new <see cref="Document"/> with enriched metadata from the given <see cref="MetadataResult"/>.
    /// </summary>
    /// <param name="document">The original document.</param>
    /// <param name="result">The metadata result to attach.</param>
    /// <returns>A new document with updated metadata.</returns>
    public static Document AttachToDocument(Document document, MetadataResult result)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(result);

        var custom = new Dictionary<string, object>(document.Metadata.Custom);

        if (result.Language is not null)
        {
            custom["Language"] = result.Language;
        }

        if (result.DocumentType != DocumentType.Unknown)
        {
            custom["DocumentType"] = result.DocumentType.ToString();
        }

        if (result.Tags.Count > 0)
        {
            custom["Tags"] = result.Tags.ToList();
        }

        if (result.HeadingCount > 0)
        {
            custom["HeadingCount"] = result.HeadingCount;
        }

        foreach (var (key, value) in result.Custom)
        {
            custom[key] = value;
        }

        var updatedMetadata = document.Metadata with
        {
            Title = result.Title ?? document.Metadata.Title,
            Author = result.Author ?? document.Metadata.Author,
            WordCount = result.WordCount > 0 ? result.WordCount : document.Metadata.WordCount,
            Custom = custom,
        };

        return document with { Metadata = updatedMetadata };
    }

    /// <summary>
    /// Returns chunks with document-level metadata merged into each chunk's metadata dictionary.
    /// </summary>
    /// <param name="chunks">The original chunks.</param>
    /// <param name="result">The metadata result to attach.</param>
    /// <returns>New chunk results with enriched metadata.</returns>
    public static IReadOnlyList<ChunkResult> AttachToChunks(
        IReadOnlyList<ChunkResult> chunks,
        MetadataResult result)
    {
        ArgumentNullException.ThrowIfNull(chunks);
        ArgumentNullException.ThrowIfNull(result);

        var enrichedChunks = new List<ChunkResult>(chunks.Count);

        foreach (var chunk in chunks)
        {
            var metadata = new Dictionary<string, object>(chunk.Metadata);

            if (result.Title is not null)
            {
                metadata["DocumentTitle"] = result.Title;
            }

            if (result.Author is not null)
            {
                metadata["DocumentAuthor"] = result.Author;
            }

            if (result.Language is not null)
            {
                metadata["DocumentLanguage"] = result.Language;
            }

            if (result.DocumentType != DocumentType.Unknown)
            {
                metadata["DocumentType"] = result.DocumentType.ToString();
            }

            enrichedChunks.Add(chunk with { Metadata = metadata });
        }

        return enrichedChunks;
    }
}
