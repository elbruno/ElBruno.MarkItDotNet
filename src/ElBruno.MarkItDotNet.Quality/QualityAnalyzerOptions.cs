// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Configuration options for the <see cref="DocumentQualityAnalyzer"/>, including
/// metric weights for overall scoring and thresholds for suggested actions.
/// </summary>
public class QualityAnalyzerOptions
{
    /// <summary>Weight for text density in the overall score (default 0.20).</summary>
    public double TextDensityWeight { get; set; } = 0.20;

    /// <summary>Weight for OCR suspicion score in the overall score (default 0.25).</summary>
    public double OcrSuspicionWeight { get; set; } = 0.25;

    /// <summary>Weight for duplicate line ratio in the overall score (default 0.15).</summary>
    public double DuplicateLineWeight { get; set; } = 0.15;

    /// <summary>Weight for empty block ratio in the overall score (default 0.10).</summary>
    public double EmptyBlockWeight { get; set; } = 0.10;

    /// <summary>Weight for reading order score in the overall score (default 0.15).</summary>
    public double ReadingOrderWeight { get; set; } = 0.15;

    /// <summary>Weight for heading consistency score in the overall score (default 0.15).</summary>
    public double HeadingConsistencyWeight { get; set; } = 0.15;

    /// <summary>Overall score at or below which the suggested action is <see cref="QualityAction.Reject"/> (default 0.2).</summary>
    public double RejectThreshold { get; set; } = 0.2;

    /// <summary>Overall score at or below which the suggested action is <see cref="QualityAction.FallbackToDocumentIntelligence"/> (default 0.4).</summary>
    public double FallbackToDocumentIntelligenceThreshold { get; set; } = 0.4;

    /// <summary>Overall score at or below which the suggested action is <see cref="QualityAction.FallbackToOcr"/> (default 0.55).</summary>
    public double FallbackToOcrThreshold { get; set; } = 0.55;

    /// <summary>Overall score at or below which the suggested action is <see cref="QualityAction.Review"/> (default 0.75).</summary>
    public double ReviewThreshold { get; set; } = 0.75;

    /// <summary>Minimum number of words in a paragraph to consider it text-rich (default 10).</summary>
    public int MinWordsForTextRich { get; set; } = 10;

    /// <summary>Ratio of special characters that triggers OCR suspicion for a word (default 0.5).</summary>
    public double SpecialCharRatioThreshold { get; set; } = 0.5;
}
