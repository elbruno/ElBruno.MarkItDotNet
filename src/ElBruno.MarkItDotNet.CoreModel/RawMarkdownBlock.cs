// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Markdown source retained verbatim when no dedicated model block exists.
/// </summary>
public record RawMarkdownBlock : DocumentBlock
{
    /// <summary>
    /// Gets the original Markdown for this block.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the source block's AST type name.
    /// </summary>
    public string? BlockType { get; init; }
}
