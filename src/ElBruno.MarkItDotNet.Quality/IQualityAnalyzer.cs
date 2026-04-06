// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Analyzes a <see cref="Document"/> and produces a <see cref="QualityReport"/>
/// describing extraction/chunking quality and recommending fallback actions.
/// </summary>
public interface IQualityAnalyzer
{
    /// <summary>
    /// Analyzes the given document and returns a quality report.
    /// </summary>
    /// <param name="document">The document to analyze.</param>
    /// <returns>A quality report with scores, issues, and a suggested action.</returns>
    QualityReport Analyze(Document document);
}
