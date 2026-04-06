// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// A heading that has been cleaned and normalized for consistent metadata usage.
/// </summary>
/// <param name="Id">Unique identifier for this heading entry.</param>
/// <param name="Text">Trimmed and whitespace-normalized heading text.</param>
/// <param name="Level">The heading level (1–6).</param>
/// <param name="OriginalText">The original, unmodified heading text.</param>
public record NormalizedHeading(string Id, string Text, int Level, string OriginalText);
