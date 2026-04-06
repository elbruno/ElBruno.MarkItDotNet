// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A heading block with text and a level (1-6) corresponding to Markdown heading depth.
/// </summary>
public record HeadingBlock : DocumentBlock
{
    /// <summary>
    /// The text content of the heading.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// The heading level (1 for h1, 2 for h2, etc.).
    /// </summary>
    public int Level { get; init; }
}
