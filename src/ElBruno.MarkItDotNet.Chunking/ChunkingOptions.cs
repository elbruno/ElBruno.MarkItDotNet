// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// Configuration options for chunking strategies, including size limits,
/// overlap settings, and atomicity preservation rules.
/// </summary>
public class ChunkingOptions
{
    /// <summary>
    /// Gets or sets the maximum chunk size (in characters or tokens, depending on strategy).
    /// Default is 512.
    /// </summary>
    public int MaxChunkSize { get; set; } = 512;

    /// <summary>
    /// Gets or sets the overlap size between consecutive chunks (in characters or tokens).
    /// Default is 50.
    /// </summary>
    public int OverlapSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets a value indicating whether tables should be kept whole and not split across chunks.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool PreserveTableAtomicity { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether figures should be kept whole and not split across chunks.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool PreserveFigureAtomicity { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom token counter function. When <see langword="null"/>,
    /// a default word-count estimator is used.
    /// </summary>
    public Func<string, int>? TokenCounter { get; set; }
}
