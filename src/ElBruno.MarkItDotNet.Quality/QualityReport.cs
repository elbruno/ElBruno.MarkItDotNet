// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Complete quality analysis report for a document, including an overall score,
/// individual issues, computed metrics, and a suggested action.
/// </summary>
/// <param name="OverallScore">Weighted overall quality score (0.0 = worst, 1.0 = best).</param>
/// <param name="Issues">List of quality issues detected.</param>
/// <param name="Metrics">Computed quality metrics for the document.</param>
/// <param name="SuggestedAction">Recommended action based on analysis results.</param>
public record QualityReport(
    double OverallScore,
    IReadOnlyList<QualityIssue> Issues,
    QualityMetrics Metrics,
    QualityAction SuggestedAction);
