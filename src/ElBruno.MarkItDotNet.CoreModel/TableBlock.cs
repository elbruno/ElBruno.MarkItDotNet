// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A table block with column headers and data rows.
/// </summary>
public record TableBlock : DocumentBlock
{
    /// <summary>
    /// Column headers for the table.
    /// </summary>
    public IReadOnlyList<string> Headers { get; init; } = [];

    /// <summary>
    /// Data rows, where each row is a list of cell values.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = [];
}
