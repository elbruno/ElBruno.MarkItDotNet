// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Root document model representing a fully parsed document with sections,
/// metadata, and source tracking information.
/// </summary>
public record Document
{
    /// <summary>
    /// Unique identifier for this document, typically a deterministic hash of content.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Top-level sections that comprise the document structure.
    /// </summary>
    public IReadOnlyList<DocumentSection> Sections { get; init; } = [];

    /// <summary>
    /// Document-level metadata such as title, author, and format information.
    /// </summary>
    public DocumentMetadata Metadata { get; init; } = new();

    /// <summary>
    /// Optional reference to the original source file or location.
    /// </summary>
    public SourceReference? Source { get; init; }
}
