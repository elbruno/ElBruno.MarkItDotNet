// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Quantitative metrics computed during quality analysis of a document.
/// </summary>
/// <param name="TextDensity">Ratio of text-rich paragraphs to total blocks (0.0–1.0).</param>
/// <param name="AverageBlockLength">Average character length of text-bearing blocks.</param>
/// <param name="EmptyBlockRatio">Ratio of empty/whitespace-only blocks to total blocks (0.0–1.0).</param>
/// <param name="DuplicateLineRatio">Ratio of duplicate paragraph lines to total paragraphs (0.0–1.0).</param>
/// <param name="TableWarningCount">Number of table-level warnings (empty cells, mismatched rows, etc.).</param>
/// <param name="OcrSuspicionScore">Score indicating likelihood of garbled OCR text (0.0–1.0).</param>
/// <param name="ReadingOrderScore">Score for heading-level consistency and reading order (0.0–1.0).</param>
/// <param name="HeadingConsistencyScore">Score for heading capitalization consistency (0.0–1.0).</param>
public record QualityMetrics(
    double TextDensity,
    double AverageBlockLength,
    double EmptyBlockRatio,
    double DuplicateLineRatio,
    int TableWarningCount,
    double OcrSuspicionScore,
    double ReadingOrderScore,
    double HeadingConsistencyScore);
