// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Abstract base record for all document content blocks.
/// Uses JSON polymorphism to support serialization of derived block types.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ParagraphBlock), "paragraph")]
[JsonDerivedType(typeof(HeadingBlock), "heading")]
[JsonDerivedType(typeof(TableBlock), "table")]
[JsonDerivedType(typeof(FigureBlock), "figure")]
[JsonDerivedType(typeof(ListBlock), "list")]
[JsonDerivedType(typeof(ListItemBlock), "listItem")]
public abstract record DocumentBlock
{
    /// <summary>
    /// Unique identifier for this block.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Optional reference to the source location of this block.
    /// </summary>
    public SourceReference? Source { get; init; }

    /// <summary>
    /// Extensible property bag for additional block-specific metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; init; } = new Dictionary<string, object>();
}
