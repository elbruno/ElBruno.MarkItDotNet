// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A single item in a list, with optional nested sub-items for hierarchical lists.
/// </summary>
public record ListItemBlock : DocumentBlock
{
    /// <summary>
    /// The text content of this list item.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Nested sub-items under this list item.
    /// </summary>
    public IReadOnlyList<ListItemBlock> SubItems { get; init; } = [];

    /// <summary>
    /// Gets whether this item is a checked or unchecked task list item.
    /// A null value indicates a regular list item.
    /// </summary>
    public bool? IsChecked { get; init; }
}
