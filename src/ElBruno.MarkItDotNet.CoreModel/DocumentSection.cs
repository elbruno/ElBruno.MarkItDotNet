// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Represents a logical section of a document, defined by an optional heading
/// and containing child blocks and nested subsections.
/// </summary>
public record DocumentSection
{
    /// <summary>
    /// Unique identifier for this section.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Optional heading that introduces this section.
    /// </summary>
    public HeadingBlock? Heading { get; init; }

    /// <summary>
    /// Content blocks within this section (paragraphs, tables, figures, etc.).
    /// </summary>
    public IReadOnlyList<DocumentBlock> Blocks { get; init; } = [];

    /// <summary>
    /// Nested subsections within this section, forming a hierarchy.
    /// </summary>
    public IReadOnlyList<DocumentSection> SubSections { get; init; } = [];
}
