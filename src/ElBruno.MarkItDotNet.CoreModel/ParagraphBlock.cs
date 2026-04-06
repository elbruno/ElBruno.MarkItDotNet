// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A paragraph block containing plain or formatted text content.
/// </summary>
public record ParagraphBlock : DocumentBlock
{
    /// <summary>
    /// The text content of the paragraph.
    /// </summary>
    public string Text { get; init; } = string.Empty;
}
