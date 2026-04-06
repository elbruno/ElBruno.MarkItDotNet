// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.VectorData;

/// <summary>
/// Default implementation of <see cref="IVectorRecordMapper"/> that extracts all
/// available metadata from <see cref="ChunkResult"/> and <see cref="Document"/>.
/// </summary>
public class DefaultVectorRecordMapper : IVectorRecordMapper
{
    /// <inheritdoc />
    public VectorRecord MapChunk(ChunkResult chunk, Document? document = null)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        var firstSource = chunk.Sources.FirstOrDefault();

        return new VectorRecord
        {
            Id = chunk.Id,
            Content = chunk.Content,
            ChunkIndex = chunk.Index,
            HeadingPath = chunk.HeadingPath,
            PageNumber = firstSource?.PageNumber,
            FilePath = firstSource?.FilePath ?? document?.Source?.FilePath,
            DocumentId = document?.Id,
            DocumentTitle = document?.Metadata.Title,
            Tags = ExtractTags(chunk, document),
            Metadata = new Dictionary<string, object>(chunk.Metadata),
        };
    }

    private static List<string> ExtractTags(ChunkResult chunk, Document? document)
    {
        var tags = new List<string>();

        if (document?.Metadata.SourceFormat is { } format)
        {
            tags.Add($"format:{format}");
        }

        if (chunk.HeadingPath is { Length: > 0 })
        {
            tags.Add("has-heading");
        }

        if (chunk.Sources.Count > 0 && chunk.Sources[0].PageNumber.HasValue)
        {
            tags.Add($"page:{chunk.Sources[0].PageNumber}");
        }

        return tags;
    }
}
