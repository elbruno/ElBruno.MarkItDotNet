// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Represents a character span within source content, defined by offset and length.
/// </summary>
public record SpanReference
{
    /// <summary>
    /// Zero-based character offset from the start of the source content.
    /// </summary>
    public int Offset { get; init; }

    /// <summary>
    /// Length of the span in characters.
    /// </summary>
    public int Length { get; init; }
}
