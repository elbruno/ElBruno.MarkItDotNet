// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.VectorData;

/// <summary>
/// Defines a mapper that converts a <see cref="ChunkResult"/> into a <see cref="VectorRecord"/>
/// suitable for vector store ingestion.
/// </summary>
public interface IVectorRecordMapper
{
    /// <summary>
    /// Maps a chunk result and optional source document to a vector record.
    /// </summary>
    /// <param name="chunk">The chunk result to map.</param>
    /// <param name="document">The optional source document for additional metadata.</param>
    /// <returns>A <see cref="VectorRecord"/> populated with chunk and document metadata.</returns>
    VectorRecord MapChunk(ChunkResult chunk, Document? document = null);
}
