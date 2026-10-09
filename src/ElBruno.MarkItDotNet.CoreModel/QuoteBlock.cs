// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// A block quote containing nested document blocks.
/// </summary>
public record QuoteBlock : DocumentBlock
{
    /// <summary>
    /// The blocks contained in the quote.
    /// </summary>
    public IReadOnlyList<DocumentBlock> Blocks { get; init; } = [];
}
