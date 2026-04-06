// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A list block containing ordered or unordered list items.
/// </summary>
public record ListBlock : DocumentBlock
{
    /// <summary>
    /// Whether the list uses numbered (ordered) items.
    /// </summary>
    public bool IsOrdered { get; init; }

    /// <summary>
    /// The list items contained in this list.
    /// </summary>
    public IReadOnlyList<ListItemBlock> Items { get; init; } = [];
}
