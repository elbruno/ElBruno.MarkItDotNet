// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A figure block representing an image with optional alt text, caption, and file path.
/// </summary>
public record FigureBlock : DocumentBlock
{
    /// <summary>
    /// Alternative text for the image, used for accessibility.
    /// </summary>
    public string? AltText { get; init; }

    /// <summary>
    /// Optional caption displayed below the figure.
    /// </summary>
    public string? Caption { get; init; }

    /// <summary>
    /// Path or URL to the image file.
    /// </summary>
    public string? ImagePath { get; init; }
}
