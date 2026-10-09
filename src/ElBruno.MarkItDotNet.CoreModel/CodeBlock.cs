// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A fenced or indented code block.
/// </summary>
public record CodeBlock : DocumentBlock
{
    /// <summary>
    /// The code content without the surrounding fence.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// The optional language or info string declared by the source.
    /// </summary>
    public string? Language { get; init; }
}
