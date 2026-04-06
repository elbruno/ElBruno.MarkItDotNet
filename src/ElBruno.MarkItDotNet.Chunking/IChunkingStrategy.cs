// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// Defines a strategy for splitting a <see cref="Document"/> into chunks
/// suitable for AI/search ingestion pipelines.
/// </summary>
public interface IChunkingStrategy
{
    /// <summary>
    /// Gets the human-readable name of this chunking strategy.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Splits the specified document into chunks using this strategy.
    /// </summary>
    /// <param name="document">The document to chunk.</param>
    /// <param name="options">Optional chunking configuration. When <see langword="null"/>, defaults are used.</param>
    /// <returns>An ordered list of chunk results.</returns>
    IReadOnlyList<ChunkResult> Chunk(Document document, ChunkingOptions? options = null);
}
